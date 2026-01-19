using Inventory.Application.Common.Interfaces;
using Inventory.Domain.PriceLists;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

internal sealed class PriceListRepository : IPriceListRepository
{
    private readonly InventoryDbContext _context;

    public PriceListRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(PriceList priceList)
    {
        await _context.PriceLists.AddAsync(priceList);
        await _context.SaveChangesAsync();
    }

    public Task UpdateAsync(PriceList priceList)
    {
        _context.PriceLists.Update(priceList);
        return Task.CompletedTask;
    }

    // ✅ THIS IS THE METHOD YOU ASKED FOR
    public Task DeleteAsync(PriceList priceList)
    {
        _context.PriceLists.Remove(priceList);
        return Task.CompletedTask;
    }

    public async Task<PriceList?> GetByIdAsync(Guid id)
    {
        return await _context.PriceLists
            .Include(x => x.PriceListItems)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<PriceList>> GetAllAsync()
    {
        return await _context.PriceLists
            .Include(x => x.PriceListItems)
            .ToListAsync();
    }
    public IQueryable<PriceList> Query()
    {
        return _context.PriceLists.AsQueryable();
    }
    public void DeleteRange(List<PriceList> PriceLists)
    {
        _context.PriceLists.RemoveRange(PriceLists);
    }

    public async Task<List<PriceList>> GetByIdsAsync(List<Guid> ids)
    {
        return await _context.PriceLists
            .Where(x => ids.Contains(x.Id))
            .ToListAsync();
    }

    public async Task<bool> HasPriceListAsync(List<Guid> pricelistIds)
    {
        return await _context.PriceLists
           .AnyAsync(x => pricelistIds.Contains(x.Id));
    }

    public async Task AddAsync(PriceList priceList, CancellationToken ct)
    {
        await _context.PriceLists.AddAsync(priceList, ct);
    }
    public async Task SaveChangesAsync(CancellationToken ct)
    {
        // Actual SQL 'INSERT' command yahan chalegi
        await _context.SaveChangesAsync(ct);
    }
}
