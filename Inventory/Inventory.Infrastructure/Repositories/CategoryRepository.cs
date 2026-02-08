using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using Inventory.Domain.Entities;
using Inventory.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly InventoryDbContext _db;

    public CategoryRepository(InventoryDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Category category)
    {
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Category category)
    {
        _db.Categories.Update(category);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Category category)
    {
        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
    }

    public async Task<Category?> GetByIdAsync(Guid id)
    {
        return await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Category>> GetAllAsync()
    {
        return await _db.Categories
            .AsNoTracking()
            .ToListAsync();
    }
    

    public async Task<List<Category>> GetByIdsAsync(List<Guid> ids)
    {
        return await _db.Categories
            .Where(x => ids.Contains(x.Id))
            .ToListAsync();
    }

    public void DeleteRange(List<Category> categories)
    {
        _db.Categories.RemoveRange(categories);
    }

    // ✅ DEPENDENCY CHECK (NO NAVIGATION PROPERTY)
    public async Task<bool> HasSubcategoriesAsync(Guid categoryId)
    {
        return await _db.Subcategories
            .AnyAsync(x => x.CategoryId == categoryId);
    }

    public async Task<bool> HasSubcategoriesAsync(List<Guid> categoryIds)
    {
        return await _db.Subcategories
            .AnyAsync(x => categoryIds.Contains(x.CategoryId));
    }

    public void Delete(Category category)
    {
        _db.Categories.Remove(category);
    }
    public IQueryable<Category> Query()
    {
        return _db.Categories.AsQueryable();
    }

    public async Task<(int successCount, List<string> errors)> UploadCategoriesAsync(IFormFile file)
    {
        var errors = new List<string>();
        var categoriesToAdd = new List<Category>();
        int successCount = 0;

        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);
            using (var workbook = new XLWorkbook(stream))
            {
                var worksheet = workbook.Worksheet(1); // First sheet
                var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Skip Header

                foreach (var row in rows)
                {
                    var code = row.Cell(1).GetValue<string>();
                    var name = row.Cell(2).GetValue<string>();

                    // --- DUPLICATE CHECK ---
                    bool exists = await _db.Categories.AnyAsync(c => c.CategoryCode == code || c.CategoryName == name);

                    if (exists)
                    {
                        errors.Add($"Row {row.RowNumber()}: Category '{name}' or Code '{code}' already exists.");
                        continue;
                    }

                    categoriesToAdd.Add(new Category
                    {
                        CategoryCode = code,
                        CategoryName = name,
                        DefaultGst = row.Cell(3).GetValue<decimal>(),
                        Description = row.Cell(4).GetValue<string>(),
                        IsActive = true
                    });
                }
            }
        }

        if (categoriesToAdd.Any())
        {
            await _db.Categories.AddRangeAsync(categoriesToAdd);
            successCount = await _db.SaveChangesAsync();
        }

        return (successCount, errors);
    }
}
