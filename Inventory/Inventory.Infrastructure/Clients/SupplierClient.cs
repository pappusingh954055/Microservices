using Inventory.Application.Clients;
using Inventory.Application.PurchaseReturn;
using Microsoft.AspNetCore.Http; // Added
using System;
using System.Collections.Generic;
using System.Linq; // Added for First/Last
using System.Net.Http;
using System.Net.Http.Headers; // Added
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Inventory.Infrastructure.Clients
{
    public class SupplierClient : ISupplierClient
    {
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SupplierClient(IHttpClientFactory factory, IHttpContextAccessor httpContextAccessor)
        {
            _client = factory.CreateClient("SupplierServiceClient"); 
            _httpContextAccessor = httpContextAccessor;
        }

        private void AddAuthorizationHeader()
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
                        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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
            try
            {
                AddAuthorizationHeader(); // Attach Token
                var response = await _client.PostAsJsonAsync("api/Supplier/get-by-ids", supplierIds);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<List<SupplierSelectDto>>();
                    return result ?? new List<SupplierSelectDto>();
                }
                return new List<SupplierSelectDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupplierClient] Error fetching suppliers: {ex.Message}");
                return new List<SupplierSelectDto>();
            }
        }

        public async Task<bool> RecordPurchaseAsync(int supplierId, decimal amount, string referenceId, string description, string createdBy)
        {
            try
            {
                AddAuthorizationHeader(); // Attach Token
                var payload = new 
                {
                    SupplierId = supplierId,
                    Amount = amount,
                    ReferenceId = referenceId, 
                    Description = description,
                    TransactionDate = DateTime.Now,
                    CreatedBy = createdBy
                };

                var response = await _client.PostAsJsonAsync("api/finance/purchase-entry", payload);
                if (!response.IsSuccessStatusCode)
                {
                     var content = await response.Content.ReadAsStringAsync();
                     throw new Exception($"Supplier Service Purchase Failed: {response.StatusCode} - {content} - URL: {_client.BaseAddress}api/finance/purchase-entry");
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupplierClient] Error recording purchase: {ex.Message}");
                return false;
            }
        }

        public async Task<Dictionary<string, decimal>> GetGRNPaymentStatusesAsync(List<string> grnNumbers)
        {
                if (grnNumbers == null || grnNumbers.Count == 0) 
                    return new Dictionary<string, decimal>();

                AddAuthorizationHeader(); // Attach Token
                
                var response = await _client.PostAsJsonAsync("api/finance/get-grn-statuses", grnNumbers);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<Dictionary<string, decimal>>();
                    return result ?? new Dictionary<string, decimal>();
                }
                
                // Throw Exception so Repository sees it
                throw new HttpRequestException($"Supplier Service Request Failed: {response.StatusCode} at {_client.BaseAddress}");
        }

        public async Task<Dictionary<int, decimal>> GetSupplierBalancesAsync(List<int> supplierIds)
        {
            if (supplierIds == null || !supplierIds.Any()) return new Dictionary<int, decimal>();

            try
            {
                AddAuthorizationHeader(); // Attach Token
                var response = await _client.PostAsJsonAsync("api/finance/get-balances", supplierIds);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Dictionary<int, decimal>>() ?? new Dictionary<int, decimal>();
                }
                return new Dictionary<int, decimal>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupplierClient] Connect Error: {ex.Message}");
                // Return empty dictionary on failure so we don't block the listing
                return new Dictionary<int, decimal>();
            }
        }
    }
}
