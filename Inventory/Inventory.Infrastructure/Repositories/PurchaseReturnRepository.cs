using ClosedXML.Excel;
using Inventory.Application.Clients;
using Inventory.Application.PurchaseReturn;
using Inventory.Application.PurchaseReturn.DTOs;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Net.Http.Json;

public class PurchaseReturnRepository : IPurchaseReturnRepository
{
    private readonly InventoryDbContext _context;
    private readonly ISupplierClient _supplierClient;

    public PurchaseReturnRepository(InventoryDbContext context, 
        ISupplierClient supplierClient)
    {
        _context = context;
        _supplierClient = supplierClient;
    }

    // 1. UI Form ke liye Rejected Items fetch karein
    public async Task<List<RejectedItemDto>> GetRejectedItemsBySupplierAsync(int supplierId)
    {
        // 1. GRNHeaders aur GRNDetails ko join karke rejected stock filter karein
        var rejectedItems = await _context.GRNDetails
            .Include(gd => gd.GRNHeader) // Navigation property mapping
            .Where(gd => gd.GRNHeader.SupplierId == supplierId && gd.RejectedQty > 0)
            .Select(gd => new RejectedItemDto
            {
                ProductId = gd.ProductId, // uniqueidentifier
                                          // ProductName agar detail table mein nahi hai toh Product navigation use karein
                ProductName = gd.Product != null ? gd.Product.Name : "Unknown Product",
                GrnRef = gd.GRNHeader.GRNNumber, // Tracking ke liye GRNNumber use kiya hai
                RejectedQty = gd.RejectedQty, // Decimal(18,2)
                Rate = gd.UnitRate // Schema mein column name UnitRate hai
            })
            .ToListAsync();

        return rejectedItems;
    }

    public async Task<List<SupplierSelectDto>> GetSuppliersForPurchaseReturnAsync()
    {
        try
        {
            // 1. Updated Join Query: Ab ye sirf RejectedQty nahi dekhega,
            // balki un sabhi Suppliers ko layega jinse GRN receive hua hai.
            var allSupplierIds = await (from gh in _context.GRNHeaders
                                        select gh.SupplierId)
                                       .Distinct()
                                       .ToListAsync();

            // Agar kisi bhi supplier se koi GRN nahi hua toh empty list return karein
            if (allSupplierIds == null || !allSupplierIds.Any())
            {
                return new List<SupplierSelectDto>();
            }

            // 2. Supplier Microservice Call: Un IDs ke basis par Names aur details fetch karein
            // Isse "ABC Enterprises" jaise naam dropdown mein bind honge
            var suppliers = await _supplierClient.GetSuppliersByIdsAsync(allSupplierIds);

            return suppliers ?? new List<SupplierSelectDto>();
        }
        catch (Exception ex)
        {
            // Error logging taaki debug karna asaan ho
            Console.WriteLine($"Error in GetSuppliersForPurchaseReturnAsync: {ex.Message}");
        }
        return new List<SupplierSelectDto>();
    }


    public async Task<List<ReceivedStockDto>> GetReceivedStockBySupplierAsync(int supplierId)
    {
        // Accepted qty fetch karein jo warehouse mein hai
        var receivedItems = await _context.GRNDetails
            .Include(gd => gd.GRNHeader)
            .Where(gd => gd.GRNHeader.SupplierId == supplierId && (gd.ReceivedQty - gd.RejectedQty) > 0)
            .Select(gd => new ReceivedStockDto
            {
                ProductId = gd.ProductId,
                ProductName = gd.Product != null ? gd.Product.Name : "Unknown Product",
                GrnRef = gd.GRNHeader.GRNNumber,
                AvailableQty = gd.ReceivedQty - gd.RejectedQty, // This is AcceptedQty
                Rate = gd.UnitRate
            })
            .ToListAsync();

        return receivedItems;
    }


    public async Task<bool> CreatePurchaseReturnAsync(PurchaseReturn returnData)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Unique ID aur Return Number generate karein [cite: 2026-02-04]
                if (returnData.Id == Guid.Empty) returnData.Id = Guid.NewGuid();
                returnData.ReturnNumber = $"PR-{DateTime.Now:yyyyMMddHHmmss}";

                decimal totalHeaderTax = 0;
                decimal totalHeaderSubTotal = 0;

