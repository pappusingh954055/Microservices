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
    private IDbContextTransaction? _transaction; // Class variable, not in constructor
    private readonly HttpClient _httpClient;
    // Constructor fix: Sirf DbContext inject karein
    public SaleOrderRepository(InventoryDbContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
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
    public async Task<List<StockExportDto>> GetStockReportDataAsync(List<Guid> productIds)
    {
        return await _context.GRNDetails
            .Where(g => productIds.Contains(g.ProductId))
            .GroupBy(g => new { g.ProductId, g.Product.Name, g.Product.Unit })
            .Select(group => new StockExportDto
            {
                ProductName = group.Key.Name,
                Unit = group.Key.Unit,
                TotalReceived = group.Sum(x => x.ReceivedQty),
                TotalRejected = group.Sum(x => x.RejectedQty),
                // Available Stock = (Received - Rejected) - Confirmed Sales
                AvailableStock = (group.Sum(x => x.ReceivedQty) - group.Sum(x => x.RejectedQty)) -
                                 (_context.SaleOrderItems
                                    .Where(si => si.ProductId == group.Key.ProductId && si.SaleOrder.Status == "Confirmed")
                                    .Sum(si => (decimal?)si.Qty) ?? 0)
            }).ToListAsync();
    }

    public async Task<List<SaleOrderListDto>> GetAllSaleOrdersAsync()
    {
        // 1. Pehle SaleOrders fetch karein (Bina Customer Join ke)
        var orders = await _context.SaleOrders
            .OrderByDescending(o => o.SODate)
            .Select(o => new SaleOrderListDto
            {
                Id = o.Id,
                SoNumber = o.SONumber,
                SoDate = o.SODate,
                CustomerId = o.CustomerId, // Hume sirf ID milegi DB se
                Status = o.Status,
                GrandTotal = o.GrandTotal,
                CustomerName = "Loading..." // Initial placeholder
            })
            .ToListAsync();

        if (!orders.Any()) return orders;

        // 2. Unique Customer IDs nikaalein taaki duplicate API calls na ho
        var customerIds = orders.Select(o => o.CustomerId).Distinct().ToList();

        // 3. Customer Microservice se Names fetch karein
        // Note: Aapka Customer service ek aisa endpoint provide kare jo List of IDs lekar Names de
        var customerDictionary = await GetCustomerNamesFromService(customerIds);

        // 4. Names ko Map karein
        foreach (var order in orders)
        {
            if (customerDictionary.TryGetValue(order.CustomerId, out var name))
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

    private async Task<Dictionary<int, string>> GetCustomerNamesFromService(List<int> ids)
    {
        try
        {
            // Example: GET /api/customers/names?ids=1&ids=2
            var response = await _httpClient.PostAsJsonAsync("api/customers/get-names", ids);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Dictionary<int, string>>() ?? new();
            }
        }
        catch (Exception ex)
        {
            // Log error
        }
        return new Dictionary<int, string>();
    }
}