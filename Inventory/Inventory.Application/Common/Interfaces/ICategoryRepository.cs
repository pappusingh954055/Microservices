using Inventory.Domain.Entities;

public interface ICategoryRepository
{
    Task AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(Category category);

    Task<List<Category>> GetAllAsync();

    Task<Category?> GetByIdAsync(Guid id);
    Task<List<Category>> GetByIdsAsync(List<Guid> ids);

    Task<bool> HasSubcategoriesAsync(Guid categoryId);
    Task<bool> HasSubcategoriesAsync(List<Guid> categoryIds);

    void Delete(Category category);
    void DeleteRange(List<Category> categories);

    IQueryable<Category> Query();
}