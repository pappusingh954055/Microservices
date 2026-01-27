using Inventory.Application.Common.DTOs;
using Inventory.Application.Common.Interfaces;
using Inventory.Application.PurchaseOrders.DTOs;
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
                EF.Functions.Like(Convert.ToString(x.Id), $"%{cleanFilter}%") ||
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
                "id" => "Id",
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

    /// <summary>
    /// bind main grid polist
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<(IEnumerable<PurchaseOrder> Data, int Total)> GetDateRangePagedOrdersAsync(GetPurchaseOrdersRequest request)
    {
        // 1. Base Query with Nested Include + GRNHeaders Link
        // HUMNE YAHAN .Include(x => x.GrnHeaders) ADD KIYA HAI
        var query = _context.PurchaseOrders
            .Include(x => x.Items)
                .ThenInclude(i => i.Product)
            .Include(x => x.GrnHeaders) // <--- Sabse important update yahi hai
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
            var endOfToDate = request.ToDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(x => x.PoDate <= endOfToDate);
        }

        // 4. Column Specific Filters
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
                    "id" => query.Where(x => x.Id.ToString().Contains(val)),
                    _ => query
                };
            }
        }

        // 5. Total Count
        var total = await query.CountAsync();

        // 6. Dynamic Sorting & Pagination Execution
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

    /// <summary>
    /// /////////
    /// </summary>
    /// <param name="po"></param>
    /// <returns></returns>
    public Task UpdateAsync(PurchaseOrder po)
    {
        _context.PurchaseOrders.Attach(po);
        _context.Entry(po).Property(x => x.Status).IsModified = true;
        return Task.CompletedTask;
    }

    public async Task<bool> SaveChangesAsync()
    {
        // Agar 1 ya usse zyada rows affect hui hain toh true return karega
        return (await _context.SaveChangesAsync()) > 0;
    }
    public async Task<PurchaseOrder> GetByIdAsyncForUpdateStatus(int id)
    {
        // PO ke saath uske items ko bhi load karna behtar hota hai (Optional)
        return await _context.PurchaseOrders
                             .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<bool> UpdatePOStatusAsync(int id, string status)
    {
        var po = await _context.PurchaseOrders.FindAsync(id);
        if (po == null) return false;

        po.Status = status; // String value save hogi (e.g., "Submitted")
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<IEnumerable<PendingPODto>> GetPendingPurchaseOrdersAsync()
    {
        return await _context.PurchaseOrders
            // Condition ko "Approved" ke liye update karein
            .Where(po => po.Status == "Approved" || po.Status == "Pending" || po.Status == "Partial")
            .Select(po => new PendingPODto
            {
                Id = po.Id,
                PoNumber = po.PoNumber,
                SupplierName = po.SupplierName, // Table column ke mutabiq
                PoDate = po.PoDate,
                Status = po.Status
            })
            .OrderByDescending(po => po.Id)
            .ToListAsync();
    }
    public async Task<IEnumerable<POItemForGRNDto>> GetPOItemsForGRNAsync(int poId)
    {
        return await _context.PurchaseOrderItems
            .Where(poi => poi.PurchaseOrderId == poId)
            .Select(poi => new POItemForGRNDto
            {
                ProductId = poi.ProductId,
                ProductName = poi.Product.Name,
                OrderedQty = poi.Qty, // image_161d13.png mein 'Qty' hai
                UnitPrice = poi.Rate, // image_161d13.png mein 'Rate' hai

                // Yahan GRNDetail use karein kyunki aapki entity ka naam wahi hai [cite: 2026-01-28]
                AlreadyReceivedQty = _context.Set<GRNDetail>()
                    .Where(gi => gi.GRNHeader.PurchaseOrderId == poId && gi.ProductId == poi.ProductId)
                    .Sum(gi => (decimal?)gi.ReceivedQty) ?? 0
            }).ToListAsync();
    }
}