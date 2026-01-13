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
    IQueryable<PriceList> Query();
    void DeleteRange(List<PriceList> subcategories);
    Task<List<PriceList>> GetByIdsAsync(List<Guid> ids);
    Task<bool> HasPriceListAsync(List<Guid> pricelistIds);
}
