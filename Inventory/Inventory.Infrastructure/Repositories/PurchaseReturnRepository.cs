using ClosedXML.Excel;
using Inventory.Application.PurchaseReturn;
using Inventory.Application.PurchaseReturn.DTOs;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Net.Http.Json;

public class PurchaseReturnRepository : IPurchaseReturnRepository
{
    private readonly InventoryDbContext _context;
    private readonly HttpClient _httpClient;

    public PurchaseReturnRepository(InventoryDbContext context, 
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClient = httpClientFactory.CreateClient("SupplierServiceClient");
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

    public async Task<List<SupplierSelectDto>> GetSuppliersWithRejectionsAsync()
    {
        try
        {
            // 1. Inventory DB se un Suppliers ki IDs nikalein jinka maal reject hua hai [cite: 2026-02-04]
            var rejectedSupplierIds = await _context.GRNDetails
                .Where(gd => gd.RejectedQty > 0)
                .Select(gd => gd.GRNHeader.SupplierId)
                .Distinct()
                .Where(id => id > 0)
                .ToListAsync();

            // Agar DB mein koi rejected maal hi nahi hai, toh aage call karne ki zaroorat nahi [cite: 2026-02-04]
            if (rejectedSupplierIds == null || !rejectedSupplierIds.Any())
            {
                Console.WriteLine("DEBUG: No rejected items found in GRNDetails."); // Console check ke liye
                return new List<SupplierSelectDto>();
            }

            // 2. Supplier Microservice se Details mangwayein [cite: 2026-02-03]
            // Note: Check karein ki BaseAddress sahi set hai HttpClient mein
            var response = await _httpClient.PostAsJsonAsync("api/Supplier/get-by-ids", rejectedSupplierIds);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<SupplierSelectDto>>();

                if (result == null || !result.Any())
                {
                    Console.WriteLine($"DEBUG: Microservice returned success but 0 suppliers for IDs: {string.Join(",", rejectedSupplierIds)}");
                }

                return result ?? new List<SupplierSelectDto>();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"DEBUG: Microservice Error {response.StatusCode}: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            // Connection ya serialization error pakadne ke liye [cite: 2026-02-03]
            Console.WriteLine($"FATAL: Microservice Communication Error: {ex.Message}");
        }

        return new List<SupplierSelectDto>();
    }


    public async Task<bool> CreatePurchaseReturnAsync(PurchaseReturn returnData)
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

                // 3. Validation: Return hamesha rejected stock se hona chahiye [cite: 2026-02-04]
                if (item.ReturnQty <= 0) throw new Exception("Qty must be > 0");
                if (item.ReturnQty > grnDetail.RejectedQty)
                    throw new Exception($"Cannot return more than rejected: {grnDetail.RejectedQty}");

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

                // 5. STOCK LOGIC FIX (Important) [cite: 2026-02-04]
                // Aapke UI dashboard ke liye ReceivedQty kam karna zaroori hai.
                // Isse Total Received (100 -> 99) hoga aur Current Stock sahi dikhega.
                grnDetail.ReceivedQty -= item.ReturnQty;
                grnDetail.RejectedQty -= item.ReturnQty; // Rejected bucket (10 -> 9)

                // Safety: Quantity minus mein na jaye [cite: 2026-02-04]
                if (grnDetail.ReceivedQty < 0) grnDetail.ReceivedQty = 0;
                if (grnDetail.RejectedQty < 0) grnDetail.RejectedQty = 0;
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
        // 1. Database Query with Date Filters (Server-side) [cite: 2026-02-04]
        var query = _context.PurchaseReturns.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(x => x.ReturnDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(x => x.ReturnDate <= toDate.Value);

        // 2. Data fetch karke Supplier Names lana [cite: 2026-02-04]
        var allRecords = await query.ToListAsync();
        var supplierIds = allRecords.Select(x => (long)x.SupplierId).Distinct().ToList();
        var supplierNames = await GetSupplierNamesFromMicroservice(supplierIds);

        // 3. Search Logic (In-memory for Supplier Name support) [cite: 2026-02-04]
        var filteredData = allRecords.AsEnumerable();
        if (!string.IsNullOrEmpty(search))
        {
            var s = search.ToLower().Trim();
            filteredData = filteredData.Where(x =>
                x.ReturnNumber.ToLower().Contains(s) ||
                (supplierNames.TryGetValue((long)x.SupplierId, out var name) && name.ToLower().Contains(s)) ||
                (x.Remarks != null && x.Remarks.ToLower().Contains(s))
            );
        }

        // 4. Dynamic Sorting Logic [cite: 2026-02-04]
        if (sortOrder?.ToLower() == "asc")
            filteredData = filteredData.OrderBy(x => GetPropertyValue(x, sortField ?? "ReturnDate"));
        else
            filteredData = filteredData.OrderByDescending(x => GetPropertyValue(x, sortField ?? "ReturnDate"));

        // 5. Pagination [cite: 2026-02-04]
        var totalCount = filteredData.Count();
        var pagedData = filteredData
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToList();

        // 6. Detailed DTO Mapping with GRN [cite: 2026-02-04]
        var pagedIds = pagedData.Select(x => x.Id).ToList();
        var grnDetails = await _context.PurchaseReturnItems
            .Where(ri => pagedIds.Contains(ri.PurchaseReturnId))
            .Select(ri => new { ri.PurchaseReturnId, ri.GrnRef })
            .Distinct()
            .ToListAsync();

        var items = pagedData.Select(x => new PurchaseReturnListDto
        {
            Id = x.Id,
            ReturnNumber = x.ReturnNumber,
            ReturnDate = x.ReturnDate,
            SupplierName = supplierNames.TryGetValue((long)x.SupplierId, out var sName) ? sName : "Unknown",
            GrnRef = string.Join(", ", grnDetails.Where(g => g.PurchaseReturnId == x.Id).Select(g => g.GrnRef)),
            TotalAmount = x.GrandTotal, // ₹1,770 mapping
            Status = x.Status ?? "Completed" // Status synchronization
        }).ToList();

        return new PurchaseReturnPagedResponse { Items = items, TotalCount = totalCount };
    }

    // Reflection helper for dynamic sort fields [cite: 2026-02-04]
    private object GetPropertyValue(object obj, string propertyName)
    {
        return obj.GetType().GetProperty(propertyName)?.GetValue(obj, null) ?? obj;
    }

    

    private async Task<Dictionary<long, string>> GetSupplierNamesFromMicroservice(List<long> supplierIds)
    {
        var dict = new Dictionary<long, string>();
        if (supplierIds == null || !supplierIds.Any()) return dict;

        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/Supplier/get-by-ids", supplierIds);

            if (response.IsSuccessStatusCode)
            {
                var suppliers = await response.Content.ReadFromJsonAsync<List<SupplierSelectDto>>();
                if (suppliers != null)
                {
                    // FIX 2: Explicitly cast Id to long during dictionary creation
                    dict = suppliers.ToDictionary(x => (long)x.Id, x => x.Name);
                }
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
        var query = await (from pr in _context.PurchaseReturns
                           where pr.Id == id
                           select new
                           {
                               Header = pr,
                               Items = (from pri in _context.PurchaseReturnItems
                                        join p in _context.Products on pri.ProductId equals p.Id
                                        where pri.PurchaseReturnId == id
                                        select new PurchaseReturnItemDto
                                        {
                                            ProductId = pri.ProductId,
                                            ProductName = p.Name,
                                            GrnRef = pri.GrnRef,
                                            ReturnQty = pri.ReturnQty,
                                            Rate = pri.Rate,
                                            GstPercent = pri.GstPercent, // Fixed
                                            TaxAmount = pri.TaxAmount,
                                            TotalAmount = pri.TotalAmount // 1770 mapping
                                        }).ToList()
                           }).FirstOrDefaultAsync();

        if (query == null) return null;

        var data = query.Header;

        // Microservice se Supplier Name lana [cite: 2026-02-04]
        var supplierDict = await GetSupplierNamesFromMicroservice(new List<long> { (long)data.SupplierId });
        string sName = supplierDict.ContainsKey((long)data.SupplierId) ? supplierDict[(long)data.SupplierId] : "Unknown";

        return new PurchaseReturnDetailDto
        {
            Id = data.Id,
            ReturnNumber = data.ReturnNumber,
            ReturnDate = data.ReturnDate,
            SupplierId = data.SupplierId,
            SupplierName = sName,
            Status = data.Status, // Mapping verify ho gayi
            Remarks = data.Remarks,
            Items = query.Items,
            SubTotal = data.SubTotal,   // 1500
            TaxAmount = data.TotalTax,  // 270
            GrandTotal = data.GrandTotal // 1770
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