using Inventory.Application.Clients;
using Inventory.Application.Common.Interfaces;
using Inventory.Application.GRN.DTOs;
using Inventory.Application.GRN.DTOs.Stock;
using Inventory.Domain.Entities;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Repositories
{
    public class GRNRepository : IGRNRepository
    {
        private readonly InventoryDbContext _context;

        private readonly INotificationRepository _notificationRepository;

        private readonly ISupplierClient _supplierClient;

        public GRNRepository(InventoryDbContext context,
            INotificationRepository notificationRepository,
            ISupplierClient supplierClient)
        {
            _context = context;
            _notificationRepository = notificationRepository;
            _supplierClient = supplierClient;
        }

        //public async Task<string> SaveGRNWithStockUpdate(GRNHeader header, List<GRNDetail> details)
        //{
        //    // 1. PO Reference Check
        //    // Note: Agar aapka ID Guid hai toh 'header.PurchaseOrderId == Guid.Empty' use karein
        //    if (header.PurchaseOrderId == null)
        //    {
        //        throw new Exception("Purchase Order Reference is missing. Cannot save GRN.");
        //    }

        //    using var transaction = await _context.Database.BeginTransactionAsync();
        //    try
        //    {
        //        // --- FIX: Fetch SupplierId from Purchase Order to avoid '0' in DB --- [cite: 2026-02-04]
        //        var po = await _context.PurchaseOrders
        //                               .FirstOrDefaultAsync(p => p.Id == header.PurchaseOrderId);

        //        if (po != null)
        //        {
        //            header.SupplierId = po.SupplierId; // PO se asali SupplierId utha liya [cite: 2026-02-04]
        //        }

        //        // 2. Header Setup - Existing Logic [cite: 2026-02-04]
        //        header.Status = "Received";
        //        header.CreatedOn = DateTime.Now;
        //        header.CreatedBy = header.CreatedBy;
        //        header.ModifiedBy = header.ModifiedBy;

        //        if (string.IsNullOrEmpty(header.GRNNumber) || header.GRNNumber == "AUTO-GEN")
        //        {
        //            header.GRNNumber = await GenerateGRNNumber();
        //        }

        //        await _context.GRNHeaders.AddAsync(header);
        //        await _context.SaveChangesAsync();

        //        // 3. Batch Fetch Products (Optimization) - Existing Logic [cite: 2026-02-04]
        //        var productIds = details.Select(d => d.ProductId).ToList();
        //        var products = await _context.Products
        //                                     .Where(p => productIds.Contains(p.Id))
        //                                     .ToListAsync();

        //        // 4. Detail Mapping & Stock Update - Existing Logic [cite: 2026-02-04]
        //        foreach (var item in details)
        //        {
        //            item.GRNHeaderId = header.Id;
        //            item.CreatedOn = DateTime.Now;
        //            item.UpdatedOn = DateTime.Now;

        //            await _context.GRNDetails.AddAsync(item);

        //            var product = products.FirstOrDefault(p => p.Id == item.ProductId);
        //            if (product != null)
        //            {
        //                product.CurrentStock += item.ReceivedQty;
        //                product.CreatedOn = DateTime.Now;
        //                product.CreatedBy = header.CreatedBy;
        //                _context.Products.Update(product);
        //            }
        //        }

        //        await _context.SaveChangesAsync();
        //        await transaction.CommitAsync();

        //        // --- NOTIFICATION TRIGGER START ---
        //        // Goods receive hone par "Goods Received" ka alert bhejein
        //        await _notificationRepository.AddNotificationAsync(
        //            "Goods Received",
        //            $"Inventory updated for PO #{header.PurchaseOrderId}. GRN {header.GRNNumber} generated successfully.",
        //            "Inventory",
        //            "/app/inventory/grn-list"
        //        );
        //        // --- NOTIFICATION TRIGGER END ---

        //        return header.GRNNumber;
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        throw new Exception($"Error: {ex.Message}");
        //    }
        //}


        public async Task<string> SaveGRNWithStockUpdate(GRNHeader header, List<GRNDetail> details)
        {
            // 1. PO Reference Check
            if (header.PurchaseOrderId == null)
            {
                throw new Exception("Purchase Order Reference is missing. Cannot save GRN.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // --- FIX: Fetch SupplierId from Purchase Order to avoid '0' in DB ---
                var po = await _context.PurchaseOrders
                                       .FirstOrDefaultAsync(p => p.Id == header.PurchaseOrderId);

                if (po != null)
                {
                    header.SupplierId = po.SupplierId; // PO se asali SupplierId utha liya
                }

                // 2. Header Setup - Existing Logic
                header.Status = "Received";
                header.CreatedOn = DateTime.Now;
                header.CreatedBy = header.CreatedBy;
                header.ModifiedBy = header.ModifiedBy;

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
                        // Existing Stock Update Logic
                        product.CurrentStock += item.ReceivedQty;
                        product.CreatedOn = DateTime.Now;
                        product.CreatedBy = header.CreatedBy;
                        _context.Products.Update(product);

                        // --- NEW: LOW STOCK ALERT TRIGGER START ---
                        // Agar stock update ke baad bhi MinStock se kam hai
                        if (product.CurrentStock <= product.MinStock)
                        {
                            // Check karein ki duplicate alert na jaye
                            bool alreadyNotified = await _context.AppNotifications
                                .AnyAsync(n => n.Title.Contains(product.Name) && !n.IsRead && n.Type == "Inventory");

                            if (!alreadyNotified)
                            {
                                await _notificationRepository.AddNotificationAsync(
                                    "Low Stock Alert",
                                    $"Item '{product.Name}' is low. Current: {product.CurrentStock}, Min: {product.MinStock}",
                                    "Inventory",
                                    "/app/inventory/current-stock"
                                );
                            }
                        }
                        // --- NEW: LOW STOCK ALERT TRIGGER END ---
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // --- NOTIFICATION TRIGGER: GOODS RECEIVED ---
                await _notificationRepository.AddNotificationAsync(
                    "Goods Received",
                    $"Inventory updated for PO #{header.PurchaseOrderId}. GRN {header.GRNNumber} generated successfully.",
                    "Inventory",
                    "/app/inventory/grn-list"
                );

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

        public async Task<POForGRNDTO?> GetPODataForGRN(int poId, int? grnHeaderId = null)
        {
            // 1. View Mode Logic: Agar poId 0 hai, toh header table se sahi POId nikaalein
            if (grnHeaderId != null && poId == 0)
            {
                poId = await _context.GRNHeaders
                    .Where(x => x.Id == grnHeaderId)
                    .Select(x => x.PurchaseOrderId)
                    .FirstOrDefaultAsync();

                if (poId == 0) return null; // Case: GRN record hi nahi mila
            }

            // 2. Fetch data with items based on mode (View vs New)
            return await _context.PurchaseOrders
                .Include(h => h.Items)
                .ThenInclude(i => i.Product)
                .Where(h => h.Id == poId)
                .Select(h => new POForGRNDTO
                {
                    POHeaderId = h.Id,
                    PONumber = h.PoNumber ?? "",
                    // Saved GRN number bind karein agar view mode hai
                    GrnNumber = grnHeaderId != null ?
                                _context.GRNHeaders.Where(x => x.Id == grnHeaderId).Select(x => x.GRNNumber).FirstOrDefault() :
                                "AUTO-GEN",
                    SupplierName = h.SupplierName ?? "Unknown",
                    Remarks = grnHeaderId != null ?
                              _context.GRNHeaders.Where(x => x.Id == grnHeaderId).Select(x => x.Remarks).FirstOrDefault() : "",

                    // Problem Solve: Ab PO-47 ke liye DB ke 2 records uthayega, na ki PO ke 3 items
                    Items = grnHeaderId != null
                        ? _context.GRNDetails
                            .Where(g => g.GRNHeaderId == grnHeaderId)
                            .Select(d => new POItemForGRNDTO
                            {
                                ProductId = d.ProductId,
                                ProductName = d.Product.Name ?? "N/A",
                                OrderedQty = d.OrderedQty,
                                ReceivedQty = d.ReceivedQty,
                                RejectedQty = d.RejectedQty,
                                AcceptedQty = d.AcceptedQty,
                                UnitRate = d.UnitRate
                            }).ToList()
                        : h.Items.Select(d => new POItemForGRNDTO
                        {
                            ProductId = d.ProductId,
                            ProductName = d.Product.Name ?? "N/A",
                            OrderedQty = d.Qty,
                            UnitRate = d.Rate - (d.Rate * (d.DiscountPercent / 100)),
                            PendingQty = d.Qty - (_context.GRNDetails
                                    .Where(g => g.ProductId == d.ProductId && g.GRNHeader.PurchaseOrderId == poId)
                                    .Sum(g => (decimal?)g.AcceptedQty) ?? 0),
                            ReceivedQty = 0,
                            RejectedQty = 0,
                            AcceptedQty = 0
                        }).ToList()
                }).FirstOrDefaultAsync();
        }

        

        public async Task<GRNPagedResponseDto> GetGRNPagedListAsync(string search, string sortField, string sortOrder, int pageIndex, int pageSize)
        {
            var query = _context.GRNHeaders.AsQueryable();

            // 1. Searching Logic (Fix: Null checks for safe searching)
            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim().ToLower();
                query = query.Where(x =>
                    (x.GRNNumber != null && x.GRNNumber.ToLower().Contains(s)) ||
                    (x.PurchaseOrder.PoNumber != null && x.PurchaseOrder.PoNumber.ToLower().Contains(s)) ||
                    (x.PurchaseOrder.SupplierName != null && x.PurchaseOrder.SupplierName.ToLower().Contains(s)));
            }

            // 2. Projection to DTO (Upgraded for Expansion Logic)
            var projectedQuery = query.Select(g => new GRNListDto
            {
                Id = g.Id,
                GRNNo = g.GRNNumber,
                RefPO = g.PurchaseOrder.PoNumber,
                SupplierName = g.PurchaseOrder.SupplierName,
                ReceivedDate = g.ReceivedDate,
                Status = g.Status,

                // Expansion ke liye items ka data
                Items = g.GRNItems.Select(d => new GRNItemSummaryDto
                {
                    ProductName = d.Product.Name,
                    ReceivedQty = d.ReceivedQty,
                    RejectedQty = d.RejectedQty,
                    UnitRate = d.UnitRate
                }).ToList(),

                // Frontend status badge logic ke liye total rejections
                TotalRejected = g.GRNItems.Sum(d => d.RejectedQty)
            });

            // 3. Sorting Fix (Matching with frontend field names)
            bool isDesc = sortOrder?.ToLower() == "desc";
            string field = sortField?.ToLower().Trim();

            projectedQuery = field switch
            {
                "grnno" or "grnnumber" => isDesc ? projectedQuery.OrderByDescending(x => x.GRNNo) : projectedQuery.OrderBy(x => x.GRNNo),
                "refpo" => isDesc ? projectedQuery.OrderByDescending(x => x.RefPO) : projectedQuery.OrderBy(x => x.RefPO),
                "suppliername" => isDesc ? projectedQuery.OrderByDescending(x => x.SupplierName) : projectedQuery.OrderBy(x => x.SupplierName),
                "receiveddate" => isDesc ? projectedQuery.OrderByDescending(x => x.ReceivedDate) : projectedQuery.OrderBy(x => x.ReceivedDate),
                _ => isDesc ? projectedQuery.OrderByDescending(x => x.Id) : projectedQuery.OrderByDescending(x => x.Id)
            };

            // 4. Final Execution with Pagination [cite: 2026-01-22]
            var totalCount = await projectedQuery.CountAsync();
            var items = await projectedQuery
                .Skip(pageIndex * pageSize) // Page skip logic
                .Take(pageSize)             // Page size logic
                .ToListAsync();

            return new GRNPagedResponseDto { Items = items, TotalCount = totalCount };
        }


        public async Task<GrnPrintDto?> GetGrnDetailsByNumberAsync(string grnNumber)
        {
            // Step 1: GRN Header fetch karein aur uske details ko PO items ke saath join karein
            var grnData = await _context.GRNHeaders
                .Where(h => h.GRNNumber == grnNumber)
                .AsNoTracking()
                .Select(h => new GrnPrintDto
                {
                    Id = h.Id,
                    GrnNumber = h.GRNNumber,
                    PurchaseOrderId = h.PurchaseOrderId,
                    SupplierId = h.SupplierId,
                    ReceivedDate = h.ReceivedDate,
                    Status = h.Status, //
                    Remarks = h.Remarks,
                    TotalAmount = h.TotalAmount,
                    // Items ko optimize tarike se fetch karne ke liye join logic
                    Items = _context.GRNDetails
                        .Where(d => d.GRNHeaderId == h.Id)
                        .Join(_context.PurchaseOrderItems,
                              d => new { h.PurchaseOrderId, d.ProductId },
                              poi => new { poi.PurchaseOrderId, poi.ProductId },
                              (d, poi) => new GrnItemPrintDto
                              {
                                  ProductName = d.Product.Name, //
                                  Sku = d.Product.Sku,
                                  Unit = d.Product.Unit,
                                  OrderedQty = d.OrderedQty,
                                  PendingQty = d.PendingQty,
                                  ReceivedQty = d.ReceivedQty,
                                  AcceptedQty = d.AcceptedQty,
                                  RejectedQty = d.RejectedQty,
                                  UnitRate = d.UnitRate,
                                  // PO Table se direct data
                                  GstPercentage = poi.GstPercent,
                                  GstAmount = (d.ReceivedQty * d.UnitRate) * (poi.GstPercent / 100),
                                  Total = d.ReceivedQty * d.UnitRate
                              }).ToList()
                })
                .FirstOrDefaultAsync();

            if (grnData == null) return null;

            // Step 2: Footer Calculations (In-Memory calculation for speed)
            grnData.SubTotal = grnData.Items.Sum(i => i.Total);
            grnData.TotalTaxAmount = grnData.Items.Sum(i => i.GstAmount);
            // Note: Agar header.TotalAmount me tax already added hai toh change na karein

            // Step 3: Supplier Microservice Call
            try
            {
                var suppliers = await _supplierClient.GetSuppliersByIdsAsync(new List<int> { grnData.SupplierId });
                grnData.SupplierName = suppliers.FirstOrDefault()?.Name ?? "Supplier Not Found";
            }
            catch
            {
                grnData.SupplierName = "Service Unavailable";
            }

            return grnData;
        }
    }
}
