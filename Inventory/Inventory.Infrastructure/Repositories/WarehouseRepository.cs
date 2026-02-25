using Inventory.Application.Common.Interfaces;
using Inventory.Domain.Entities;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Repositories;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly InventoryDbContext _context;

    public WarehouseRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Warehouse warehouse)
    {
        await _context.Warehouses.AddAsync(warehouse);
    }

    public Task UpdateAsync(Warehouse warehouse)
    {
        _context.Warehouses.Update(warehouse);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Warehouse warehouse)
    {
        _context.Warehouses.Remove(warehouse);
        return Task.CompletedTask;
    }

    public async Task<List<Warehouse>> GetAllAsync()
    {
        return await _context.Warehouses.AsNoTracking().ToListAsync();
    }

    public async Task<Warehouse?> GetByIdAsync(Guid id)
    {
        return await _context.Warehouses.FindAsync(id);
    }
}
