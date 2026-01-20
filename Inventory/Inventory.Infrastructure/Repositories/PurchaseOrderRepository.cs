using Domain.Entities;
using Inventory.Application.Common.Interfaces;
using Inventory.Domain.Entities;
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
        // EF Core automatically handles adding child Items because they are part of the Aggregate
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
}
