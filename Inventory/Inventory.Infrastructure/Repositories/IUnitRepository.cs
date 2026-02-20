using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Infrastructure.Repositories
{
    public class UnitRepository : IUnitRepository
    {
        private readonly InventoryDbContext _context;
        public UnitRepository(InventoryDbContext context) => _context = context;

        public async Task AddAsync(UnitMaster unit) => await _context.Units.AddAsync(unit);

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<UnitMaster>> GetAllAsync()
        {
            return await _context.Units
                         .AsNoTracking()
                         .ToListAsync();
        }

        public async Task<UnitMaster> GetByIdAsync(int id) => await _context.Units.FindAsync(id);

        public async Task UpdateAsync(UnitMaster unit)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ExistsAsync(string name)
        {
            return await _context.Units.AnyAsync(u => u.Name.ToLower() == name.ToLower());
        }
    }
}
