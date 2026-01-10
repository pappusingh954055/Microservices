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
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<PriceList>> GetAllAsync()
    {
        return await _context.PriceLists
            .Include(x => x.Items)
            .ToListAsync();
    }
}
