using Inventory.Application.Common.Interfaces;
using Inventory.Application.PurchaseOrders.DTOs;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Inventory.Application.Common.DTOs;

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
        // 1. Base Query with Eager Loading for Items and Products
        var query = _context.PurchaseOrders
            .Include(x => x.Items)
                .ThenInclude(i => i.Product) // Product data load karega taaki null error na aaye
            .AsSplitQuery()
            .AsNoTracking()
            .AsQueryable();

        // 2. Filtering Logic (Multi-column search)
        if (!string.IsNullOrWhiteSpace(filter))
        {
            var cleanFilter = filter.Trim();

            query = query.Where(x =>
                EF.Functions.Like(x.PoNumber, $"%{cleanFilter}%") ||
                EF.Functions.Like(x.Status, $"%{cleanFilter}%") ||
                EF.Functions.Like(x.SupplierName, $"%{cleanFilter}%") // Direct DB column search
            );
        }

        // 3. Sorting Logic
        if (!string.IsNullOrEmpty(sortField))
        {
            string mappedField = sortField.ToLower() switch
            {
                "ponumber" => "PoNumber",
                "podate" => "PoDate",
                "grandtotal" => "GrandTotal",
                "status" => "Status",
                "suppliername" => "SupplierName",
                _ => "PoDate"
            };

            query = (sortOrder?.ToLower() == "desc")
                ? query.OrderByDescending(x => EF.Property<object>(x, mappedField))
                : query.OrderBy(x => EF.Property<object>(x, mappedField));
        }
        else
        {
            query = query.OrderByDescending(x => x.PoDate);
        }

        // 4. Execution
        var totalCount = await query.CountAsync();

        var items = await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(IEnumerable<PurchaseOrder> Data, int Total)> GetDateRangePagedOrdersAsync(GetPurchaseOrdersRequest request)
    {
        // 1. Base Query with Nested Include
        var query = _context.PurchaseOrders
            .Include(x => x.Items)
                .ThenInclude(i => i.Product)
            .AsQueryable();

        // 2. GLOBAL SEARCH FIX: Search PO No, Supplier Name, or Status
        if (!string.IsNullOrWhiteSpace(request.Filter))
        {
            var searchTerm = request.Filter.Trim().ToLower();
            query = query.Where(x =>
                x.PoNumber.ToLower().Contains(searchTerm) ||
                x.SupplierName.ToLower().Contains(searchTerm) ||
                x.Status.ToLower().Contains(searchTerm)
            );
        }

        // 3. Global Date Range Filter
        if (request.FromDate.HasValue)
        {
            query = query.Where(x => x.PoDate >= request.FromDate.Value);
        }
        if (request.ToDate.HasValue)
        {
            // Pure din ka data lene ke liye end of day logic
            var endOfToDate = request.ToDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(x => x.PoDate <= endOfToDate);
        }

        // 4. Column Specific Filters (Advanced Grid Filters)
        if (request.Filters != null && request.Filters.Any())
        {
            foreach (var f in request.Filters)
            {
                if (string.IsNullOrEmpty(f.Value)) continue;
                var val = f.Value.ToLower();

                query = f.Field.ToLower() switch
                {
                    "ponumber" => query.Where(x => x.PoNumber.ToLower().Contains(val)),
                    "suppliername" => query.Where(x => x.SupplierName.ToLower().Contains(val)),
                    "status" => query.Where(x => x.Status.ToLower().Contains(val)),
                    _ => query
                };
            }
        }

        // 5. Total Count (Filtering ke baad count lena zaroori hai)
        var total = await query.CountAsync();

        // 6. Dynamic Sorting & Pagination Execution
        // Note: OrderBy hamesha Pagination se pehle karein
        var data = await query
            .OrderByDescending(x => x.PoDate)
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return (data, total);
    }
    public async Task<PurchaseOrder?> GetByIdWithItemsAsync(int id, CancellationToken ct)
    {
      var data= await _context.PurchaseOrders
            .Include(x => x.Items) // Yeh child table 'PurchaseOrderItems' se data layega
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return data;
    }

    public void Update(PurchaseOrder po) => _context.PurchaseOrders.Update(po);

    public void RemoveItem(PurchaseOrderItem item) => _context.PurchaseOrderItems.Remove(item);

    public async Task<bool> DeleteItemAsync(int itemId)
    {
        var item = await _context.PurchaseOrderItems.FindAsync(itemId);
        if (item == null) return false;

        _context.PurchaseOrderItems.Remove(item);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task UpdatePOTotalsAsync(int poId)
    {
        var items = await _context.PurchaseOrderItems
                                  .Where(x => x.PurchaseOrderId == poId)
                                  .ToListAsync();

        var po = await _context.PurchaseOrders.FindAsync(poId);

        if (po != null)
        {
            // 1. SubTotal hamesha (Total - TaxAmount) hona chahiye agar aapke DB mein Total inclusive hai
            // Ya phir agar aapke paas TaxableAmount ka alag column hai toh wo use karein.
            po.SubTotal = items.Sum(x => x.Total - x.TaxAmount);

            // 2. TotalTax bilkul sahi hai
            po.TotalTax = items.Sum(x => x.TaxAmount);

            // 3. GrandTotal ab accurate aayega
            po.GrandTotal = po.SubTotal + po.TotalTax;

            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> BulkDeleteItemsAsync(List<int> itemIds)
    {
        var items = await _context.PurchaseOrderItems
                                  .Where(x => itemIds.Contains(x.Id))
                                  .ToListAsync();

        if (!items.Any()) return false;

        _context.PurchaseOrderItems.RemoveRange(items);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<PurchaseOrder> GetByIdAsync(int id)
    {
        // .Include() tab use karein agar delete se pehle Items ka status check karna ho
        return await _context.PurchaseOrders
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public void Delete(PurchaseOrder po)
    {
        _context.PurchaseOrders.Remove(po);
    }

    public async Task<List<PurchaseOrder>> GetByIdsAsync(List<int> ids)
    {
        return await _context.PurchaseOrders
            .Where(x => ids.Contains(x.Id))
            .ToListAsync();
    }
}