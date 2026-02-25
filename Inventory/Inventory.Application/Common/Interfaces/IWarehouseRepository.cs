using Inventory.Domain.Entities;

namespace Inventory.Application.Common.Interfaces;

public interface IWarehouseRepository
{
    Task AddAsync(Warehouse warehouse);
    Task UpdateAsync(Warehouse warehouse);
    Task DeleteAsync(Warehouse warehouse);
    Task<List<Warehouse>> GetAllAsync();
    Task<Warehouse?> GetByIdAsync(Guid id);
}
