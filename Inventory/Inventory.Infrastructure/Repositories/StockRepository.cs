using ClosedXML.Excel;
using Inventory.Application.Common.Interfaces;
using Inventory.Application.GRN.DTOs.Stock;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Linq;

namespace Inventory.Infrastructure.Repositories
{
    public class StockRepository : IStockRepository
    {
        private readonly InventoryDbContext _context;

        public StockRepository(InventoryDbContext context)
        {
            _context = context;
        }        

        public Task<StockRefillDetailsDto> GetRefillDetailsAsync(Guid productId)
        {
            throw new NotImplementedException();
        }



        //public async Task<StockPagedResponseDto> GetCurrentStockAsync(
        // string? search,
        // string? sortField,
        // string? sortOrder,
        // int pageIndex,
        // int pageSize,
        // DateTime? startDate = null,
        // DateTime? endDate = null)
        //    {
        //        // 1. Base Query on GRNDetails with Date Filters applied first for performance
        //        var baseQuery = _context.GRNDetails.AsQueryable();

        //        if (startDate.HasValue)
        //        {
        //            baseQuery = baseQuery.Where(x => x.GRNHeader.ReceivedDate >= startDate.Value);
        //        }
        //        if (endDate.HasValue)
        //        {
        //            baseQuery = baseQuery.Where(x => x.GRNHeader.ReceivedDate <= endDate.Value);
        //        }

        //        // 2. Optimized Grouping Logic
        //        var query = baseQuery
        //            .GroupBy(g => new
        //            {
        //                ProductId = g.ProductId,
        //                ProductName = g.Product.Name,
        //                UnitName = g.Product.Unit,
        //                MinStock = g.Product.MinStock
        //            })
        //            .Select(group => new StockSummaryDto
        //            {
        //                ProductId = group.Key.ProductId,
        //                ProductName = group.Key.ProductName,
        //                Unit = group.Key.UnitName,
        //                MinStockLevel = group.Key.MinStock,

        //                TotalReceived = group.Sum(x => x.ReceivedQty),
        //                TotalRejected = group.Sum(x => x.RejectedQty),
        //                AvailableStock = group.Sum(x => x.ReceivedQty) - group.Sum(x => x.RejectedQty),

        //                LastRate = group.OrderByDescending(x => x.Id).Select(x => x.UnitRate).FirstOrDefault(),
        //                LastPurchaseOrderId = group.OrderByDescending(x => x.Id).Select(x => x.GRNHeader.PurchaseOrderId).FirstOrDefault(),
        //                LastSupplierId = group.OrderByDescending(x => x.Id).Select(x => x.GRNHeader.PurchaseOrder.SupplierId).FirstOrDefault(),

        //                History = group.OrderByDescending(x => x.GRNHeader.ReceivedDate)
        //                               .SelectMany(h => _context.GRNDetails
        //                                   .Where(allG => allG.GRNHeaderId == h.GRNHeaderId)
        //                                   .Select(allG => new StockHistoryDto
        //                                   {
        //                                       ReceivedDate = allG.GRNHeader.ReceivedDate,
        //                                       PONumber = allG.GRNHeader.PurchaseOrder.PoNumber,
        //                                       SupplierName = allG.GRNHeader.PurchaseOrder.SupplierName,
        //                                       ProductName = allG.Product.Name,
        //                                       ReceivedQty = allG.ReceivedQty,
        //                                       RejectedQty = allG.RejectedQty
        //                                   })).ToList()
        //            });

        //        // 3. Search Logic
        //        if (!string.IsNullOrEmpty(search))
        //        {
        //            query = query.Where(x => x.ProductName.Contains(search));
        //        }

        //        // 4. FIXED Dynamic Sorting: Added 'totalreceived' case
        //        bool isDesc = sortOrder?.ToLower() == "desc";
        //        query = sortField?.ToLower() switch
        //        {
        //            "productname" => isDesc ? query.OrderByDescending(x => x.ProductName) : query.OrderBy(x => x.ProductName),
        //            "totalreceived" => isDesc ? query.OrderByDescending(x => x.TotalReceived) : query.OrderBy(x => x.TotalReceived), // Added Fix
        //            "availablestock" => isDesc ? query.OrderByDescending(x => x.AvailableStock) : query.OrderBy(x => x.AvailableStock),
        //            "totalrejected" => isDesc ? query.OrderByDescending(x => x.TotalRejected) : query.OrderBy(x => x.TotalRejected),
        //            "unitrate" => isDesc ? query.OrderByDescending(x => x.LastRate) : query.OrderBy(x => x.LastRate),
        //            _ => query.OrderBy(x => x.ProductName)
        //        };

