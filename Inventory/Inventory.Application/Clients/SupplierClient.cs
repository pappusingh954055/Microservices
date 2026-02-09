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
}