using Inventory.Application.Clients;
using Inventory.Application.PurchaseReturn;
using System.Net.Http.Json;

public class SupplierClient : ISupplierClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public SupplierClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<SupplierSelectDto>> GetSuppliersByIdsAsync(List<int> supplierIds)
    {
        var client = _httpClientFactory.CreateClient("SupplierServiceClient");
        if (supplierIds == null || !supplierIds.Any()) return new List<SupplierSelectDto>();

        // Supplier microservice ke endpoint ko hit karna      

        var response = await client.PostAsJsonAsync("api/Supplier/get-by-ids", supplierIds);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<SupplierSelectDto>>() ?? new List<SupplierSelectDto>();
        }

        return new List<SupplierSelectDto>();
    }

    public async Task<bool> RecordPurchaseAsync(int supplierId, decimal amount, string referenceId, string description, string createdBy)
    {
        var client = _httpClientFactory.CreateClient("SupplierServiceClient");
        var payload = new
        {
            SupplierId = supplierId,
            Amount = amount,
            ReferenceId = referenceId,
            Description = description,
            TransactionDate = DateTime.Now,
            CreatedBy = createdBy
        };

        var response = await client.PostAsJsonAsync("api/finance/purchase-entry", payload);
        return response.IsSuccessStatusCode;
    }

    public async Task<Dictionary<string, decimal>> GetGRNPaymentStatusesAsync(List<string> grnNumbers)
    {
        var client = _httpClientFactory.CreateClient("SupplierServiceClient");
        var response = await client.PostAsJsonAsync("api/finance/get-grn-statuses", grnNumbers);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Dictionary<string, decimal>>() ?? new Dictionary<string, decimal>();
        }

        return new Dictionary<string, decimal>();
    }

    public async Task<Dictionary<int, decimal>> GetSupplierBalancesAsync(List<int> supplierIds)
    {
        var client = _httpClientFactory.CreateClient("SupplierServiceClient");
        var response = await client.PostAsJsonAsync("api/finance/get-balances", supplierIds);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Dictionary<int, decimal>>() ?? new Dictionary<int, decimal>();
        }

        return new Dictionary<int, decimal>();
    }
}
