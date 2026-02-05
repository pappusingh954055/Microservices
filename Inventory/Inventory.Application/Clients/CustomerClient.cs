using Inventory.Application.Clients;
using System.Net.Http.Json;

namespace Inventory.Infrastructure.Clients;

public class CustomerClient : ICustomerClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CustomerClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Dictionary<int, string>> GetCustomerNamesAsync(List<int> customerIds)
    {
        // Aapka wahi "CustomerService" yahan call ho raha hai [cite: 2026-02-05]
        var client = _httpClientFactory.CreateClient("CustomerService");

        // Batch API call: Customer Microservice ko IDs bhejein
        var response = await client.PostAsJsonAsync("api/customers/get-names-by-ids", customerIds);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Dictionary<int, string>>() ?? new();
        }

        return new Dictionary<int, string>();
    }

    public async Task<List<CustomerLookupDto>> GetCustomersForLookupAsync()
    {
        var client = _httpClientFactory.CreateClient("CustomerService"); // Aapka 7173 wala client
        return await client.GetFromJsonAsync<List<CustomerLookupDto>>("api/customers/lookup") ?? new();
    }
}