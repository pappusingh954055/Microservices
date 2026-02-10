using Inventory.Application.Clients.DTOs;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http; // Needed for IHttpContextAccessor

namespace Inventory.Application.Clients
{
    public class CompanyClient : ICompanyClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CompanyClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("CompanyService");
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CompanyProfileDto> GetCompanyProfileAsync()
        {
            try
            {
                // 1. Get Token from Current Request
                var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        AuthenticationHeaderValue.Parse(token);
                }

                var response = await _httpClient.GetAsync("api/Company/profile"); // Case sensitive check
                if (response.IsSuccessStatusCode)
                {
                    var profile = await response.Content.ReadFromJsonAsync<CompanyProfileDto>();
                    
                    // Logo URL fix: If relative, prepend Base Address
                    if (profile != null && !string.IsNullOrEmpty(profile.LogoUrl) && !profile.LogoUrl.StartsWith("http"))
                    {
                        var baseUrl = _httpClient.BaseAddress?.ToString().TrimEnd('/');
                        profile.LogoUrl = $"{baseUrl}{profile.LogoUrl}";
                    }

                    return profile;
                }
                else
                {
                    Console.WriteLine($"Company Service Error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Company Microservice call failed: {ex.Message}");
            }
            return null;
        }
    }
}
