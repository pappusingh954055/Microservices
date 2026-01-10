using Inventory.Application.Common.Interfaces;
using Inventory.Domain.PriceLists;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Repositories;

public sealed class PriceListRepository : IPriceListRepository
{
    private readonly InventoryDbContext _db;

    public PriceListRepository(InventoryDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(PriceList priceList)
    {
        _db.PriceLists.Add(priceList);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(PriceList priceList)
    {
        _db.PriceLists.Update(priceList);
        await _db.SaveChangesAsync();
    }

    public async Task<PriceList?> GetByIdAsync(Guid id)
    {
        return await _db.PriceLists
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<PriceList>> GetAllAsync()
    {
        return await _db.PriceLists
            .AsNoTracking()
            .ToListAsync();
    }

    public Task DeleteAsync(PriceList priceList)
    {
        throw new NotImplementedException();
    }
}