        //        // 5. Final Execution with Pagination
        //        var totalCount = await query.CountAsync();
        //        var items = await query.Skip(pageIndex * pageSize).Take(pageSize).ToListAsync();

        //        return new StockPagedResponseDto
        //        {
        //            Items = items,
        //            TotalCount = totalCount
        //        };
        //    }

        //    public async Task<StockPagedResponseDto> GetCurrentStockAsync(
        //string? search,
        //string? sortField,
        //string? sortOrder,
        //int pageIndex,
        //int pageSize,
        //DateTime? startDate = null,
        //DateTime? endDate = null)
        //    {
        //        // STEP 1: Sirf Base GRN data grouping karein (Sales aur History ke bina)
        //        // Ye query ekdum light hai aur kabhi timeout nahi degi
        //        var baseQuery = _context.GRNDetails.AsQueryable();

        //        if (startDate.HasValue)
        //            baseQuery = baseQuery.Where(x => x.GRNHeader.ReceivedDate >= startDate.Value);
        //        if (endDate.HasValue)
        //            baseQuery = baseQuery.Where(x => x.GRNHeader.ReceivedDate <= endDate.Value);

        //        var groupedQuery = baseQuery
        //            .GroupBy(g => new
        //            {
        //                g.ProductId,
        //                ProductName = g.Product.Name,
        //                UnitName = g.Product.Unit,
        //                MinStock = g.Product.MinStock
        //            })
        //            .Select(group => new StockSummaryDto
        //            {
        //                ProductId = group.Key.ProductId,
        //                ProductName = group.Key.ProductName,
        //                Unit = group.Key.UnitName,
        //                MinStockLevel = group.Key.MinStock,
        //                TotalReceived = group.Sum(x => x.ReceivedQty),
        //                TotalRejected = group.Sum(x => x.RejectedQty),
        //                // Initial available stock (Sales minus karne se pehle)
        //                AvailableStock = group.Sum(x => x.ReceivedQty) - group.Sum(x => x.RejectedQty),
        //                LastRate = group.OrderByDescending(x => x.Id).Select(x => x.UnitRate).FirstOrDefault(),
        //                LastPurchaseOrderId = group.OrderByDescending(x => x.Id).Select(x => x.GRNHeader.PurchaseOrderId).FirstOrDefault()
        //            });

        //        // Search Logic
        //        if (!string.IsNullOrEmpty(search))
        //        {
        //            groupedQuery = groupedQuery.Where(x => x.ProductName.Contains(search));
        //        }

        //        // Sorting Logic
        //        bool isDesc = sortOrder?.ToLower() == "desc";
        //        groupedQuery = sortField?.ToLower() switch
        //        {
        //            "productname" => isDesc ? groupedQuery.OrderByDescending(x => x.ProductName) : groupedQuery.OrderBy(x => x.ProductName),
        //            "totalreceived" => isDesc ? groupedQuery.OrderByDescending(x => x.TotalReceived) : groupedQuery.OrderBy(x => x.TotalReceived),
        //            "availablestock" => isDesc ? groupedQuery.OrderByDescending(x => x.AvailableStock) : groupedQuery.OrderBy(x => x.AvailableStock),
        //            _ => groupedQuery.OrderBy(x => x.ProductName)
        //        };

        //        // STEP 2: Pagination execute karke sirf limited items (e.g., 10 items) layein
        //        var totalCount = await groupedQuery.CountAsync();
        //        var items = await groupedQuery.Skip(pageIndex * pageSize).Take(pageSize).ToListAsync();

        //        // STEP 3: Ab sirf in 10 items ke liye Sales aur History fetch karein
        //        // Ye loop sirf 10 baar chalega, isliye performance par koi asar nahi padega
        //        foreach (var item in items)
        //        {
        //            // 1. Calculate Total Sold for this product
        //            item.TotalSold = await _context.SaleOrderItems
        //                .Where(si => si.ProductId == item.ProductId && si.SaleOrder.Status == "Confirmed")
        //                .SumAsync(si => (decimal?)si.Qty) ?? 0;

        //            // 2. Final Stock Update
        //            item.AvailableStock = (item.TotalReceived - item.TotalRejected) - item.TotalSold;

