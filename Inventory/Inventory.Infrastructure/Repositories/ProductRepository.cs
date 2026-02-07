using DocumentFormat.OpenXml.InkML;
using Inventory.Application.Common.Interfaces;
using Inventory.Application.Products.DTOs;
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
    public async Task<ProductRateDto> GetProductRateAsync(Guid productId, Guid? priceListId)
    {
        // Products table se shuru karein taaki humein base details mil sakein
        var query = from p in _db.Products.AsNoTracking()
                    where p.Id == productId
                    select new ProductRateDto(
                        p.Id,
                        priceListId,
                        // Subquery to get PriceList Rate (agar priceListId null nahi hai toh)
                        _db.PriceListItems
                            .Where(pli => pli.ProductId == productId && pli.PriceListId == priceListId)
                            .Select(pli => pli.Rate)
                            .FirstOrDefault(), // Agar nahi mila toh 0.0m aayega
                        p.BasePurchasePrice, // Product Master wala base price
                        p.Unit ?? "PCS",     // Fallback Unit
                        p.DefaultGst ?? 0m,        // Default GST %
                        p.HSNCode            // HSN Code
                    );

        var result = await query.FirstOrDefaultAsync();

        if (result == null)
        {
            throw new Exception("Product not found in Master.");
        }

        return result;
    }

    public async Task<IEnumerable<LowStockProductDto>> GetLowStockProductsAsync()
    {
        return await _db.Products
            .AsNoTracking()
            .Where(p => p.IsActive && p.CurrentStock <= p.MinStock) // Dashboard wala logic
            .Select(p => new LowStockProductDto
            {
                Id = p.Id,
                CategoryName = p.Category.CategoryName, // Join logic
                SubCategoryName = p.Subcategory.SubcategoryName,
                ProductName = p.Name,
                SKU = p.Sku,
                Unit = p.Unit,
                CurrentStock = p.CurrentStock,
                MinStock = p.MinStock,
                BasePurchasePrice = p.BasePurchasePrice
            })
            .ToListAsync();
    }

    public async Task<List<ExcelExportDto>> GetLowStockExportDataAsync()
    {
        return await _db.Products
            .AsNoTracking()
            .Where(p => p.IsActive && p.CurrentStock <= p.MinStock)
            .Select(p => new ExcelExportDto
            {
                ProductName = p.Name,
                SKU = p.Sku,
                Category = p.Category.CategoryName,
                CurrentStock = p.CurrentStock,
                MinStock = p.MinStock,
                Unit = p.Unit
            })
            .ToListAsync();
    }
}
