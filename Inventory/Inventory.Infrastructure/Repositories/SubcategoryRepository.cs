using Microsoft.EntityFrameworkCore;
using Inventory.Domain.Entities;
using Inventory.Application.Common.Interfaces;
using Inventory.Infrastructure.Persistence;

namespace Inventory.Infrastructure.Repositories;

public sealed class SubcategoryRepository : ISubcategoryRepository
{
    private readonly InventoryDbContext _db;

    public SubcategoryRepository(InventoryDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Subcategory subcategory)
    {
        _db.Subcategories.Add(subcategory);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Subcategory subcategory)
    {
        _db.Subcategories.Update(subcategory);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Subcategory subcategory)
    {
        _db.Subcategories.Remove(subcategory);
        await _db.SaveChangesAsync();
    }

    public async Task<Subcategory?> GetByIdAsync(Guid id)
    {
        return await _db.Subcategories
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Subcategory>> GetAllAsync()
    {
        return await _db.Subcategories
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Subcategory>> GetByCategoryIdAsync(Guid categoryId)
    {
        return await _db.Subcategories
            .AsNoTracking()
            .Where(x => x.CategoryId == categoryId)
            .ToListAsync();
    }
}
