using Inventory.Domain.Entities;

namespace Inventory.Application.Common.Interfaces;

public interface IProductRepository
{
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Product product);

    Task<Product?> GetByIdAsync(Guid id);
    Task<List<Product>> GetAllAsync();

    Task<List<Product>> GetByCategoryIdAsync(Guid categoryId);
    Task<List<Product>> GetBySubcategoryIdAsync(Guid subcategoryId);
}