                foreach (var item in returnData.Items)
                {
                    // 2. Precise GRN Detail fetch using Product and Ref [cite: 2026-02-04]
                    var grnDetail = await _context.GRNDetails
                        .Include(gd => gd.GRNHeader)
                        .FirstOrDefaultAsync(gd => gd.ProductId == item.ProductId
                                             && gd.GRNHeader.GRNNumber == item.GrnRef);

                    if (grnDetail == null) throw new Exception($"GRN not found for {item.GrnRef}");

                    // 3. Validation: Return check [cite: 2026-02-04]
                    if (item.ReturnQty <= 0) throw new Exception("Qty must be > 0");
                    
                    // Available = Rejected + Accepted
                    decimal totalAvailable = grnDetail.ReceivedQty; 
                    if (item.ReturnQty > totalAvailable)
                        throw new Exception($"Cannot return more than available stock: {totalAvailable}");

                    // 4. Financials mapping from PO
                    var poItem = await _context.PurchaseOrderItems
                        .FirstOrDefaultAsync(poi => poi.ProductId == item.ProductId);

                    if (poItem != null)
                    {
                        item.GstPercent = poItem.GstPercent;
                        item.DiscountPercent = poItem.DiscountPercent;

                        decimal baseAmount = item.ReturnQty * item.Rate;
                        decimal discountAmt = baseAmount * (item.DiscountPercent / 100);
                        decimal taxableAmount = baseAmount - discountAmt;
                        decimal itemTax = taxableAmount * (item.GstPercent / 100);

                        item.TaxAmount = itemTax;
                        item.TotalAmount = taxableAmount + itemTax;

                        totalHeaderSubTotal += taxableAmount;
                        totalHeaderTax += itemTax;
                    }

                    // 5. STOCK LOGIC FIX (Important)
                    // Pehle RejectedQty se deduct karein, fir bacha hua AcceptedQty (Received - Rejected) se
                    decimal qtyToReturn = item.ReturnQty;
                    
                    if (grnDetail.RejectedQty >= qtyToReturn)
                    {
                        grnDetail.RejectedQty -= qtyToReturn;
                    }
                    else
                    {
                        // Some part is from rejected, rest from accepted
                        // Note: AcceptedQty is virtual (ReceivedQty - RejectedQty)
                        // So decreasing ReceivedQty automatically decreases AcceptedQty if we don't decrease RejectedQty as much
                        // Wait, logic:
                        // Total Received = 10 (Rejected 2, Accepted 8)
                        // Return 5:
                        // Rejected becomes 0 (Returned 2)
                        // Accepted becomes 5 (Returned 3)
                        // Total Received becomes 5.
                        
                        decimal fromRejected = grnDetail.RejectedQty;
                        grnDetail.RejectedQty = 0;
                        // Rest is already handled by ReceivedQty -= qtyToReturn
                    }

                    grnDetail.ReceivedQty -= qtyToReturn;
                    if (grnDetail.ReceivedQty < 0) grnDetail.ReceivedQty = 0;

                    // ========================================================
                    // ADDITIONAL LOGIC: UPDATE PRODUCT CURRENT STOCK
                    // ========================================================
                    var product = await _context.Products
                        .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                    if (product != null)
                    {
                        product.CurrentStock -= item.ReturnQty;
                    }
                }

                // 6. Header Totals Update
                returnData.SubTotal = totalHeaderSubTotal;
                returnData.TotalTax = totalHeaderTax;
                returnData.GrandTotal = totalHeaderSubTotal + totalHeaderTax;

