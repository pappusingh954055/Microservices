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

    public async Task AddAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken)
    {
        await _context.PurchaseOrders.AddAsync(purchaseOrder);
        await _context.SaveChangesAsync();
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
