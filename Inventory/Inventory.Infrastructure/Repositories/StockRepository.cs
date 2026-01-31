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



        //public async Task<StockPagedResponseDto> GetCurrentStockAsync(string search, string sortField, string sortOrder, int pageIndex, int pageSize)
        //{
        //    // 1. Base Query with Grouping and Contextual Data [cite: 2026-01-22]
        //    var query = _context.GRNDetails
        //        .GroupBy(g => new {
        //            // Grouping sirf Product level par, taaki stock calculation sahi rahe [cite: 2026-01-22]
        //            ProductId = g.ProductId,
        //            ProductName = g.Product.Name,
        //            UnitName = g.Product.Unit
        //        })
        //        .Select(group => new StockSummaryDto
        //        {
        //            ProductId = group.Key.ProductId,      // GUID [cite: 2026-01-22]
        //            ProductName = group.Key.ProductName,
        //            Unit = group.Key.UnitName,

        //            // Stock Calculation: Pure group ka total [cite: 2026-01-22]
        //            TotalReceived = group.Sum(x => x.ReceivedQty),

        //            // Last Rate: Group ke sabse latest record se [cite: 2026-01-22]
        //            LastRate = group.OrderByDescending(x => x.Id).Select(x => x.UnitRate).FirstOrDefault(),

        //            // PurchaseOrderId (Guid): Latest GRN ke header se [cite: 2026-01-22]
        //            LastPurchaseOrderId = group.OrderByDescending(x => x.Id)
        //                                       .Select(x => x.GRNHeader.PurchaseOrderId)
        //                                       .FirstOrDefault(),

        //            // SupplierId (int): Latest PO header se [cite: 2026-01-22]
        //            LastSupplierId = group.OrderByDescending(x => x.Id)
        //                                  .Select(x => x.GRNHeader.PurchaseOrder.SupplierId)
        //                                  .FirstOrDefault()
        //        });

        //    // 2. Searching logic [cite: 2026-01-22]
        //    if (!string.IsNullOrEmpty(search))
        //    {
        //        query = query.Where(x => x.ProductName.Contains(search));
        //    }

        //    // 3. Dynamic Sorting [cite: 2026-01-22]
        //    if (!string.IsNullOrEmpty(sortField))
        //    {
        //        bool isDesc = sortOrder?.ToLower() == "desc";

        //        query = sortField.ToLower() switch
        //        {
        //            "productname" => isDesc ? query.OrderByDescending(x => x.ProductName) : query.OrderBy(x => x.ProductName),
        //            "totalreceived" => isDesc ? query.OrderByDescending(x => x.TotalReceived) : query.OrderBy(x => x.TotalReceived),
        //            "unitrate" => isDesc ? query.OrderByDescending(x => x.LastRate) : query.OrderBy(x => x.LastRate),
        //            _ => query.OrderBy(x => x.ProductName)
        //        };
        //    }
        //    else
        //    {
        //        query = query.OrderBy(x => x.ProductName);
        //    }

        //    // 4. Execution with Pagination [cite: 2026-01-22]
        //    var totalCount = await query.CountAsync();

        //    var items = await query
        //        .Skip(pageIndex * pageSize)
        //        .Take(pageSize)
        //        .ToListAsync();

        //    return new StockPagedResponseDto
        //    {
        //        Items = items,
        //        TotalCount = totalCount
        //    };
        //}

        public Task<StockRefillDetailsDto> GetRefillDetailsAsync(Guid productId)
        {
            throw new NotImplementedException();
        }

        //public async Task<StockPagedResponseDto> GetCurrentStockAsync(string search, string sortField, string sortOrder, int pageIndex, int pageSize)
        //{
        //    // 1. Optimized Base Query
        //    var query = _context.GRNDetails
        //        .GroupBy(g => new
        //        {
        //            ProductId = g.ProductId,
        //            ProductName = g.Product.Name,
        //            UnitName = g.Product.Unit,
        //            // Header se Supplier aur PO ki info grouping mein hi le lete hain taaki baar-baar join na ho
        //            MinStock = g.Product.MinStock
        //        })
        //        .Select(group => new StockSummaryDto
        //        {
        //            ProductId = group.Key.ProductId,
        //            ProductName = group.Key.ProductName,
        //            Unit = group.Key.UnitName,
        //            MinStockLevel = group.Key.MinStock,

        //            // ACTUAL STOCK: Received minus Sold (Agar Sale table hai toh yahan minus logic aayega)
        //            TotalReceived = group.Sum(x => x.ReceivedQty),

        //            // AvailableStock abhi ke liye TotalReceived hi hai jab tak Sales table nahi judti
        //            AvailableStock = group.Sum(x => x.ReceivedQty),

        //            // Latest Price: Latest entry se uthayenge
        //            LastRate = group.OrderByDescending(x => x.Id).Select(x => x.UnitRate).FirstOrDefault(),

        //            // Reference IDs: Naya PO banane ke liye zaroori hain
        //            LastPurchaseOrderId = group.OrderByDescending(x => x.Id).Select(x => x.GRNHeader.PurchaseOrderId).FirstOrDefault(),
        //            LastSupplierId = group.OrderByDescending(x => x.Id).Select(x => x.GRNHeader.PurchaseOrder.SupplierId).FirstOrDefault()
        //        });

        //    // 2. Search Logic (Same rahega)
        //    if (!string.IsNullOrEmpty(search))
        //    {
        //        query = query.Where(x => x.ProductName.Contains(search));
        //    }

        //    // 3. Dynamic Sorting
        //    bool isDesc = sortOrder?.ToLower() == "desc";
        //    query = sortField?.ToLower() switch
        //    {
        //        "productname" => isDesc ? query.OrderByDescending(x => x.ProductName) : query.OrderBy(x => x.ProductName),
        //        "availablestock" => isDesc ? query.OrderByDescending(x => x.AvailableStock) : query.OrderBy(x => x.AvailableStock),
        //        "unitrate" => isDesc ? query.OrderByDescending(x => x.LastRate) : query.OrderBy(x => x.LastRate),
        //        _ => query.OrderBy(x => x.ProductName)
        //    };

        //    // 4. Execution
        //    var totalCount = await query.CountAsync();
        //    var items = await query.Skip(pageIndex * pageSize).Take(pageSize).ToListAsync();

        //    return new StockPagedResponseDto { Items = items, TotalCount = totalCount };
        //}

        public async Task<StockPagedResponseDto> GetCurrentStockAsync(string search, string sortField, string sortOrder, int pageIndex, int pageSize)
        {
            // 1. Optimized Base Query with Rejection Logic
            var query = _context.GRNDetails
                .GroupBy(g => new
                {
                    ProductId = g.ProductId,
                    ProductName = g.Product.Name,
                    UnitName = g.Product.Unit,
                    MinStock = g.Product.MinStock
                })
                .Select(group => new StockSummaryDto
                {
                    ProductId = group.Key.ProductId,
                    ProductName = group.Key.ProductName,
                    Unit = group.Key.UnitName,
                    MinStockLevel = group.Key.MinStock,

                    // Gate par total kitna maal utra [cite: 2026-01-31]
                    TotalReceived = group.Sum(x => x.ReceivedQty),

                    // TOTAL REJECTED: Jitna maal damage record kiya gaya [cite: 2026-01-31]
                    TotalRejected = group.Sum(x => x.RejectedQty),

                    // AVAILABLE STOCK: Formula badal kar (Received - Rejected) kar diya hai [cite: 2026-01-31]
                    // Note: Jab Sales table judegi toh yahan se '- SoldQty' bhi minus hoga.
                    AvailableStock = group.Sum(x => x.ReceivedQty) - group.Sum(x => x.RejectedQty),

                    // Latest Price: Latest entry se uthayenge
                    LastRate = group.OrderByDescending(x => x.Id).Select(x => x.UnitRate).FirstOrDefault(),

                    // Reference IDs: Naya PO banane ke liye zaroori hain
                    LastPurchaseOrderId = group.OrderByDescending(x => x.Id).Select(x => x.GRNHeader.PurchaseOrderId).FirstOrDefault(),
                    LastSupplierId = group.OrderByDescending(x => x.Id).Select(x => x.GRNHeader.PurchaseOrder.SupplierId).FirstOrDefault()
                });

            // 2. Search Logic
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => x.ProductName.Contains(search));
            }

            // 3. Dynamic Sorting
            bool isDesc = sortOrder?.ToLower() == "desc";
            query = sortField?.ToLower() switch
            {
                "productname" => isDesc ? query.OrderByDescending(x => x.ProductName) : query.OrderBy(x => x.ProductName),
                "availablestock" => isDesc ? query.OrderByDescending(x => x.AvailableStock) : query.OrderBy(x => x.AvailableStock),
                "totalrejected" => isDesc ? query.OrderByDescending(x => x.TotalRejected) : query.OrderBy(x => x.TotalRejected), // Naya sorting option
                "unitrate" => isDesc ? query.OrderByDescending(x => x.LastRate) : query.OrderBy(x => x.LastRate),
                _ => query.OrderBy(x => x.ProductName)
            };

            // 4. Execution
            var totalCount = await query.CountAsync();
            var items = await query.Skip(pageIndex * pageSize).Take(pageSize).ToListAsync();

            return new StockPagedResponseDto { Items = items, TotalCount = totalCount };
        }
    }
}
