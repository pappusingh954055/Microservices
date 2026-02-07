using Inventory.Application.Clients;
using Inventory.Application.SaleOrders.DTOs;
using Inventory.Application.SaleOrders.SaleReturn.DTOs;
using Inventory.Domain.Entities;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Repositories
{
    public class SaleReturnRepository : ISaleReturnRepository
    {
        private readonly InventoryDbContext _context;
        private readonly ICustomerClient _customerClient;

        public SaleReturnRepository(InventoryDbContext context, ICustomerClient customerClient
            )
        {
            _context = context;
            _customerClient = customerClient;
        }

        public async Task<SaleReturnPagedResponse> GetSaleReturnsAsync(
         string? search,
         string? status,
         int pageIndex,
         int pageSize,
         DateTime? fromDate,
         DateTime? toDate,
         string sortField,
         string sortOrder)
        {
            // AsNoTracking performance ke liye zaroori hai
            var query = _context.SaleReturnHeaders.AsNoTracking().AsQueryable();

            // 1. Optimized Date Range Filters (SARGable)
            if (fromDate.HasValue)
                query = query.Where(x => x.ReturnDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(x => x.ReturnDate <= toDate.Value);

            // 2. Optimized Status Widget Filter (Range check for TODAY performance)
            if (!string.IsNullOrEmpty(status))
            {
                if (status == "TODAY")
                {
                    var today = DateTime.Today;
                    var tomorrow = today.AddDays(1);
                    query = query.Where(x => x.ReturnDate >= today && x.ReturnDate < tomorrow);
                }
                else
                {
                    query = query.Where(x => x.Status == status);
                }
            }

            // 3. Global Search Fix: ReturnNo + SO Ref + Optimized Customer Name Lookup
            if (!string.IsNullOrEmpty(search))
            {
                // STEP: Naye optimized Microservice endpoint ko call karna [cite: 2026-02-06]
                // Yeh memory mein filter nahi karega, seedha matching IDs laayega
                var matchingCustomerIds = await _customerClient.SearchCustomerIdsByNameAsync(search);

                query = query.Where(x => x.ReturnNumber.Contains(search) ||
                                         (x.SaleOrder != null && x.SaleOrder.SONumber.Contains(search)) ||
                                         (matchingCustomerIds != null && matchingCustomerIds.Contains(x.CustomerId)));
            }

            // 4. Server-Side Sorting Fix: Added Date, SO Ref, and Customer sorting cases
            bool isDesc = sortOrder?.ToLower() == "desc";
            query = sortField?.ToLower() switch
            {
                "returnnumber" => isDesc ? query.OrderByDescending(x => x.ReturnNumber) : query.OrderBy(x => x.ReturnNumber),
                "returndate" => isDesc ? query.OrderByDescending(x => x.ReturnDate) : query.OrderBy(x => x.ReturnDate),
                "totalamount" => isDesc ? query.OrderByDescending(x => x.TotalAmount) : query.OrderBy(x => x.TotalAmount),
                "status" => isDesc ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
                "soref" => isDesc ? query.OrderByDescending(x => x.SaleOrder.SONumber) : query.OrderBy(x => x.SaleOrder.SONumber),
                // Customer Name remote hai, isliye ID par sorting proxy karte hain [cite: 2026-02-06]
                "customername" => isDesc ? query.OrderByDescending(x => x.CustomerId) : query.OrderBy(x => x.CustomerId),
                _ => query.OrderByDescending(x => x.ReturnDate)
            };

            // Calculate count before Skip/Take for accuracy
            var totalCount = await query.CountAsync();

            // 5. Execution & Projection
            var items = await query
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .Select(x => new SaleReturnListDto
                {
                    SaleReturnHeaderId = x.SaleReturnHeaderId,
                    ReturnNumber = x.ReturnNumber,
                    ReturnDate = x.ReturnDate,
                    CustomerId = x.CustomerId,
                    SoRef = x.SaleOrder != null ? x.SaleOrder.SONumber : string.Empty,
                    TotalAmount = x.TotalAmount,
                    Status = x.Status
                }).ToListAsync();

            // 6. Microservice Mapping logic (External Name Binding for Display) [cite: 2026-02-06]
            var customerIds = items.Select(i => i.CustomerId).Distinct().ToList();
            if (customerIds.Any())
            {
                var customerMap = await _customerClient.GetCustomerNamesAsync(customerIds);
                foreach (var item in items)
                {
                    item.CustomerName = customerMap != null && customerMap.ContainsKey(item.CustomerId)
                                        ? customerMap[item.CustomerId]
                                        : "Unknown Customer";
                }
            }

            return new SaleReturnPagedResponse { Items = items, TotalCount = totalCount };
        }



        public async Task<bool> CreateSaleReturnAsync(SaleReturnHeader header)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {

                decimal calculatedSubTotal = 0;
                decimal calculatedTaxAmount = 0;


                foreach (var item in header.ReturnItems)
                {

                    decimal itemSubTotal = item.ReturnQty * item.UnitPrice;
                    decimal itemTax = itemSubTotal * (item.TaxPercentage / 100m);


                    item.TaxAmount = itemTax;
                    item.TotalAmount = itemSubTotal + itemTax;
                    item.CreatedOn = DateTime.Now;


                    calculatedSubTotal += itemSubTotal;
                    calculatedTaxAmount += itemTax;


                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        // Sales Return = Increase Current Stock
                        product.CurrentStock += item.ReturnQty;
                        product.ModifiedOn = DateTime.Now;
                        product.ModifiedBy = header.CreatedBy ?? "system";
                    }
                }

                // 3. Header table columns update (0.00 fix karne ke liye)
                header.SubTotal = calculatedSubTotal;
                header.TaxAmount = calculatedTaxAmount;
                header.DiscountAmount = header.DiscountAmount;
                // TotalAmount final sync
                header.TotalAmount = calculatedSubTotal + calculatedTaxAmount - (header.DiscountAmount);
                header.CreatedOn = DateTime.Now;

                // 4. Save Sale Return
                _context.SaleReturnHeaders.Add(header);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<decimal> GetRemainingReturnableQtyAsync(int saleOrderId, Guid productId)
        {
            // 1. Sale Order mein kitni quantity bechi gayi thi (e.g., 6)
            var totalSold = await _context.SaleOrderItems
                .AsNoTracking()
                .Where(soi => soi.SaleOrderId == saleOrderId && soi.ProductId == productId)
                .Select(soi => soi.Qty)
                .FirstOrDefaultAsync();


            var totalReturned = await _context.SaleReturnItems
                .AsNoTracking()
                .Where(sri => sri.SaleReturnHeader.SaleOrderId == saleOrderId &&
                              sri.ProductId == productId &&
                              sri.SaleReturnHeader.Status == "Confirmed")
                .SumAsync(sri => (decimal?)sri.ReturnQty) ?? 0;

            // 3. Calculation (6 - 2 = 4 pieces available for return)
            // Agar result 0 se niche jaye, toh 0 hi return karein taaki UI minus na dikhaye
            var remaining = totalSold - totalReturned;

            return remaining > 0 ? remaining : 0;
        }

        public async Task<List<SaleReturnExportDto>> GetExportDataAsync(DateTime? fromDate, DateTime? toDate)
        {
            return await _context.SaleReturnHeaders
                .AsNoTracking()
                .Include(h => h.SaleOrder) // Join for SONumber
                .Where(h => (!fromDate.HasValue || h.ReturnDate >= fromDate) &&
                            (!toDate.HasValue || h.ReturnDate <= toDate))
                .Select(h => new SaleReturnExportDto
                {
                    ReturnNumber = h.ReturnNumber,
                    ReturnDate = h.ReturnDate.ToString("dd-MM-yyyy"),
                    SONumber = h.SaleOrder.SONumber ?? "N/A", // From SaleOrders table
                    TotalAmount = h.TotalAmount, //
                    Status = h.Status,
                    // CustomerId hum bad mein name se replace karenge
                    CustomerName = h.CustomerId.ToString()
                })
                .ToListAsync();
        }

        public async Task<SaleReturnSummaryDto> GetDashboardSummaryAsync()
        {
            var today = DateTime.Today;

            // 1. Aaj kitne returns aaye
            var totalToday = await _context.SaleReturnHeaders
                .CountAsync(x => x.ReturnDate.Date == today);

            // 2. Confirmed returns ka count aur refund value
            var confirmedData = await _context.SaleReturnHeaders
                .Where(x => x.Status == "CONFIRMED")
                .Select(x => x.TotalAmount)
                .ToListAsync();

            // 3. Stock re-filled pcs (Items table se sum)
            var totalPcs = await _context.SaleReturnItems
                .SumAsync(x => x.ReturnQty);

            return new SaleReturnSummaryDto
            {
                TotalReturnsToday = totalToday,
                TotalRefundValue = confirmedData.Sum(), // ₹5,062.20 logic
                ConfirmedReturns = confirmedData.Count, // 10 logic
                StockRefilledPcs = totalPcs
            };
        }

       
    }
}