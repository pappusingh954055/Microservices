using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Clients
{
    public interface ICustomerClient
    {
        // Batch call method jo IDs lekar Name ka Dictionary (Map) dega [cite: 2026-02-05]
        Task<Dictionary<int, string>> GetCustomerNamesAsync(List<int> customerIds);

        Task<List<CustomerLookupDto>> GetCustomersForLookupAsync();

        Task<List<int>> SearchCustomerIdsByNameAsync(string searchName);

        Task RecordSaleAsync(int customerId, decimal amount, string referenceId, string description, string createdBy);
        Task<CustomerLookupDto?> GetCustomerByIdAsync(int id);
    }
}
