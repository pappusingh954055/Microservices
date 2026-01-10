using Inventory.Application.Common.Interfaces;
using Inventory.Domain.Entities;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

internal sealed class SubcategoryRepository : ISubcategoryRepository
{
    private readonly InventoryDbContext _context;

    public SubcategoryRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Subcategory subcategory)
    {
        await _context.Subcategories.AddAsync(subcategory);
    }

    public Task UpdateAsync(Subcategory subcategory)
    {
        _context.Subcategories.Update(subcategory);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Subcategory subcategory)
    {
        _context.Subcategories.Remove(subcategory);
        return Task.CompletedTask;
    }

    public async Task<Subcategory?> GetByIdAsync(Guid id)
    {
        return await _context.Subcategories
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Subcategory>> GetAllAsync()
    {
        return await _context.Subcategories.ToListAsync();
    }
}