        //            // 3. History fetch (Sirf is product ki specific history)
        //            item.History = await _context.GRNDetails
        //                .Where(g => g.ProductId == item.ProductId)
        //                .OrderByDescending(g => g.GRNHeader.ReceivedDate)
        //                .Take(15) // Sirf top 15 records dikhayein speed ke liye
        //                .Select(allG => new StockHistoryDto
        //                {
        //                    ReceivedDate = allG.GRNHeader.ReceivedDate,
        //                    PONumber = allG.GRNHeader.PurchaseOrder.PoNumber,
        //                    SupplierName = allG.GRNHeader.PurchaseOrder.SupplierName,
        //                    ProductName = allG.Product.Name,
        //                    ReceivedQty = allG.ReceivedQty,
        //                    RejectedQty = allG.RejectedQty
        //                }).ToListAsync();
        //        }

        //        return new StockPagedResponseDto
        //        {
        //            Items = items,
        //            TotalCount = totalCount
        //        };
        //    }

     public async Task<StockPagedResponseDto> GetCurrentStockAsync(
     string? search,
     string? sortField,
     string? sortOrder,
     int pageIndex,
     int pageSize,
     DateTime? startDate = null,
     DateTime? endDate = null)
        {
            // STEP 1: Base Query - Data ko fast fetch karne ke liye AsNoTracking [cite: 2026-02-04]
            var baseQuery = _context.GRNDetails.AsNoTracking().AsQueryable();

            if (startDate.HasValue)
                baseQuery = baseQuery.Where(x => x.GRNHeader.ReceivedDate >= startDate.Value);
            if (endDate.HasValue)
                baseQuery = baseQuery.Where(x => x.GRNHeader.ReceivedDate <= endDate.Value);

            // STEP 2: Grouping Logic - Product wise aggregate [cite: 2026-02-04]
            var groupedQuery = baseQuery
                .GroupBy(g => new
                {
                    g.ProductId,
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
                    // TotalReceived: Ye total kitna mal andar aaya hai (#8 for Test-001)
                    TotalReceived = group.Sum(x => x.ReceivedQty),
                    TotalRejected = group.Sum(x => x.RejectedQty),

                    // AvailableStock: Initial value same as TotalReceived [cite: 2026-02-04]
                    AvailableStock = group.Sum(x => x.ReceivedQty),

                    LastRate = group.OrderByDescending(x => x.Id).Select(x => x.UnitRate).FirstOrDefault(),
                    LastPurchaseOrderId = group.OrderByDescending(x => x.Id).Select(x => x.GRNHeader.PurchaseOrderId).FirstOrDefault()
                });

            // STEP 3: Search & Sorting [cite: 2026-02-04]
            if (!string.IsNullOrEmpty(search))
                groupedQuery = groupedQuery.Where(x => x.ProductName.Contains(search));

            bool isDesc = sortOrder?.ToLower() == "desc";
            groupedQuery = sortField?.ToLower() switch
            {
                "productname" => isDesc ? groupedQuery.OrderByDescending(x => x.ProductName) : groupedQuery.OrderBy(x => x.ProductName),
                "totalreceived" => isDesc ? groupedQuery.OrderByDescending(x => x.TotalReceived) : groupedQuery.OrderBy(x => x.TotalReceived),
                "availablestock" => isDesc ? groupedQuery.OrderByDescending(x => x.AvailableStock) : groupedQuery.OrderBy(x => x.AvailableStock),
                _ => groupedQuery.OrderBy(x => x.ProductName)
            };

            // Execution of count and paged items [cite: 2026-02-04]
            var totalCount = await groupedQuery.CountAsync();
            var items = await groupedQuery.Skip(pageIndex * pageSize).Take(pageSize).ToListAsync();

            // STEP 4: Real-Time Sales & Returns Fix [cite: 2026-02-04]
            foreach (var item in items)
            {
                // 1. Confirmed Sales fetch karein (Confirmed and Completed) [cite: 2026-02-04]
                // Ye wahi 2 units hain jo Sale Order mein minus ho rahi hain
                var totalSold = await _context.SaleOrderItems
                    .Where(si => si.ProductId == item.ProductId &&
                                (si.SaleOrder.Status == "Confirmed" || si.SaleOrder.Status == "Completed"))
                    .SumAsync(si => (decimal?)si.Qty) ?? 0;

                item.TotalSold = totalSold;

                // 2. Purchase Returns fetch karein (Debit Note) [cite: 2026-02-04]
                var totalReturned = await _context.PurchaseReturnItems
                    .Where(ri => ri.ProductId == item.ProductId)
                    .SumAsync(ri => (decimal?)ri.ReturnQty) ?? 0;

                item.TotalRejected = totalReturned; // Optional: Syncing rejected with returns

                // --- FINAL SYNC CALCULATION ---
                // Dashboard ab 8 - 2 = 6 hi dikhayega
                item.AvailableStock = item.TotalReceived - (item.TotalSold + totalReturned);

                // --- Audit Trail Logic ---
                item.History = await _context.GRNDetails
                    .Where(g => g.ProductId == item.ProductId)
                    .OrderByDescending(g => g.GRNHeader.ReceivedDate)
                    .Take(10)
                    .Select(allG => new StockHistoryDto
                    {
                        ReceivedDate = allG.GRNHeader.ReceivedDate,
                        PONumber = allG.GRNHeader.PurchaseOrder.PoNumber,
                        SupplierName = allG.GRNHeader.PurchaseOrder.SupplierName,
                        ProductName = allG.Product.Name,
                        ReceivedQty = allG.ReceivedQty,
                        RejectedQty = allG.RejectedQty
                    }).ToListAsync();
            }

            return new StockPagedResponseDto
            {
                Items = items,
                TotalCount = totalCount
            };
        }

