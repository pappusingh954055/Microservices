using Customers.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Customers.Application.Common.Interfaces
{
    public interface ICustomerRepository
    {
        Task AddAsync(Customer customer);
        Task<List<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync(int id);
        Task UpdateAsync(Customer customer);
        Task DeleteAsync(Customer customer);

        //bulk customer call
        Task<Dictionary<int, string>> GetCustomerNamesByIdsAsync(List<int> ids);

        //single cusomer call
        Task<string?> GetCustomerNameByIdAsync(int id);

        Task<List<CustomerLookupDto>> GetCustomersLookupAsync();
    }
}
