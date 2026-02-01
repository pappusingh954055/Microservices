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

        

        public async Task<StockPagedResponseDto> GetCurrentStockAsync(string search, string sortField, string sortOrder, int pageIndex, int pageSize)
        {
            // 1. Optimized Base Query [cite: 2026-01-31]
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

                    TotalReceived = group.Sum(x => x.ReceivedQty),
                    TotalRejected = group.Sum(x => x.RejectedQty),
                    AvailableStock = group.Sum(x => x.ReceivedQty) - group.Sum(x => x.RejectedQty),

                    LastRate = group.OrderByDescending(x => x.Id).Select(x => x.UnitRate).FirstOrDefault(),
                    LastPurchaseOrderId = group.OrderByDescending(x => x.Id).Select(x => x.GRNHeader.PurchaseOrderId).FirstOrDefault(),
                    LastSupplierId = group.OrderByDescending(x => x.Id).Select(x => x.GRNHeader.PurchaseOrder.SupplierId).FirstOrDefault(),

                    // Traceability: PO ke hisaab se saare products ki details
                    // Hum yahan us PurchaseOrderId ko pakad rahe hain aur us PO ke saare GRN entries dikha rahe hain
                    History = group.OrderByDescending(x => x.GRNHeader.ReceivedDate)
                                   .SelectMany(h => _context.GRNDetails
                                       .Where(allG => allG.GRNHeaderId == h.GRNHeaderId) // Same PO/GRN ke saare items
                                       .Select(allG => new StockHistoryDto
                                       {
                                           ReceivedDate = allG.GRNHeader.ReceivedDate,
                                           PONumber = allG.GRNHeader.PurchaseOrder.PoNumber,
                                           SupplierName = allG.GRNHeader.PurchaseOrder.SupplierName,
                                           // Yahan hum ProductName bhi add kar rahe hain taaki pata chale kaunsa item tha
                                           ProductName = allG.Product.Name,
                                           ReceivedQty = allG.ReceivedQty,
                                           RejectedQty = allG.RejectedQty
                                       })).ToList()
                });

            // 2. Search Logic
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => x.ProductName.Contains(search));
            }

            // 3. Dynamic Sorting [cite: 2026-01-31]
            bool isDesc = sortOrder?.ToLower() == "desc";
            query = sortField?.ToLower() switch
            {
                "productname" => isDesc ? query.OrderByDescending(x => x.ProductName) : query.OrderBy(x => x.ProductName),
                "availablestock" => isDesc ? query.OrderByDescending(x => x.AvailableStock) : query.OrderBy(x => x.AvailableStock),
                "totalrejected" => isDesc ? query.OrderByDescending(x => x.TotalRejected) : query.OrderBy(x => x.TotalRejected),
                "unitrate" => isDesc ? query.OrderByDescending(x => x.LastRate) : query.OrderBy(x => x.LastRate),
                _ => query.OrderBy(x => x.ProductName)
            };

            // 4. Final Execution
            var totalCount = await query.CountAsync();
            var items = await query.Skip(pageIndex * pageSize).Take(pageSize).ToListAsync();

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
                    // FIX: AcceptedQty ki jagah direct (Received - Rejected) karein taaki 141 - 2 = 139 aaye
                    AvailableStock = g.Sum(x => x.ReceivedQty) - g.Sum(x => x.RejectedQty),
                    // Value (Avg) match kar raha hai (₹1,650.00)
                    LastRate = g.OrderByDescending(x => x.Id).Select(x => x.UnitRate).FirstOrDefault(),
                    MinStockLevel = g.Key.MinLevel
                })
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Current Stock");

                // Header Styling
                string[] headers = { "Product Name", "Total Received", "Rejected", "Current Stock", "Value (Avg)", "Total Value" };
                var headerRow = worksheet.Row(1);
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = headerRow.Cell(i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#3f51b5");
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                int row = 2;
                foreach (var item in stockData)
                {
                    worksheet.Cell(row, 1).Value = item.ProductName;
                    worksheet.Cell(row, 2).Value = item.TotalReceived;
                    worksheet.Cell(row, 3).Value = item.TotalRejected;
                    // Ab ye 139 dikhayega
                    worksheet.Cell(row, 4).Value = item.AvailableStock;

                    var rateCell = worksheet.Cell(row, 5);
                    rateCell.Value = item.LastRate;
                    rateCell.Style.NumberFormat.Format = "₹ #,##0.00";

                    // Total Value Formula
                    var totalValCell = worksheet.Cell(row, 6);
                    totalValCell.FormulaA1 = $"=D{row}*E{row}";
                    totalValCell.Style.NumberFormat.Format = "₹ #,##0.00";

                    // Low Stock Highlight
                    if (item.AvailableStock <= item.MinStockLevel)
                    {
                        worksheet.Cell(row, 4).Style.Font.FontColor = XLColor.Red;
                        worksheet.Cell(row, 4).Style.Font.Bold = true;
                    }
                    row++;
                }

                // Grand Total
                int lastDataRow = row - 1;
                worksheet.Cell(row, 5).Value = "Total Inventory Value:";
                worksheet.Cell(row, 5).Style.Font.Bold = true;
                worksheet.Cell(row, 6).FormulaA1 = $"=SUM(F2:F{lastDataRow})";
                worksheet.Cell(row, 6).Style.Font.Bold = true;
                worksheet.Cell(row, 6).Style.NumberFormat.Format = "₹ #,##0.00";

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
