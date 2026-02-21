using Inventory.Application.Clients;
using Inventory.Application.PurchaseReturn;
using System.Net.Http.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class SupplierClient : ISupplierClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;

    public SupplierClient(IHttpClientFactory httpClientFactory, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    private void AddAuthorizationHeader(HttpClient client)
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null && context.Request.Headers.ContainsKey("Authorization"))
            {
                var authHeader = context.Request.Headers["Authorization"].ToString();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var token = authHeader.Substring("Bearer ".Length).Trim();
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SupplierClient] Failed to attach auth token: {ex.Message}");
        }
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

    public async Task<bool> RecordPurchaseReturnAsync(int supplierId, decimal amount, string referenceId, string description, string createdBy)
    {
        var client = _httpClientFactory.CreateClient("SupplierServiceClient");
        var payload = new
        {
            SupplierId = supplierId,
            Amount = amount, // This should be positive, backend logic handles it as debit note
            ReferenceId = referenceId,
            Description = description,
            TransactionDate = DateTime.Now,
            CreatedBy = createdBy,
            TransactionType = "DebitNote"
        };

        // Using same finance entry endpoint but with specific type
        var response = await client.PostAsJsonAsync("api/finance/purchase-entry", payload);
        return response.IsSuccessStatusCode;
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

    public async Task<List<int>> SearchSupplierIdsByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return new List<int>();

        var client = _httpClientFactory.CreateClient("SupplierServiceClient");
        AddAuthorizationHeader(client);

        try
        {
            var response = await client.GetAsync($"api/Supplier/search-ids?name={Uri.EscapeDataString(name)}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<int>>() ?? new List<int>();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[SupplierClient] Search failed: {response.StatusCode} - {error}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SupplierClient] Search Exception: {ex.Message}");
        }

        return new List<int>();
    }
}

