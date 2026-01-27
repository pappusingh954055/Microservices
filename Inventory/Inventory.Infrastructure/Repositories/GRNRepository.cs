using Inventory.Application.Common.Interfaces;
using Inventory.Application.GRN.DTOs;
using Inventory.Domain.Entities;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Repositories
{
    public class GRNRepository : IGRNRepository
    {
        private readonly InventoryDbContext _context;

        public GRNRepository(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<string> SaveGRNWithStockUpdate(GRNHeader header, List<GRNDetail> details)
        {
            // 1. PO Reference Check
            if (header.PurchaseOrderId <= 0)
            {
                throw new Exception("Purchase Order Reference (POHeaderId) is missing. Cannot save GRN.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 2. Header Setup
                header.Status = "Received"; // SQL NULL error fix
                header.CreatedOn = DateTime.Now;
                header.CreatedBy = header.CreatedBy;               
                header.ModifiedBy = header.ModifiedBy;
                // Agar UI se AUTO-GEN aaya hai toh naya number generate karein
                if (string.IsNullOrEmpty(header.GRNNumber) || header.GRNNumber == "AUTO-GEN")
                {
                    header.GRNNumber = await GenerateGRNNumber();
                }

                await _context.GRNHeaders.AddAsync(header);
                await _context.SaveChangesAsync();

                // 3. Batch Fetch Products (Optimization)
                var productIds = details.Select(d => d.ProductId).ToList();
                var products = await _context.Products
                                             .Where(p => productIds.Contains(p.Id))
                                             .ToListAsync();

                // 4. Detail Mapping & Stock Update
                foreach (var item in details)
                {
                    item.GRNHeaderId = header.Id;
                    item.CreatedOn = DateTime.Now;
                    item.UpdatedOn = DateTime.Now;

                    await _context.GRNDetails.AddAsync(item);

                    var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                    if (product != null)
                    {
                        product.CurrentStock += item.ReceivedQty;
                        product.CreatedOn = DateTime.Now;
                        product.CreatedBy = header.CreatedBy;
                        _context.Products.Update(product);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // SUCCESS: Ab true ki jagah naya GRN Number return karein
                return header.GRNNumber;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Error: {ex.Message}");
            }
        }

        public async Task<string> GenerateGRNNumber()
        {
            // Logic to generate like GRN-2026-001
            return $"GRN-{DateTime.Now.Year}-{new Random().Next(1000, 9999)}";
        }

        public async Task<POForGRNDTO?> GetPODataForGRN(int poId)
        {
            return await _context.PurchaseOrders
                .Include(h => h.Items)
                .ThenInclude(i => i.Product) // Product include karein taaki Name mile
                .Where(h => h.Id == poId)
                .Select(h => new POForGRNDTO
                {
                    POHeaderId = h.Id,
                    PONumber = h.PoNumber ?? "",
                    SupplierId = h.SupplierId,
                    SupplierName = h.SupplierName ?? "Unknown",
                    Items = h.Items.Select(d => new POItemForGRNDTO
                    {
                        ProductId = d.ProductId,
                        ProductName = d.Product.Name ?? "N/A",
                        OrderedQty = d.Qty,

                        // Logic: Rate se Discount percentage minus karein
                        // Example: 2000 - (2000 * 20 / 100) = 1600
                        UnitRate = d.Rate - (d.Rate * (d.DiscountPercent / 100)),

                        DiscountPercentage = d.DiscountPercent,
                        PendingQty = d.Qty - (_context.GRNDetails
                            .Where(g => g.ProductId == d.ProductId && g.GRNHeader.PurchaseOrderId == poId)
                            .Sum(g => (decimal?)g.ReceivedQty) ?? 0)
                    }).ToList()
                }).FirstOrDefaultAsync();
        }

        public async Task<GRNPagedResponseDto> GetGRNPagedListAsync(string search, string sortField, string sortOrder, int pageIndex, int pageSize)
        {
            // 1. Initial Query with Joins (Entity se data nikalne ke liye)
            var query = _context.GRNHeaders.AsQueryable();

            // 2. Projection: DTO mein map karein
            var projectedQuery = query.Select(g => new GRNListDto
            {
                Id = g.Id,
                GRNNo = g.GRNNumber,
                RefPO = g.PurchaseOrder.PoNumber,
                SupplierName = g.PurchaseOrder.SupplierName,
                ReceivedDate = g.ReceivedDate,
                Status = g.Status
            });

            // 3. Searching
            if (!string.IsNullOrEmpty(search))
            {
                string s = search.ToLower();
                projectedQuery = projectedQuery.Where(x =>
                    x.GRNNo.ToLower().Contains(s) ||
                    x.RefPO.ToLower().Contains(s) ||
                    x.SupplierName.ToLower().Contains(s));
            }

            // 4. FIXED Dynamic Sorting
            // Note: 'sortField' exactly match hona chahiye aapke displayedColumns se
            bool isDesc = sortOrder?.ToLower() == "desc";

            projectedQuery = (sortField?.ToLower()) switch
            {
                "grnnumber" => isDesc ? projectedQuery.OrderByDescending(x => x.GRNNo) : projectedQuery.OrderBy(x => x.GRNNo),
                "refpo" => isDesc ? projectedQuery.OrderByDescending(x => x.RefPO) : projectedQuery.OrderBy(x => x.RefPO),
                "suppliername" => isDesc ? projectedQuery.OrderByDescending(x => x.SupplierName) : projectedQuery.OrderBy(x => x.SupplierName),
                "receiveddate" => isDesc ? projectedQuery.OrderByDescending(x => x.ReceivedDate) : projectedQuery.OrderBy(x => x.ReceivedDate),
                "status" => isDesc ? projectedQuery.OrderByDescending(x => x.Status) : projectedQuery.OrderBy(x => x.Status),
                _ => isDesc ? projectedQuery.OrderByDescending(x => x.Id) : projectedQuery.OrderByDescending(x => x.Id)
            };

            // 5. Execution [cite: 2026-01-22]
            var totalCount = await projectedQuery.CountAsync();
            var items = await projectedQuery
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new GRNPagedResponseDto { Items = items, TotalCount = totalCount };
        }
    }
}
