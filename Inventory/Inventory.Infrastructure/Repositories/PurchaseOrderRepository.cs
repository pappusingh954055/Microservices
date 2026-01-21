using Inventory.Application.Common.Interfaces;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public sealed class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly InventoryDbContext _context;

    public PurchaseOrderRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(PurchaseOrder po, CancellationToken ct)
    {
        await _context.PurchaseOrders.AddAsync(po, ct);
    }

    public async Task<string> GetLastPoNumberAsync()
    {
        // Database se sabse latest PO number uthayein
        return await _context.PurchaseOrders
            .OrderByDescending(x => x.Id)
            .Select(x => x.PoNumber)
            .FirstOrDefaultAsync();
    }


    public async Task<string> GetLatestPoNumberAsync()
    {
        // PO/26-27/0001 format logic ke liye last record uthayenge
        return await _context.PurchaseOrders
            .OrderByDescending(x => x.Id)
            .Select(x => x.PoNumber)
            .FirstOrDefaultAsync();
    }
}
