using Inventory.Application.PurchaseReturn;
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
        // 1. IDs fetch karein aur '0' ya null ko filter karein
        var rejectedSupplierIds = await _context.GRNDetails
            .Where(gd => gd.RejectedQty > 0)
            .Select(gd => gd.GRNHeader.SupplierId)
            .Distinct()
            .Where(id => id > 0) // Sirf valid IDs bhejien
            .ToListAsync();

        if (rejectedSupplierIds == null || !rejectedSupplierIds.Any())
        {
            return new List<SupplierSelectDto>();
        }

        try
        {
            // 2. Microservice Call [cite: 2026-02-03]
            // Check karein ki Supplier Microservice ka endpoint exactly 'get-by-ids' hi hai
            var response = await _httpClient.PostAsJsonAsync("api/Supplier/get-by-ids", rejectedSupplierIds);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<SupplierSelectDto>>();

                // Agar result null hai toh empty list bhejien
                return result ?? new List<SupplierSelectDto>();
            }
        }
        catch (Exception ex)
        {
            // Yahan console par error dekhein ki kya connection fail ho raha hai
            Console.WriteLine($"Microservice Communication Error: {ex.Message}");
        }

        return new List<SupplierSelectDto>();
    }

    // 2. Purchase Return Save karein aur Stock Update karein [cite: 2026-02-03]
    public async Task<bool> CreatePurchaseReturnAsync(PurchaseReturn returnData)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // A. Unique Return Number generate karein
            returnData.ReturnNumber = $"PR-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
            returnData.Id = Guid.NewGuid();

            // B. Header aur Items save karein
            _context.PurchaseReturns.Add(returnData);

            // C. GRN DETAIL UPDATE LOGIC (Stocks table bypass)
            foreach (var item in returnData.Items)
            {
                // Hum us specific GRN Detail row ko dhundenge jiske against return ho raha hai
                // GrnRef mein humne GRNNumber store kiya tha
                var grnDetail = await _context.GRNDetails
                    .FirstOrDefaultAsync(gd => gd.ProductId == item.ProductId
                                         && gd.GRNHeader.GRNNumber == item.GrnRef);

                if (grnDetail != null)
                {
                    // Logic: Rejected quantity ab return ho rahi hai
                    // Isse aapka 'Rejected' bucket balance ho jayega
                    grnDetail.RejectedQty -= item.ReturnQty;

                    // Optional: Agar aap track karna chahte hain kitna return ho chuka hai
                    // toh aap GRNDetails mein 'ReturnedQty' column add kar sakte hain
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return false;
        }
    }
}