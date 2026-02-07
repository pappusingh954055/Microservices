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
        
        var client = _httpClientFactory.CreateClient("CustomerService");

        // Batch API call: Customer Microservice ko IDs bhejein
        var response = await client.PostAsJsonAsync("api/customers/get-names", customerIds);

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

    public async Task<List<int>> SearchCustomerIdsByNameAsync(string searchName)
    {
        if (string.IsNullOrWhiteSpace(searchName)) return new List<int>();

        var client = _httpClientFactory.CreateClient("CustomerService");

        try
        {
            var response = await client.GetAsync($"api/customers/search-ids?name={Uri.EscapeDataString(searchName)}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<int>>() ?? new List<int>();
            }
        }
        catch (Exception ex)
        {
            // Logging error for better debugging
            Console.WriteLine($"Error in SearchCustomerIdsByNameAsync: {ex.Message}");
        }

        return new List<int>();
    }
}