        public async Task<byte[]> GenerateStockExcel(List<Guid> productIds)
        {
            var stockData = await _context.GRNDetails
                .Where(x => productIds.Contains(x.ProductId))
                .Include(x => x.Product)
                .GroupBy(x => new {
                    x.ProductId,
                    ProductName = x.Product.Name,
                    MinLevel = x.Product.MinStock
                })
                .Select(g => new {
                    ProductName = g.Key.ProductName,
                    TotalReceived = g.Sum(x => x.ReceivedQty),
                    TotalRejected = g.Sum(x => x.RejectedQty),
                    // Correct logic: 141 - 2 = 139
                    AvailableStock = g.Sum(x => x.ReceivedQty) - g.Sum(x => x.RejectedQty),
                    // Latest Rate: ₹1,650.00
                    LastRate = g.OrderByDescending(x => x.Id).Select(x => x.UnitRate).FirstOrDefault(),
                    MinStockLevel = g.Key.MinLevel
                })
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Current Stock");

                // 1. Header Styling
                string[] headers = { "Product Name", "Total Received", "Rejected", "Current Stock", "Value (Avg)", "Total Value" };
                var headerRow = worksheet.Row(1);
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = headerRow.Cell(i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.FontColor = XLColor.White;
                    // Background Fix: SetBackgroundColor aur Pattern Solid use karein
                    cell.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#3f51b5"));
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                int row = 2;
                foreach (var item in stockData)
                {
                    worksheet.Cell(row, 1).Value = item.ProductName;
                    worksheet.Cell(row, 2).Value = item.TotalReceived;
                    worksheet.Cell(row, 3).Value = item.TotalRejected;

                    var stockCell = worksheet.Cell(row, 4);
                    stockCell.Value = item.AvailableStock;

                    // 2. RED COLOR LOGIC: Agar stock MinLevel se kam hai
                    if (item.AvailableStock <= item.MinStockLevel)
                    {
                        stockCell.Style.Font.SetFontColor(XLColor.Red);
                        stockCell.Style.Font.Bold = true;
                    }

                    // Rate Column
                    var rateCell = worksheet.Cell(row, 5);
                    rateCell.Value = item.LastRate;
                    rateCell.Style.NumberFormat.Format = "₹ #,##0.00";

                    // Total Value Calculation
                    var totalValCell = worksheet.Cell(row, 6);
                    totalValCell.FormulaA1 = $"=D{row}*E{row}";
                    totalValCell.Style.NumberFormat.Format = "₹ #,##0.00";

                    // 3. ZEBRA STRIPES: Har alternate row par halka grey color
                    if (row % 2 != 0)
                    {
                        // Range select karke poori row ka color set karein
                        worksheet.Range(row, 1, row, 6).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#F9FAFB"));
                    }
                    row++;
                }

                // 4. Grand Total Styling
                int lastDataRow = row - 1;
                worksheet.Cell(row, 5).Value = "Total Inventory Value:";
                worksheet.Cell(row, 5).Style.Font.Bold = true;

                var grandTotalCell = worksheet.Cell(row, 6);
                grandTotalCell.FormulaA1 = $"=SUM(F2:F{lastDataRow})";
                grandTotalCell.Style.Font.Bold = true;
                grandTotalCell.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#F1F5F9"));
                grandTotalCell.Style.NumberFormat.Format = "₹ #,##0.00";

                worksheet.Columns().AdjustToContents();
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }



    }
}
