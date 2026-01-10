using Inventory.Domain.Entities;
using Inventory.Domain.PriceLists;

namespace Inventory.Application.Common.Interfaces;

public interface IPriceListRepository
{
    Task AddAsync(PriceList priceList);
    Task UpdateAsync(PriceList priceList);
    Task DeleteAsync(PriceList priceList);

    Task<PriceList?> GetByIdAsync(Guid id);
    Task<List<PriceList>> GetAllAsync();
}
