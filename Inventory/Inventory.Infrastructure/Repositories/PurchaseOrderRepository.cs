using Inventory.Application.Common.Interfaces;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Inventory.Domain.Entities; // Ensure this is present

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

    public async Task<string?> GetLastPoNumberAsync()
    {
        return await _context.PurchaseOrders
            .OrderByDescending(x => x.Id)
            .Select(x => x.PoNumber)
            .FirstOrDefaultAsync();
    }

    public async Task<string?> GetLatestPoNumberAsync()
    {
        return await _context.PurchaseOrders
            .OrderByDescending(x => x.Id)
            .Select(x => x.PoNumber)
            .FirstOrDefaultAsync();
    }

    public async Task<(IEnumerable<PurchaseOrder> Items, int TotalCount)> GetPagedOrdersAsync(
        int pageIndex, int pageSize, string? sortField, string? sortOrder, string? filter)
    {
        // 1. Base Query with Includes
        var query = _context.PurchaseOrders
            .Include(x => x.Items)
            .AsNoTracking() // Performance ke liye behtar hai read-only queries mein
            .AsQueryable();

        // 2. Filtering Logic
        if (!string.IsNullOrWhiteSpace(filter))
        {
            query = query.Where(x => x.PoNumber.Contains(filter) || x.Status.Contains(filter));
        }

        // 3. Sorting Logic (Fixing the "poNumber" vs "PoNumber" case issue)
        if (!string.IsNullOrEmpty(sortField))
        {
            // Angular camelCase bhejta hai, EF Core PascalCase mangta hai
            string mappedField = sortField.ToLower() switch
            {
                "ponumber" => "PoNumber",
                "podate" => "PoDate",
                "grandtotal" => "GrandTotal",
                "status" => "Status",
                "supplierid" => "SupplierId",
                _ => "PoDate" // Default sort column
            };

            query = (sortOrder?.ToLower() == "desc")
                ? query.OrderByDescending(x => EF.Property<object>(x, mappedField))
                : query.OrderBy(x => EF.Property<object>(x, mappedField));
        }
        else
        {
            query = query.OrderByDescending(x => x.PoDate); // Default ordering
        }

        // 4. Execution (Sequential to avoid DataReader errors)
        var totalCount = await query.CountAsync();

        var items = await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}