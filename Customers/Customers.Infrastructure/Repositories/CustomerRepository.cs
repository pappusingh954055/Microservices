using Customers.Application.Common.Interfaces;
using Customers.Domain.Entities;
using Customers.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Customers.Infrastructure.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly CustomerDbContext _context;

        public CustomerRepository(CustomerDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Customer>> GetAllAsync()
            => await _context.Customers.ToListAsync();

        public async Task<Customer?> GetByIdAsync(int id)
            => await _context.Customers.FindAsync(id);

        public async Task UpdateAsync(Customer customer)
        {
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Customer customer)
        {
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// bulk customer call
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<Dictionary<int, string>> GetCustomerNamesByIdsAsync(List<int> ids)
        {
            return await _context.Customers
                .Where(c => ids.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.CustomerName);
        }

        /// <summary>
        /// Single call
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<string?> GetCustomerNameByIdAsync(int id)
        {
            // Database se sirf Name column select karein performance ke liye
            return await _context.Customers
                .Where(c => c.Id == id)
                .Select(c => c.CustomerName)
                .FirstOrDefaultAsync();
        }
    }
}
