using DinkToPdf;
using DinkToPdf.Contracts;
using Inventory.Application.Common.DTOs;
using Inventory.Application.Common.Interfaces;
using Inventory.Application.PurchaseOrders.DTOs;
using Inventory.Domain.Entities;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq;

public sealed class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly InventoryDbContext _context;
    private readonly IConverter _converter;

    public PurchaseOrderRepository(InventoryDbContext context, IConverter converter)
    {
        _context = context;
        _converter = converter;
    }

    public async Task AddAsync(PurchaseOrder po, CancellationToken ct)
    {
        await _context.PurchaseOrders.AddAsync(po, ct);
    }    

    public async Task<string?> GetLastPoNumberAsync()
    {
        return await _context.PurchaseOrders.AsNoTracking()
            .OrderByDescending(x => x.Id)
            .Select(x => x.PoNumber)
            .FirstOrDefaultAsync();
    }

    public async Task<string?> GetLatestPoNumberAsync()
    {
        return await _context.PurchaseOrders.AsNoTracking()
            .OrderByDescending(x => x.Id)
            .Select(x => x.PoNumber)
            .FirstOrDefaultAsync();
    }

    public async Task<(IEnumerable<PurchaseOrder> Items, int TotalCount)> GetPagedOrdersAsync(
    int pageIndex, int pageSize, string? sortField, string? sortOrder, string? filter)
    {
        // 1. Base Query with Eager Loading for Items and Products
        var query = _context.PurchaseOrders.AsNoTracking()
            .Include(x => x.Items)
                .ThenInclude(i => i.Product) // Product data load karega taaki null error na aaye
            .AsSplitQuery()
            .AsNoTracking()
            .AsQueryable();

        // 2. Filtering Logic (Multi-column search)
        if (!string.IsNullOrWhiteSpace(filter))
        {
            var cleanFilter = filter.Trim();

            query = query.Where(x =>
                EF.Functions.Like(x.PoNumber, $"%{cleanFilter}%") ||
                EF.Functions.Like(Convert.ToString(x.Id), $"%{cleanFilter}%") ||
                EF.Functions.Like(x.Status, $"%{cleanFilter}%") ||
                EF.Functions.Like(x.SupplierName, $"%{cleanFilter}%") // Direct DB column search
            );
        }

        // 3. Sorting Logic
        if (!string.IsNullOrEmpty(sortField))
        {
            string mappedField = sortField.ToLower() switch
            {
                "ponumber" => "PoNumber",
                "podate" => "PoDate",
                "grandtotal" => "GrandTotal",
                "status" => "Status",
                "id" => "Id",
                "suppliername" => "SupplierName",
                _ => "PoDate"
            };

            query = (sortOrder?.ToLower() == "desc")
                ? query.OrderByDescending(x => EF.Property<object>(x, mappedField))
                : query.OrderBy(x => EF.Property<object>(x, mappedField));
        }
        else
        {
            query = query.OrderByDescending(x => x.PoDate);
        }

        // 4. Execution
        var totalCount = await query.CountAsync();

        var items = await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <summary>
    /// bind main grid polist
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<(IEnumerable<PurchaseOrder> Data, int Total)> GetDateRangePagedOrdersAsync(GetPurchaseOrdersRequest request)
    {
        // STEP 1: Base Query - AsNoTracking use karein fast read ke liye [cite: 2026-02-04]
        // Include yahan se hata diya hai taaki CountAsync fast chale [cite: 2026-02-04]
        var query = _context.PurchaseOrders
            .AsNoTracking()
            .AsQueryable();

        // 1. GLOBAL SEARCH FIX
        if (!string.IsNullOrWhiteSpace(request.Filter))
        {
            var searchTerm = request.Filter.Trim().ToLower();
            query = query.Where(x =>
                (x.PoNumber != null && x.PoNumber.ToLower().Contains(searchTerm)) ||
                (x.SupplierName != null && x.SupplierName.ToLower().Contains(searchTerm)) ||
                (x.Status != null && x.Status.ToLower().Contains(searchTerm))
            );
        }

        // 2. DATE RANGE
        if (request.FromDate.HasValue) query = query.Where(x => x.PoDate >= request.FromDate.Value);
        if (request.ToDate.HasValue)
        {
            var endOfToDate = request.ToDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(x => x.PoDate <= endOfToDate);
        }

        // 3. COLUMN SPECIFIC FILTERS FIX
        if (request.Filters != null && request.Filters.Any())
        {
            foreach (var f in request.Filters)
            {
                var val = (f.Value ?? "").Trim().ToLower();
                var field = (f.Field ?? "").Trim().ToLower();

                if (string.IsNullOrEmpty(val)) continue;

                query = field switch
                {
                    "status" => query.Where(x => x.Status != null && x.Status.ToLower() == val),
                    "ponumber" or "po no." => query.Where(x => x.PoNumber != null && x.PoNumber.ToLower().Contains(val)),
                    "suppliername" => query.Where(x => x.SupplierName != null && x.SupplierName.ToLower().Contains(val)),
                    "id" => query.Where(x => x.Id.ToString().Contains(val)),
                    _ => query
                };
            }
        }

        // STEP 2: Get Total Count before adding heavy Includes [cite: 2026-02-04]
        var total = await query.CountAsync();

        // 4. DYNAMIC SORTING FIX
        bool isDesc = request.SortOrder?.ToLower() == "desc";
        string sortField = request.SortField?.ToLower().Trim();

        query = sortField switch
        {
            "status" => isDesc ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            "ponumber" => isDesc ? query.OrderByDescending(x => x.PoNumber) : query.OrderBy(x => x.PoNumber),
            "suppliername" => isDesc ? query.OrderByDescending(x => x.SupplierName) : query.OrderBy(x => x.SupplierName),
            _ => isDesc ? query.OrderByDescending(x => x.PoDate) : query.OrderBy(x => x.PoDate)
        };

        // STEP 3: Optimized Data Fetch [cite: 2026-02-04]
        // Ab sirf wahi 10-20 records ke liye Include chalega jo page par hain [cite: 2026-02-04]
        var data = await query
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize)
            .Include(x => x.Items)
                .ThenInclude(i => i.Product)
            .Include(x => x.GrnHeaders)
            .ToListAsync();

        return (data, total);
    }
    public async Task<PurchaseOrder?> GetByIdWithItemsAsync(int id, CancellationToken ct)
    {
      var data= await _context.PurchaseOrders.AsNoTracking()
            .Include(x => x.Items) // Yeh child table 'PurchaseOrderItems' se data layega
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return data;
    }

    public void Update(PurchaseOrder po) => _context.PurchaseOrders.Update(po);

    public void RemoveItem(PurchaseOrderItem item) => _context.PurchaseOrderItems.Remove(item);

    public async Task<bool> DeleteItemAsync(int itemId)
    {
        var item = await _context.PurchaseOrderItems.FindAsync(itemId);
        if (item == null) return false;

        _context.PurchaseOrderItems.Remove(item);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task UpdatePOTotalsAsync(int poId)
    {
        var items = await _context.PurchaseOrderItems.AsNoTracking()
                                  .Where(x => x.PurchaseOrderId == poId)
                                  .ToListAsync();

        var po = await _context.PurchaseOrders.FindAsync(poId);

        if (po != null)
        {
            // 1. SubTotal hamesha (Total - TaxAmount) hona chahiye agar aapke DB mein Total inclusive hai
            // Ya phir agar aapke paas TaxableAmount ka alag column hai toh wo use karein.
            po.SubTotal = items.Sum(x => x.Total - x.TaxAmount);

            // 2. TotalTax bilkul sahi hai
            po.TotalTax = items.Sum(x => x.TaxAmount);

            // 3. GrandTotal ab accurate aayega
            po.GrandTotal = po.SubTotal + po.TotalTax;

            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> BulkDeleteItemsAsync(List<int> itemIds)
    {
        var items = await _context.PurchaseOrderItems.AsNoTracking()
                                  .Where(x => itemIds.Contains(x.Id))
                                  .ToListAsync();

        if (!items.Any()) return false;

        _context.PurchaseOrderItems.RemoveRange(items);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<PurchaseOrder> GetByIdAsync(int id)
    {
        // .Include() tab use karein agar delete se pehle Items ka status check karna ho
        return await _context.PurchaseOrders.AsNoTracking()
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public void Delete(PurchaseOrder po)
    {
        _context.PurchaseOrders.Remove(po);
    }

    public async Task<List<PurchaseOrder>> GetByIdsAsync(List<int> ids)
    {
        return await _context.PurchaseOrders.AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .ToListAsync();
    }

    /// <summary>
    /// /////////
    /// </summary>
    /// <param name="po"></param>
    /// <returns></returns>
    public Task UpdateAsync(PurchaseOrder po)
    {
        _context.PurchaseOrders.Attach(po);
        _context.Entry(po).Property(x => x.Status).IsModified = true;
        return Task.CompletedTask;
    }

    public async Task<bool> SaveChangesAsync()
    {
        // Agar 1 ya usse zyada rows affect hui hain toh true return karega
        return (await _context.SaveChangesAsync()) > 0;
    }
    public async Task<PurchaseOrder> GetByIdAsyncForUpdateStatus(int id)
    {
        // PO ke saath uske items ko bhi load karna behtar hota hai (Optional)
        return await _context.PurchaseOrders.AsNoTracking()
                             .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<bool> UpdatePOStatusAsync(int id, string status)
    {
        var po = await _context.PurchaseOrders.FindAsync(id);
        if (po == null) return false;

        po.Status = status; 
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<IEnumerable<PendingPODto>> GetPendingPurchaseOrdersAsync()
    {
        return await _context.PurchaseOrders.AsNoTracking()
            .Where(po => po.Status == "Approved" && !_context.GRNHeaders.Any(grn => grn.PurchaseOrderId == po.Id))
            .Select(po => new PendingPODto
            {
                Id = po.Id,
                PoNumber = po.PoNumber,
                SupplierName = po.SupplierName,
                PoDate = po.PoDate,
                Status = po.Status
            })
            .OrderByDescending(po => po.Id)
            .ToListAsync();
    }
    public async Task<IEnumerable<POItemForGRNDto>> GetPOItemsForGRNAsync(int poId)
    {
        var poItems = await _context.PurchaseOrderItems.AsNoTracking()
            .Where(poi => poi.PurchaseOrderId == poId)
            .Include(poi => poi.Product)
            .ToListAsync();

        var receivedQuantities = await _context.Set<GRNDetail>()

            .Where(gi => gi.GRNHeader.PurchaseOrderId == poId)
            .GroupBy(gi => gi.ProductId)
            .Select(g => new { ProductId = g.Key, Total = g.Sum(x => (decimal?)x.ReceivedQty) ?? 0 })
            .ToListAsync();

       
        return poItems.Select(poi => new POItemForGRNDto
        {
            ProductId = poi.ProductId,
            ProductName = poi.Product?.Name,
            OrderedQty = poi.Qty,
            UnitPrice = poi.Rate,
            AlreadyReceivedQty = receivedQuantities.FirstOrDefault(rq => rq.ProductId == poi.ProductId)?.Total ?? 0
        }).ToList();
    }

    public async Task<POHeaderDetailsDto?> GetPOHeaderAsync(int lastPurchaseOrderId)
    {
        return await _context.PurchaseOrders.AsNoTracking()
        .Where(x => x.Id == lastPurchaseOrderId)
        
        .Select(x => new POHeaderDetailsDto
        {
            PurchaseOrderId = x.Id,
            ExpectedDeliveryDate=x.ExpectedDeliveryDate,
            SupplierId = x.SupplierId,      // int
            SupplierName = x.SupplierName,
            PriceListId = x.PriceListId, 
            
            Remarks = x.Remarks,
            PoNumber = x.PoNumber,
            PoDate = DateTime.Now           // Hamesha current date rakhein
        }).FirstOrDefaultAsync();
    }

    public async Task<ProductPriceDto?> GetPriceListRateAsync( Guid productId, Guid priceListId)
    {
        return await _context.PriceListItems.AsNoTracking()
            .Where(pi => pi.PriceListId == priceListId && pi.ProductId == productId)
            .Select(pi => new ProductPriceDto
            {
                ProductId = pi.ProductId,
                Rate = pi.Rate, // From dbo.PriceListItems
                Unit = pi.Unit, // From dbo.PriceListItems
                                // From dbo.Products.DefaultGst
                GstPercent = pi.Product.DefaultGst??0
            })
            .FirstOrDefaultAsync();
    }
    public async Task<bool> BulkSentForApprovalAsync(List<long> ids)
    {
        
        var pos = await _context.PurchaseOrders
            .Where(x => ids.Contains(x.Id) && x.Status == "Draft")
            .ToListAsync();

     
        if (pos == null || !pos.Any())
        {
            return false; 
        }

        foreach (var po in pos)
        {
            po.Status = "Submitted"; 
            po.UpdatedDate = DateTime.Now; 
        }

        // 3. Ab SaveChanges kaam karega kyunki tracking ON hai
        return await _context.SaveChangesAsync() > 0;
    }

    
    public async Task<bool> BulkApprovePOsAsync(List<long> ids, string approvedBy)
    {
       
        var pos = await _context.PurchaseOrders
            .Where(x => ids.Contains(x.Id) && x.Status == "Submitted")
            .ToListAsync();

        if (pos == null || !pos.Any()) return false;

        foreach (var po in pos)
        {
            // 2. Status update to Approved
            po.Status = "Approved";
            po.UpdatedBy = approvedBy; 
            po.UpdatedDate = DateTime.Now; 
        }

        // 3. SaveChanges execute karega kyunki tracking ON hai
        return await _context.SaveChangesAsync() > 0;
    }

    // PORepository.cs
    public async Task<bool> BulkRejectPOsAsync(List<long> ids, string rejectedBy)
    {
        // 1. Sirf 'Submitted' status wale POs hi Reject kiye ja sakte hain
        var pos = await _context.PurchaseOrders
            .Where(x => ids.Contains(x.Id) && x.Status == "Submitted")
            .ToListAsync();

        if (pos == null || !pos.Any()) return false;

        foreach (var po in pos)
        {
            // 2. Status update to Rejected
            po.Status = "Rejected";
            po.UpdatedBy = rejectedBy;
            po.UpdatedDate = DateTime.Now; // DB column match
        }

        // 3. Tracking ON hai isliye changes save ho jayenge
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<PODocumentDto> GetPODetailsForPrintAsync(long id)
    {
        // 1. FIX: StringComparison ko hata kar simple '==' use karein taaki EF ise SQL mein translate kar sake
        // SQL Server default mein case-insensitive match hi karta hai
        bool isReceived = await _context.GRNHeaders
            .AsNoTracking()
            .AnyAsync(g => g.PurchaseOrderId == id && g.Status == "Received");

        // 2. Data fetch karein optimized join ke saath
        return await _context.PurchaseOrders
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(po => new PODocumentDto
            {
                // Header Mapping
                PoNumber = po.PoNumber,
                SupplierName = po.SupplierName,
                PoDate = po.PoDate,
                Remarks = po.Remarks,

                // Tax aur SubTotal details for Modal UI
                SubTotal = po.SubTotal,
                TotalTax = po.TotalTax,
                GrandTotal = po.GrandTotal,

                // Dynamic Title based on GRN status
                Status = isReceived ? "TAX INVOICE" : "BILL OF SUPPLY",
                CreatedBy = po.CreatedBy,

                // Items Mapping with Products Table Join
                Items = _context.PurchaseOrderItems
                    .AsNoTracking()
                    .Where(item => item.PurchaseOrderId == po.Id)
                    .Join(_context.Products,
                          item => item.ProductId,
                          prod => prod.Id,
                          (item, prod) => new POItemDocumentDto
                          {
                              ProductId = item.ProductId,
                              ProductName = prod.Name, // Actual Name
                              Qty = item.Qty,
                              Unit = item.Unit,
                              Rate = item.Rate,
                              TaxAmount = item.TaxAmount,
                              Total = item.Total
                          }).ToList()
            }).FirstOrDefaultAsync();
    }

    public async Task<PORepoPrintResponse> GeneratePOReportPdfAsync(long id)
    {
        // 1. Timeout Fix: Simple query bina nested include ke
        var po = await _context.PurchaseOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (po == null) return null;

        // 2. STATUS FIX: GRNHeaders table mein check karein ki kya ye PO receive ho chuka hai
        // Hum check kar rahe hain ki kya is PurchaseOrderId ke liye koi GRN entry exist karti hai
        bool isReceived = await _context.GRNHeaders
            .AnyAsync(g => g.PurchaseOrderId == id && g.Status == "Received");

        string documentTitle = isReceived ? "TAX INVOICE" : "PURCHASE ORDER";

        // 3. Items fetch optimized: Timeout se bachne ke liye alag query
        var itemsWithNames = await (from item in _context.PurchaseOrderItems
                                    join prod in _context.Products on item.ProductId equals prod.Id
                                    where item.PurchaseOrderId == id
                                    select new
                                    {
                                        ProductName = prod.Name,
                                        item.Qty,
                                        item.Unit,
                                        item.Rate,
                                        item.Total
                                    }).ToListAsync();

        // 4. HTML Template with dynamic documentTitle
        var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: Arial, sans-serif; padding: 30px; color: #333; line-height: 1.4; }}
        .header-title {{ color: #1a73e8; margin: 0; text-align: center; font-size: 28px; }}
        .bill-label {{ border: 2px solid #333; display: inline-block; padding: 8px 25px; margin-top: 15px; font-weight: bold; text-transform: uppercase; }}
        .po-table {{ width: 100%; border-collapse: collapse; margin-top: 25px; }}
        .po-table th {{ background-color: #f8f9fa; border: 1px solid #dee2e6; padding: 12px; text-align: center; font-size: 14px; }}
        .po-table td {{ border: 1px solid #dee2e6; padding: 10px; font-size: 13px; }}
        .total-box {{ float: right; width: 40%; margin-top: 30px; border: none; }}
    </style>
</head>
<body>
    <div style='text-align: center; margin-bottom: 25px;'>
        <h1 class='header-title'>ELECTRIC INVENTORY</h1>
        <p style='margin: 5px 0; color: #666; font-size: 14px;'>PREMIUM INVENTORY MANAGEMENT SYSTEM</p>
        <h2 class='bill-label'>{documentTitle}</h2> </div>
    
    <hr style='border: 1px solid #eee;'/>
    
    <table style='width: 100%; margin: 20px 0;'>
        <tr>
            <td><strong>PO Number:</strong> {po.PoNumber}</td>
            <td style='text-align: right;'><strong>Date:</strong> {po.PoDate:dd MMM yyyy}</td>
        </tr>
        <tr>
            <td><strong>Supplier:</strong> {po.SupplierName}</td>
            <td style='text-align: right;'><strong>Type:</strong> {documentTitle}</td>
        </tr>
    </table>

    <table class='po-table'>
        <thead>
            <tr>
                <th style='text-align: left;'>Product Description</th>
                <th>Qty</th>
                <th>Unit</th>
                <th style='text-align: right;'>Rate</th>
                <th style='text-align: right;'>Total</th>
            </tr>
        </thead>
        <tbody>";

        foreach (var item in itemsWithNames)
        {
            htmlContent += $@"
        <tr>
            <td><b>{item.ProductName}</b></td>
            <td style='text-align: center;'>{item.Qty}</td>
            <td style='text-align: center;'>{item.Unit}</td>
            <td style='text-align: right;'>&#8377;{item.Rate:N2}</td>
            <td style='text-align: right;'>&#8377;{item.Total:N2}</td>
        </tr>";
        }

        htmlContent += $@"
        </tbody>
    </table>

    <div class='total-box'>
        <table style='width: 100%;'>
            <tr>
                <td><strong>Grand Total:</strong></td>
                <td style='text-align: right; color: #1a73e8; font-size: 1.3em;'>
                    <strong>&#8377;{po.GrandTotal:N2}</strong>
                </td>
            </tr>
        </table>
    </div>
    
    <div style='margin-top: 100px; clear: both;'>
        <p style='border-top: 1px solid #333; width: 220px; text-align: center; font-weight: bold;'>Authorized Signatory</p>
    </div>
</body>
</html>";

        // 5. PDF generation
        var pdfBytes = _converter.Convert(new HtmlToPdfDocument()
        {
            GlobalSettings = { PaperSize = PaperKind.A4, Margins = new MarginSettings { Top = 10, Bottom = 10 } },
            Objects = { new ObjectSettings { HtmlContent = htmlContent, WebSettings = { DefaultEncoding = "utf-8" } } }
        });

        return new PORepoPrintResponse
        {
            PdfBytes = pdfBytes,
            HeaderTitle = documentTitle // Controller ko dynamic title milega
        };
    }
}