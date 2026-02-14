
using Suppliers.Application.DTOs;

public interface ISupplierRepository
{
    IQueryable<Supplier> Query();
    Task<Supplier?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<IEnumerable<Supplier>> GetAllAsync();
    Task AddAsync(Supplier supplier);
    Task UpdateAsync(Supplier supplier);
    Task SaveChangesAsync();
    Task<List<SupplierSelectDto>> GetSuppliersByIdsAsync(List<int> ids);
}

