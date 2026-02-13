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
        int successCount = 0;

        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);
            using (var workbook = new XLWorkbook(stream))
            {
                var worksheet = workbook.Worksheet(1); // First sheet
                var rows = worksheet.RangeUsed().RowsUsed();

                // 1. Header Validation
                var headerRow = rows.FirstOrDefault();
                if (headerRow == null)
                {
                    errors.Add("Invalid Template: File is empty.");
                    return (0, errors);
                }

                var expectedHeaders = new List<string> { "CategoryCode", "CategoryName", "DefaultGst", "Description" };
                var actualHeaders = new List<string>();

                for (int i = 1; i <= 4; i++)
                {
                     actualHeaders.Add(headerRow.Cell(i).GetValue<string>().Trim());
                }

                if (!expectedHeaders.SequenceEqual(actualHeaders))
                {
                    errors.Add($"Invalid Template: Headers do not match. Expected: {string.Join(", ", expectedHeaders)}");
                     return (0, errors);
                }

                var dataRows = rows.Skip(1);

                // 2. Pre-fetch Categories for duplicate check (All records for Code, Active only for Name)
                var existingCategories = await _db.Categories
                    .AsNoTracking()
                    .Select(c => new { c.CategoryCode, c.CategoryName, c.IsActive })
                    .ToListAsync();
                
                var allCodeSet = new HashSet<string>(existingCategories.Select(x => x.CategoryCode.ToLower().Trim()));
                var activeNameSet = new HashSet<string>(existingCategories.Where(x => x.IsActive).Select(x => x.CategoryName.ToLower().Trim()));

                var newCategories = new List<Category>();

                // 3. In-File Duplicate Check
                var fileCodes = new HashSet<string>();
                var fileNames = new HashSet<string>();

                foreach (var row in dataRows)
                {
                     int rowNum = row.RowNumber();
                     try 
                     {
                        var code = row.Cell(1).Value.ToString()?.Trim();
                        var name = row.Cell(2).Value.ToString()?.Trim();
                        var gstValue = row.Cell(3).Value;
                        var description = row.Cell(4).Value.ToString()?.Trim();

                        // Skip Empty Rows (Strict)
                        if (string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(name))
                        {
                            continue;
                        }

                        // Validation
                        if (string.IsNullOrWhiteSpace(name))
                        {
                            errors.Add($"Row {rowNum}: Category Name is required.");
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(code))
                        {
                            errors.Add($"Row {rowNum}: Category Code is required.");
                            continue;
                        }

                        // Duplicate Check (In-File)
                        if (fileCodes.Contains(code.ToLower()))
                        {
                            errors.Add($"Row {rowNum}: Duplicate Code '{code}' in file.");
                            continue;
                        }
                        if (fileNames.Contains(name.ToLower()))
                        {
                             errors.Add($"Row {rowNum}: Duplicate Name '{name}' in file.");
                             continue;
                        }

                        // Duplicate Check (DB Level) - All for Code, Active for Name
                        if (allCodeSet.Contains(code.ToLower()))
                        {
                            errors.Add($"Row {rowNum}: Code '{code}' already exists in database.");
                            continue;
                        }
                        if (activeNameSet.Contains(name.ToLower()))
                        {
                            errors.Add($"Row {rowNum}: Active Category with Name '{name}' already exists.");
                            continue;
                        }

                        fileCodes.Add(code.ToLower());
                        fileNames.Add(name.ToLower());

                        // GST Parsing
                        decimal defaultGst = 0;
                        if (!gstValue.IsBlank)
                        {
                             if (!decimal.TryParse(gstValue.ToString(), out defaultGst))
                             {
                                 errors.Add($"Row {rowNum}: Invalid GST '{gstValue}'.");
                                 continue;
                             }
                        }

                        // Create Category
                        var category = new Category(
                            name,
                            code,
                            defaultGst,
                            description,
                            true, // IsActive
                            null // ParentCategoryId
                        );
                        
                        newCategories.Add(category);
                        successCount++;
                     }
                     catch(Exception ex)
                     {
                         errors.Add($"Row {rowNum}: Fatal error - {ex.Message}");
                     }
                }

                if (!newCategories.Any() && !errors.Any())
                {
                    errors.Add("No valid data rows found in the file.");
                }

                if (newCategories.Any())
                {
                    await _db.Categories.AddRangeAsync(newCategories);
                    await _db.SaveChangesAsync();
                }
            }
        }
        return (successCount, errors);
    }
}
