using Inventory.Application.Common.Interfaces;
using Inventory.Domain.Entities;
using Inventory.Domain.PriceLists;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly InventoryDbContext _db;

    public ProductRepository(InventoryDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Product product)
    {
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        _db.Products.Update(product);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Product product)
    {
        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _db.Products
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Product>> GetAllAsync()
    {
        return await _db.Products
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Product>> GetByCategoryIdAsync(Guid categoryId)
    {
        return await _db.Products
            .AsNoTracking()
            .Where(x => x.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task<List<Product>> GetBySubcategoryIdAsync(Guid subcategoryId)
    {
        return await _db.Products
            .AsNoTracking()
            .Where(x => x.SubcategoryId == subcategoryId)
            .ToListAsync();
    }

    public IQueryable<Product> Query()
    {
        return _db.Products.AsQueryable();
    }
    public void DeleteRange(List<Product> products)
    {
        _db.Products.RemoveRange(products);
    }

    public async Task<List<Product>> GetByIdsAsync(List<Guid> ids)
    {
        return await _db.Products
            .Where(x => ids.Contains(x.Id))
            .ToListAsync();
    }

    public async Task<bool> HasPriceListAsync(List<Guid> ProductsIds)
    {
        return await _db.Products
           .AnyAsync(x => ProductsIds.Contains(x.Id));
    }

    public async Task<List<Product>> SearchActiveProductsAsync(string term)
    {
        return await _db.Products
         .AsNoTracking() // Read-only query optimization
         .Where(p => p.IsActive && p.Name.Contains(term)) // Sirf Name par search lagayein
         .Take(20)
         .ToListAsync();
    }
}
