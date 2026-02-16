using Inventory.Application.Clients;
using Inventory.Application.Common.Interfaces;
using Inventory.Application.GRN.DTOs;
using Inventory.Application.GRN.DTOs.Stock;
using Inventory.Domain.Entities;
using Inventory.Infrastructure.Persistence;
using MediatR;
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

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // --- FIX: Fetch SupplierId from Purchase Order to avoid '0' in DB ---
                    // Include Items taaki niche ReceivedQty update ho sake
                    var po = await _context.PurchaseOrders
                                           .Include(p => p.Items)
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
                            if (product.CurrentStock <= product.MinStock)
                            {
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

                        // --- EXTRA LOGIC: Update PurchaseOrderItem Received Qty ---
                        if (po != null)
                        {
                            var poItem = po.Items.FirstOrDefault(pi => pi.ProductId == item.ProductId);
                            if (poItem != null)
                            {
                                // Existing ReceivedQty mein current GRN ki qty add kar rahe hain
                                poItem.ReceivedQty = (poItem.ReceivedQty) + item.ReceivedQty;
                                _context.PurchaseOrderItems.Update(poItem);
                            }
                        }
                    }

                    // --- EXTRA LOGIC: Auto Update PO Status if fully received ---
                    if (po != null && po.Items.All(i => (i.ReceivedQty) >= i.Qty))
                    {
                        po.Status = "Received";
                        _context.PurchaseOrders.Update(po);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // --- NOTIFICATION TRIGGER: GOODS RECEIVED ---
                    await _notificationRepository.AddNotificationAsync(
                        "Goods Received",
                        $"Inventory updated for PO #{po?.PoNumber ?? header.PurchaseOrderId.ToString()}. GRN {header.GRNNumber} generated successfully.",
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
            });
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

            // 2. Fetch data based on mode (View vs New)
            return await _context.PurchaseOrders
                .Include(h => h.Items)
                .ThenInclude(i => i.Product)
                .Where(h => h.Id == poId)
                .Select(h => new POForGRNDTO
                {
                    POHeaderId = h.Id,
                    PONumber = h.PoNumber ?? "",
                    // View mode mein database se saved GRN Number uthayein
                    GrnNumber = grnHeaderId != null ?
                                _context.GRNHeaders.Where(x => x.Id == grnHeaderId).Select(x => x.GRNNumber).FirstOrDefault() :
                                "AUTO-GEN",
                    SupplierName = h.SupplierName ?? "Unknown",
                    Remarks = grnHeaderId != null ?
                              _context.GRNHeaders.Where(x => x.Id == grnHeaderId).Select(x => x.Remarks).FirstOrDefault() : "",

                    // Problem Solve: Data Separation with Correct Logic
                    Items = grnHeaderId != null
                        ? _context.GRNDetails
                            .Where(g => g.GRNHeaderId == grnHeaderId)
                            .Include(d => d.Product)
                            .Select(d => new { d, poi = _context.PurchaseOrderItems.FirstOrDefault(p => p.PurchaseOrderId == h.Id && p.ProductId == d.ProductId) })
                            .Select(x => new POItemForGRNDTO
                            {
                                ProductId = x.d.ProductId,
                                ProductName = x.d.Product.Name ?? "N/A",
                                OrderedQty = x.d.OrderedQty,
                                ReceivedQty = x.d.ReceivedQty,
                                RejectedQty = x.d.RejectedQty,
                                AcceptedQty = x.d.ReceivedQty - x.d.RejectedQty,
                                UnitRate = x.d.UnitRate,
                                PendingQty = x.d.OrderedQty - x.d.ReceivedQty,
                                DiscountPercent = x.poi != null ? x.poi.DiscountPercent : 0,
                                GstPercent = x.poi != null ? x.poi.GstPercent : 0,
                                TaxAmount = (x.d.ReceivedQty - x.d.RejectedQty) * x.d.UnitRate * (x.poi != null ? x.poi.GstPercent / 100 : 0)
                            }).ToList()
                        : h.Items.Select(d => new POItemForGRNDTO
                        {
                            ProductId = d.ProductId,
                            ProductName = d.Product.Name ?? "N/A",
                            OrderedQty = d.Qty,
                            UnitRate = d.Rate,
                            DiscountPercent = d.DiscountPercent,
                            GstPercent = d.GstPercent,

                            PendingQty = d.Qty - (d.ReceivedQty),
                            ReceivedQty = d.Qty - (d.ReceivedQty),
                            RejectedQty = 0,
                            AcceptedQty = d.Qty - (d.ReceivedQty),
                            // Auto-calculate Tax Amount
                            TaxAmount = ((d.Qty - (d.ReceivedQty)) * d.Rate * (1 - d.DiscountPercent / 100)) * (d.GstPercent / 100)
                        }).ToList()
                }).FirstOrDefaultAsync();
        }



        public async Task<GRNPagedResponseDto> GetGRNPagedListAsync(string search, string sortField, string sortOrder, int pageIndex, int pageSize)
        {
            var query = _context.GRNHeaders.AsQueryable();

            // 1. Searching Logic (Existing preserved)
            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim().ToLower();
                query = query.Where(x =>
                    (x.GRNNumber != null && x.GRNNumber.ToLower().Contains(s)) ||
                    (x.PurchaseOrder.PoNumber != null && x.PurchaseOrder.PoNumber.ToLower().Contains(s)) ||
                    (x.PurchaseOrder.SupplierName != null && x.PurchaseOrder.SupplierName.ToLower().Contains(s)));
            }

            // 2. Projection to DTO (Corrected Pending Logic)
            var projectedQuery = query.Select(g => new GRNListDto
            {
                Id = g.Id,
                GRNNo = g.GRNNumber,
                RefPO = g.PurchaseOrder.PoNumber,
                SupplierName = g.PurchaseOrder.SupplierName,
                SupplierId = g.SupplierId,  // For payment navigation
                ReceivedDate = g.ReceivedDate,
                Status = g.Status,
                TotalAmount = g.TotalAmount,  // GRN Total Amount
                PaymentStatus = "Unpaid",  // Default - To be calculated from Supplier Ledger

                Items = g.GRNItems.Select(d => new GRNItemSummaryDto
                {
                    ProductName = d.Product.Name,
                    OrderedQty = d.OrderedQty,
                    ReceivedQty = d.ReceivedQty,

                    // FIX: Pending calculation for historical view
                    // Hum PO Item ki cumulative 'ReceivedQty' ke bajaye transaction level logic use karenge
                    // Pending = Total Ordered - Jo is GRN tak total receive ho chuka tha
                    PendingQty = d.OrderedQty - (
                        _context.GRNDetails
                            .Where(prev => prev.ProductId == d.ProductId &&
                                           prev.GRNHeader.PurchaseOrderId == g.PurchaseOrderId &&
                                           prev.GRNHeader.CreatedOn <= g.CreatedOn)
                            .Sum(prev => prev.ReceivedQty)
                    ),

                    RejectedQty = d.RejectedQty,
                    UnitRate = d.UnitRate
                }).ToList(),

                TotalRejected = g.GRNItems.Sum(d => d.RejectedQty)
            });

            // 3. Sorting Fix
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

            // 4. Final Execution
            var totalCount = await projectedQuery.CountAsync();
            var items = await projectedQuery
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
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
                                  DiscountPercent = poi.DiscountPercent,
                                  // PO Table se direct data
                                  GstPercentage = poi.GstPercent,
                                  GstAmount = ((d.ReceivedQty * d.UnitRate) * (1 - poi.DiscountPercent / 100)) * (poi.GstPercent / 100),
                                  Total = (d.ReceivedQty * d.UnitRate) * (1 - poi.DiscountPercent / 100)
                              }).ToList()
                })
                .FirstOrDefaultAsync();

            if (grnData == null) return null;

            // Step 2: Footer Calculations (In-Memory calculation for speed)
            grnData.SubTotal = grnData.Items.Sum(i => i.Total);
            grnData.TotalTaxAmount = grnData.Items.Sum(i => i.GstAmount);
            grnData.TotalAmount = grnData.SubTotal + grnData.TotalTaxAmount;

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

        public async Task<bool> CreateBulkGrnFromPoAsync(BulkGrnRequestDto request)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    foreach (var poId in request.PurchaseOrderIds)
                    {
                        // 1. PO aur Items fetch karein
                        var poHeader = await _context.PurchaseOrders
                            .Include(p => p.Items)
                            .FirstOrDefaultAsync(p => p.Id == poId && (p.Status == "Approved" || p.Status == "Partially Received"));

                        if (poHeader == null) continue;

                        // 2. Custom function se GRN Number generate karein
                        string newGrnNumber = await GenerateGRNNumber();

                        // 3. Naya GRN Header create karein
                        var grnHeader = new GRNHeader
                        {
                            GRNNumber = newGrnNumber,
                            PurchaseOrderId = poId,
                            SupplierId = poHeader.SupplierId,
                            ReceivedDate = DateTime.Now,
                            TotalAmount = poHeader.GrandTotal,
                            Status = "Received",
                            Remarks = "Bulk Processed from PO",
                            CreatedBy = request.CreatedBy,
                            CreatedOn = DateTime.Now
                        };

                        _context.GRNHeaders.Add(grnHeader);
                        await _context.SaveChangesAsync();

                        bool isFullPoReceived = true; // Check karne ke liye ki PO complete hua ya nahi

                        // 4. PO Items ko map karein, Stock update karein aur ReceivedQty track karein
                        foreach (var item in poHeader.Items)
                        {
                            // Pending check: Kitna aana baaki hai?
                            decimal pendingForThisItem = item.Qty - (item.ReceivedQty);

                            if (pendingForThisItem <= 0) continue; // Agar ye item poora aa chuka hai toh skip karein

                            // Bulk upload mein hum bacha hua poora maal receive kar rahe hain
                            decimal qtyToReceiveNow = pendingForThisItem;

                            var grnDetail = new GRNDetail
                            {
                                GRNHeaderId = grnHeader.Id,
                                ProductId = item.ProductId,
                                OrderedQty = item.Qty,
                                ReceivedQty = qtyToReceiveNow,
                                AcceptedQty = qtyToReceiveNow,
                                RejectedQty = 0,
                                UnitRate = item.Rate,
                                CreatedBy = request.CreatedBy,
                                CreatedOn = DateTime.Now
                            };
                            _context.GRNDetails.Add(grnDetail);

                            // FIX A: PO Item table mein ReceivedQty update karein taaki Pending calculation sahi ho
                            item.ReceivedQty = (item.ReceivedQty) + qtyToReceiveNow;

                            // FIX B: STOCK UPDATE LOGIC
                            var product = await _context.Products
                                .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                            if (product != null)
                            {
                                product.CurrentStock += qtyToReceiveNow;
                            }

                            // Check: Agar abhi bhi koi item pending reh gaya (Partial delivery case)
                            if (item.ReceivedQty < item.Qty)
                            {
                                isFullPoReceived = false;
                            }
                        }

                        // 5. PO status update (Partial vs Full)
                        poHeader.Status = isFullPoReceived ? "GRN Processed" : "Partially Received";

                        // 6. NOTIFICATION TRIGGER
                        await _notificationRepository.AddNotificationAsync(
                            "Goods Received",
                            $"Inventory updated for PO #{poId}. GRN {newGrnNumber} generated successfully.",
                            "Inventory",
                            "/app/inventory/grn-list"
                        );
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Bulk GRN Error: {ex.Message}");
                    return false;
                }
            });
        }
    }
}
