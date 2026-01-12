using Inventory.Domain.Entities;

public interface ICategoryRepository
{
    Task AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(Category category);

    Task<Category?> GetByIdAsync(Guid id);
    Task<List<Category>> GetAllAsync();

    IQueryable<Category> Query();
}