                _context.PurchaseReturns.Add(returnData);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return false;
            }
        });
    }


    public async Task<PurchaseReturnPagedResponse> GetPurchaseReturnsAsync(
    string? search,
    int pageIndex,
    int pageSize,
    DateTime? fromDate = null,
    DateTime? toDate = null,
    string? sortField = "ReturnDate",
    string? sortOrder = "desc")
    {
        // 1. Initial Query with NoTracking for high performance
        var query = _context.PurchaseReturns
            .AsNoTracking()
            .AsQueryable();

        // 2. Date Filtering Logic
        if (fromDate.HasValue)
            query = query.Where(x => x.ReturnDate >= fromDate.Value);

        if (toDate.HasValue)
        {
            var endOfToDate = toDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(x => x.ReturnDate <= endOfToDate);
        }

        // 3. Robust Searching Logic
        if (!string.IsNullOrEmpty(search))
        {
            var s = search.ToLower().Trim();

            // Step A: Microservice se matching Supplier IDs fetch karein
            var matchedSupplierIds = await GetSupplierIdsByNameFromMicroservice(s);

            // Step B: Search by ReturnNumber, Remarks, or Supplier
            query = query.Where(x =>
                (x.ReturnNumber != null && x.ReturnNumber.ToLower().Contains(s)) ||
                (x.Remarks != null && x.Remarks.ToLower().Contains(s)) ||
                (matchedSupplierIds != null && matchedSupplierIds.Contains((long)x.SupplierId))
            );
        }

        // 4. Server-side Count (Fast performance)
        var totalCount = await query.CountAsync();

        // 5. SORTING LOGIC: Mapping UI fields to DB columns
        bool isDesc = sortOrder?.ToLower() == "desc" || string.IsNullOrEmpty(sortOrder);
        string effectiveSortField = sortField?.ToLower().Trim() switch
        {
            "totalamount" or "grandtotal" => "GrandTotal",
            "returnnumber" => "ReturnNumber",
            "returndate" => "ReturnDate",
            "id" => "Id",
            _ => "ReturnDate" // Default: ReturnDate load orders by date
        };

        if (isDesc)
            query = query.OrderByDescending(x => EF.Property<object>(x, effectiveSortField));
        else
            query = query.OrderBy(x => EF.Property<object>(x, effectiveSortField));

        // 6. Execution & Pagination
        var pagedData = await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync();

        if (pagedData == null || !pagedData.Any())
            return new PurchaseReturnPagedResponse { Items = new List<PurchaseReturnListDto>(), TotalCount = totalCount };

        // 7. Bulk Data Enrichment (Supplier Names & Items)
        var supplierIds = pagedData.Select(x => (long)x.SupplierId).Distinct().ToList();
        var supplierNames = await GetSupplierNamesFromMicroservice(supplierIds);

        var pagedIds = pagedData.Select(x => x.Id).ToList();
        
        // Items enrichment using Split Query pattern for efficiency
        var grnDetailsList = await _context.PurchaseReturnItems
            .AsNoTracking()
            .Where(ri => pagedIds.Contains(ri.PurchaseReturnId))
            .Select(ri => new { ri.PurchaseReturnId, ri.GrnRef })
            .Distinct()
            .ToListAsync();

        var grnLookup = grnDetailsList
            .GroupBy(x => x.PurchaseReturnId)
            .ToDictionary(g => g.Key, g => string.Join(", ", g.Select(i => i.GrnRef).Distinct()));

        // 8. Final Mapping
        var items = pagedData.Select(x => new PurchaseReturnListDto
        {
            Id = x.Id,
            ReturnNumber = x.ReturnNumber,
            ReturnDate = x.ReturnDate,
            SupplierName = supplierNames.GetValueOrDefault((long)x.SupplierId, "Unknown"),
            GrnRef = grnLookup.GetValueOrDefault(x.Id, "N/A"),
            TotalAmount = x.GrandTotal,
            Status = "Completed"
        }).ToList();

        return new PurchaseReturnPagedResponse { Items = items, TotalCount = totalCount };
    }

    // 9. Helper Method to fetch matching Supplier IDs [cite: 2026-02-04]
    private async Task<List<long>> GetSupplierIdsByNameFromMicroservice(string name)
    {
        try
        {
            // IMPORTANT: Verify karein ki aapka microservice endpoint IDs return kar raha hai [cite: 2026-02-04]
            // var response = await _httpClient.GetFromJsonAsync<List<long>>($"api/suppliers/search-ids?name={name}");
            // return response ?? new List<long>();
            return new List<long>(); // Temporary empty for now to avoid compilation error if _httpClient removed
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Supplier search error: {ex.Message}");
            return new List<long>();
        }
    }



    private async Task<Dictionary<long, string>> GetSupplierNamesFromMicroservice(List<long> supplierIds)
    {
        var dict = new Dictionary<long, string>();
        if (supplierIds == null || !supplierIds.Any()) return dict;

        try
        {
            var suppliers = await _supplierClient.GetSuppliersByIdsAsync(supplierIds.Select(x => (int)x).ToList());
            if (suppliers != null)
            {
                dict = suppliers.ToDictionary(x => (long)x.Id, x => x.Name);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching supplier names: {ex.Message}");
        }

        return dict;
    }


    public async Task<PurchaseReturnDetailDto?> GetPurchaseReturnByIdAsync(Guid id)
    {
        // 1. Optimize: Eager Loading use karein aur database par ek hi lightweight call bhein
        var purchaseReturn = await _context.PurchaseReturns
            .AsNoTracking() // Read-only query ke liye best performance
            .Include(x => x.Items) // Navigation property se items fetch karein
            .FirstOrDefaultAsync(x => x.Id == id);

        if (purchaseReturn == null) return null;

        // 2. Optimization: Items aur Products ka mapping database level ki jagah memory mein karein
        // Taaki nested join ka timeout load khatam ho jaye
        var itemDtos = await (from pri in _context.PurchaseReturnItems.AsNoTracking()
                              join p in _context.Products.AsNoTracking() on pri.ProductId equals p.Id
                              where pri.PurchaseReturnId == id
                              select new PurchaseReturnItemDto
                              {
                                  ProductId = pri.ProductId,
                                  ProductName = p.Name,
                                  GrnRef = pri.GrnRef,
                                  ReturnQty = pri.ReturnQty,
                                  Rate = pri.Rate,
                                  GstPercent = pri.GstPercent,
                                  TaxAmount = pri.TaxAmount,
                                  TotalAmount = pri.TotalAmount
                              }).ToListAsync();

        // 3. Supplier Name fetch karein
        var supplierDict = await GetSupplierNamesFromMicroservice(new List<long> { (long)purchaseReturn.SupplierId });
        string sName = supplierDict.ContainsKey((long)purchaseReturn.SupplierId)
                       ? supplierDict[(long)purchaseReturn.SupplierId] : "Unknown";

        // 4. Final DTO Mapping (No functional changes, purely performance fix)
        return new PurchaseReturnDetailDto
        {
            Id = purchaseReturn.Id,
            ReturnNumber = purchaseReturn.ReturnNumber,
            ReturnDate = purchaseReturn.ReturnDate,
            SupplierId = purchaseReturn.SupplierId,
            SupplierName = sName,
            Status = "Completed", // Existing requirement status fix
            Remarks = purchaseReturn.Remarks,
            Items = itemDtos,
            SubTotal = purchaseReturn.SubTotal,
            TaxAmount = purchaseReturn.TotalTax,
            GrandTotal = purchaseReturn.GrandTotal
        };
    }

    public async Task<byte[]> ExportPurchaseReturnsToExcelAsync(DateTime? fromDate, DateTime? toDate)
    {
        // 1. Database se Purchase Returns fetch karein [cite: 2026-02-04]
        var data = await _context.PurchaseReturns
            .AsNoTracking()
            .Where(x => (!fromDate.HasValue || x.ReturnDate >= fromDate) &&
                        (!toDate.HasValue || x.ReturnDate <= toDate))
            .OrderByDescending(x => x.ReturnDate)
            .ToListAsync();

        // 2. Microservice se Supplier Names fetch karein [cite: 2026-02-04]
        var supplierIds = data.Select(x => (long)x.SupplierId).Distinct().ToList();
        var supplierNamesDict = await GetSupplierNamesFromMicroservice(supplierIds); // Aapka existing helper method

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Debit Notes");

            // Headers definition [cite: 2026-02-04]
            string[] headers = { "Return #", "Date", "Supplier Name", "Sub Total", "Tax Amount", "Grand Total", "Remarks" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#4F81BD");
                cell.Style.Font.FontColor = XLColor.White;
            }

            // 3. Fill Data Rows with Supplier Names [cite: 2026-02-04]
            int currentRow = 2;
            foreach (var item in data)
            {
                worksheet.Cell(currentRow, 1).Value = item.ReturnNumber;
                worksheet.Cell(currentRow, 2).Value = item.ReturnDate.ToString("dd-MMM-yyyy");

                // --- FIX: Microservice dictionary se naam map karein --- [cite: 2026-02-04]
                string sName = supplierNamesDict.ContainsKey((long)item.SupplierId)
                               ? supplierNamesDict[(long)item.SupplierId]
                               : "Unknown Supplier";
                worksheet.Cell(currentRow, 3).Value = sName;

                worksheet.Cell(currentRow, 4).Value = item.SubTotal;
                worksheet.Cell(currentRow, 5).Value = item.TotalTax;
                worksheet.Cell(currentRow, 6).Value = item.GrandTotal;
                worksheet.Cell(currentRow, 7).Value = item.Remarks;

                // Formatting [cite: 2026-02-04]
                worksheet.Range(currentRow, 4, currentRow, 6).Style.NumberFormat.Format = "₹ #,##0.00";
                currentRow++;
            }

            worksheet.Columns().AdjustToContents();

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
        }
    }
}