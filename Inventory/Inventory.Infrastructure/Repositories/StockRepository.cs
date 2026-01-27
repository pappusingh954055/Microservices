using Inventory.Application.Common.Interfaces;
using Inventory.Application.GRN.DTOs.Stock;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Repositories
{
    public class StockRepository : IStockRepository
    {
        private readonly InventoryDbContext _context;

        public StockRepository(InventoryDbContext context)
        {
            _context = context;
        }

        //public async Task<IEnumerable<StockSummaryDto>> GetCurrentStockAsync()
        //{
        //    return await _context.GRNDetails
        //        .GroupBy(g => new {
        //            ProductName = g.Product.Name,
        //            // Hum Unit ko PurchaseOrderItems ya direct Product se link karenge
        //            // Agar Product table mein Unit hai toh g.Product.Unit use karein
        //            UnitName = g.Product.Unit // Maan lete hain Product table mein Unit hai
        //        })
        //        .Select(group => new StockSummaryDto
        //        {
        //            ProductName = group.Key.ProductName,
        //            // Yahan string conversion ki zaroorat nahi agar Unit string hai
        //            Unit = group.Key.UnitName,
        //            TotalReceived = group.Sum(x => x.ReceivedQty),
        //            LastRate = group.OrderByDescending(x => x.Id)
        //                            .Select(x => x.UnitRate)
        //                            .FirstOrDefault()
        //        })
        //        .ToListAsync();
        //}

        public async Task<StockPagedResponseDto> GetCurrentStockAsync(string search, string sortField, string sortOrder, int pageIndex, int pageSize)
        {
            // 1. Base Query (Projection pehle hi karni hogi)
            var query = _context.GRNDetails
                .GroupBy(g => new {
                    ProductName = g.Product.Name,
                    UnitName = g.Product.Unit
                })
                .Select(group => new StockSummaryDto
                {
                    ProductName = group.Key.ProductName,
                    Unit = group.Key.UnitName,
                    TotalReceived = group.Sum(x => x.ReceivedQty),
                    // LastRate logic same rakhi hai
                    LastRate = group.OrderByDescending(x => x.Id).Select(x => x.UnitRate).FirstOrDefault()
                });

            // 2. Searching [cite: 2026-01-22]
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => x.ProductName.Contains(search));
            }

            // 3. Fixed Dynamic Sorting
            // EF.Property ki jagah hum explicit property mapping use karenge
            if (!string.IsNullOrEmpty(sortField))
            {
                bool isDesc = sortOrder?.ToLower() == "desc";

                query = sortField.ToLower() switch
                {
                    "productname" => isDesc ? query.OrderByDescending(x => x.ProductName) : query.OrderBy(x => x.ProductName),
                    "totalreceived" => isDesc ? query.OrderByDescending(x => x.TotalReceived) : query.OrderBy(x => x.TotalReceived),
                    "unitrate" => isDesc ? query.OrderByDescending(x => x.LastRate) : query.OrderBy(x => x.LastRate),
                    _ => query.OrderBy(x => x.ProductName) // Default case
                };
            }
            else
            {
                query = query.OrderBy(x => x.ProductName);
            }

            // 4. Get Total Count [cite: 2026-01-22]
            var totalCount = await query.CountAsync();

            // 5. Pagination [cite: 2026-01-22]
            var items = await query
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new StockPagedResponseDto
            {
                Items = items,
                TotalCount = totalCount
            };
        }
    }
}
