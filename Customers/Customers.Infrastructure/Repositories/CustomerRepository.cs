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
        //public async Task<Dictionary<int, string>> GetCustomerNamesByIdsAsync(List<int> ids)
        //{
        //    return await _context.Customers
        //        .Where(c => ids.Contains(c.Id))
        //        .ToDictionaryAsync(c => c.Id, c => c.CustomerName);
        //}
        public async Task<Dictionary<int, string>> GetCustomerNamesByIdsAsync(List<int> ids)
        {
            // 1. Validation: Agar list empty hai toh turant return karein taaki DB trip bache
            if (ids == null || !ids.Any())
                return new Dictionary<int, string>();

            // 2. Duplicate IDs remove karein taaki SQL IN clause chhota rahe
            var distinctIds = ids.Distinct().ToList();

            return await _context.Customers
                .AsNoTracking() // 3. Tracking off karein, ye performance ke liye bahut zaroori hai [cite: 2026-02-06]
                .Where(c => distinctIds.Contains(c.Id))
                .Select(c => new { c.Id, c.CustomerName }) // 4. Sirf wahi columns layein jo chahiye
                .ToDictionaryAsync(x => x.Id, x => x.CustomerName);
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

        public async Task<List<CustomerLookupDto>> GetCustomersLookupAsync()
        {
            // Database se sirf Id aur CustomerName select karna [cite: 2026-02-05]
            return await _context.Customers
                .AsNoTracking() // Performance ke liye [cite: 2026-02-05]
                .Select(c => new CustomerLookupDto
                {
                    Id = c.Id,
                    Name = c.CustomerName // Aapki table mein 'CustomerName' column hai [cite: 2026-02-05]
                })
                .ToListAsync();
        }

        public async Task<List<int>> GetIdsByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return new List<int>();
            return await _context.Customers
                .AsNoTracking()
                .Where(c => EF.Functions.Like(c.CustomerName, $"%{name}%"))
                .Select(c => c.Id)
                .ToListAsync();
        }
    }
}
