using Inventory.Application.Common.Interfaces;
using Inventory.Application.SaleOrders.DTOs;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using YourProjectNamespace.Entities;

public class SaleOrderRepository : ISaleOrderRepository
{
    private readonly InventoryDbContext _context;
    private IDbContextTransaction? _transaction; 
    private readonly HttpClient _httpClient;
   
    public SaleOrderRepository(InventoryDbContext context, 
        IHttpClientFactory httpClientFactory)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
       
        if (httpClientFactory == null) throw new ArgumentNullException(nameof(httpClientFactory));

        _httpClient = httpClientFactory.CreateClient("CustomerService");
    }

    // BeginTransactionAsync logic fix
    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    // Guid aur Decimal support ke saath methods
    public async Task<decimal> GetAvailableStockAsync(Guid productId)
    {
        return await _context.Products
            .Where(p => p.Id == productId)
            .Select(p => p.CurrentStock)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateProductStockAsync(Guid productId, decimal adjustmentQty)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product != null)
        {
            product.CurrentStock += adjustmentQty;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string> GetLastSONumberAsync() =>
        await _context.SaleOrders.OrderByDescending(x => x.Id).Select(x => x.SONumber).FirstOrDefaultAsync();

    public async Task<int> SaveAsync(SaleOrder order)
    {
        _context.SaleOrders.Add(order);
        await _context.SaveChangesAsync();
        return order.Id;
    }
    public async Task<List<StockExportDto>> GetSaleReportDataAsync(List<int> orderIds) // logic according to integer IDs
    {
        // Selected Orders ke Product IDs fetch karein
        return await _context.SaleOrderItems
            .Where(si => orderIds.Contains(si.SaleOrderId)) // Filter by selected integer IDs
            .GroupBy(si => new { si.ProductId, si.ProductName, si.Unit })
            .Select(group => new StockExportDto
            {
                ProductName = group.Key.ProductName,
                Unit = group.Key.Unit,
                TotalReceived = _context.GRNDetails.Where(g => g.ProductId == group.Key.ProductId).Sum(x => x.ReceivedQty),
                TotalRejected = _context.GRNDetails.Where(g => g.ProductId == group.Key.ProductId).Sum(x => x.RejectedQty),
                AvailableStock = (_context.GRNDetails.Where(g => g.ProductId == group.Key.ProductId).Sum(x => x.ReceivedQty) -
                                  _context.GRNDetails.Where(g => g.ProductId == group.Key.ProductId).Sum(x => x.RejectedQty)) -
                                 (_context.SaleOrderItems.Where(si => si.ProductId == group.Key.ProductId && si.SaleOrder.Status == "Confirmed").Sum(si => (decimal?)si.Qty) ?? 0)
            }).ToListAsync();
    }

    public async Task<List<SaleOrderListDto>> GetAllSaleOrdersAsync()
    {
        // 1. Database se SaleOrders fetch karein
        var orders = await _context.SaleOrders
            .OrderByDescending(o => o.SODate)
            .Select(o => new SaleOrderListDto
            {
                Id = o.Id,
                SoNumber = o.SONumber,
                SoDate = o.SODate,
                CustomerId = o.CustomerId,
                Status = o.Status,
                GrandTotal = o.GrandTotal,
                CustomerName = "Loading..."
            })
            .ToListAsync();

        if (orders == null || !orders.Any()) return new List<SaleOrderListDto>();

        // 2. Unique Customer IDs ki list taiyar karein
        var customerIds = orders.Select(o => o.CustomerId).Distinct().ToList();

        // 3. Customer Microservice se data fetch karein
        var customerDictionary = await GetCustomerNamesFromService(customerIds);

        // 4. Dictionary se names map karein
        foreach (var order in orders)
        {
            if (customerDictionary != null && customerDictionary.TryGetValue(order.CustomerId, out var name))
            {
                order.CustomerName = name;
            }
            else
            {
                order.CustomerName = "Unknown Customer";
            }
        }

        return orders;
    }

    // Helper method jo actual Microservice call handle karega
    private async Task<Dictionary<int, string>> GetCustomerNamesFromService(List<int> customerIds)
    {
        try
        {
            // Note: URL wahi hona chahiye jo Customers.API ke controller mein defined hai
            var response = await _httpClient.PostAsJsonAsync("api/customers/get-names", customerIds);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<Dictionary<int, string>>();
                return data ?? new Dictionary<int, string>();
            }
        }
        catch (Exception ex)
        {
            // Agar Microservice band hai toh crash na ho
            Console.WriteLine($"Microservice call failed: {ex.Message}");
        }

        return new Dictionary<int, string>();
    }



    public async Task<bool> UpdateSaleOrderStatusAsync(int id, string status)
    {
        // 1. Pehle Order fetch karein
        var order = await _context.SaleOrders.FindAsync(id);
        if (order == null) return false;

        // 2. Agar status 'Confirmed' ho raha hai aur pehle se nahi tha
        if (status == "Confirmed" && order.Status != "Confirmed")
        {
            // Order ke saare items nikaalein
            var items = await _context.SaleOrderItems
                                      .Where(x => x.SaleOrderId == id)
                                      .ToListAsync();

            foreach (var item in items)
            {
                // Product table se stock kam karein
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    // Current stock mein se order qty minus kar dein
                    product.CurrentStock -= item.Qty;
                }
            }
        }

        // 3. Status update karein aur save karein
        order.Status = status;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<SaleOrderDetailDto?> GetSaleOrderByIdAsync(int id)
    {
        // 1. Database se Order aur uske Items fetch karein
        var order = await _context.SaleOrders
      
            .Include(o => o.Items)
            .Where(o => o.Id == id)
            .Select(o => new SaleOrderDetailDto
            {
                Id = o.Id,
                SoNumber = o.SONumber,
                SoDate = o.SODate,
                CustomerId = o.CustomerId,
                Status = o.Status,
                GrandTotal = o.GrandTotal,
                // Items ki mapping yahan karein
                Items = o.Items.Select(oi => new SaleOrderItemDto
                {
                    ProductId = oi.ProductId,
                    // ProductName direct column se aa jayega
                    ProductName = oi.ProductName,
                    Qty = oi.Qty,
                    Rate = oi.Rate,
                    Total = oi.Qty * oi.Rate
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (order == null) return null;

        // 2. Customer Name fetch karein Microservice se
        try
        {
            // Single customer name fetch call
            var response = await _httpClient.GetAsync($"api/customers/{order.CustomerId}/name");
            if (response.IsSuccessStatusCode)
            {
                order.CustomerName = await response.Content.ReadAsStringAsync();
            }
        }
        catch
        {
            order.CustomerName = "Name Fetch Failed";
        }

        return order;
    }
}