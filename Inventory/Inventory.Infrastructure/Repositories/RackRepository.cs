using Inventory.Application.Common.Interfaces;
using Inventory.Domain.Entities;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Repositories;

public class RackRepository : IRackRepository
{
    private readonly InventoryDbContext _context;

    public RackRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Rack rack)
    {
        await _context.Racks.AddAsync(rack);
    }

    public Task UpdateAsync(Rack rack)
    {
        _context.Racks.Update(rack);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Rack rack)
    {
        _context.Racks.Remove(rack);
        return Task.CompletedTask;
    }

    public async Task<List<Rack>> GetAllAsync()
    {
        return await _context.Racks
            .Include(r => r.Warehouse)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Rack>> GetByWarehouseIdAsync(Guid warehouseId)
    {
        return await _context.Racks
            .Where(r => r.WarehouseId == warehouseId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Rack?> GetByIdAsync(Guid id)
    {
        return await _context.Racks.FindAsync(id);
    }
}
