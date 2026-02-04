using Inventory.Application.Common.DTOs;
using Inventory.Application.Common.Interfaces;
using Inventory.Application.PurchaseOrders.DTOs;
using Inventory.Domain.Entities;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
        // STEP 1: Base Query - AsNoTracking use karein fast read ke liye [cite: 2026-02-04]
        // Include yahan se hata diya hai taaki CountAsync fast chale [cite: 2026-02-04]
        var query = _context.PurchaseOrders
            .AsNoTracking()
            .AsQueryable();

        // 1. GLOBAL SEARCH FIX
        if (!string.IsNullOrWhiteSpace(request.Filter))
        {
            var searchTerm = request.Filter.Trim().ToLower();
            query = query.Where(x =>
                (x.PoNumber != null && x.PoNumber.ToLower().Contains(searchTerm)) ||
                (x.SupplierName != null && x.SupplierName.ToLower().Contains(searchTerm)) ||
                (x.Status != null && x.Status.ToLower().Contains(searchTerm))
            );
        }

        // 2. DATE RANGE
        if (request.FromDate.HasValue) query = query.Where(x => x.PoDate >= request.FromDate.Value);
        if (request.ToDate.HasValue)
        {
            var endOfToDate = request.ToDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(x => x.PoDate <= endOfToDate);
        }

        // 3. COLUMN SPECIFIC FILTERS FIX
        if (request.Filters != null && request.Filters.Any())
        {
            foreach (var f in request.Filters)
            {
                var val = (f.Value ?? "").Trim().ToLower();
                var field = (f.Field ?? "").Trim().ToLower();

                if (string.IsNullOrEmpty(val)) continue;

                query = field switch
                {
                    "status" => query.Where(x => x.Status != null && x.Status.ToLower() == val),
                    "ponumber" or "po no." => query.Where(x => x.PoNumber != null && x.PoNumber.ToLower().Contains(val)),
                    "suppliername" => query.Where(x => x.SupplierName != null && x.SupplierName.ToLower().Contains(val)),
                    "id" => query.Where(x => x.Id.ToString().Contains(val)),
                    _ => query
                };
            }
        }

        // STEP 2: Get Total Count before adding heavy Includes [cite: 2026-02-04]
        var total = await query.CountAsync();

        // 4. DYNAMIC SORTING FIX
        bool isDesc = request.SortOrder?.ToLower() == "desc";
        string sortField = request.SortField?.ToLower().Trim();

        query = sortField switch
        {
            "status" => isDesc ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            "ponumber" => isDesc ? query.OrderByDescending(x => x.PoNumber) : query.OrderBy(x => x.PoNumber),
            "suppliername" => isDesc ? query.OrderByDescending(x => x.SupplierName) : query.OrderBy(x => x.SupplierName),
            _ => isDesc ? query.OrderByDescending(x => x.PoDate) : query.OrderBy(x => x.PoDate)
        };

        // STEP 3: Optimized Data Fetch [cite: 2026-02-04]
        // Ab sirf wahi 10-20 records ke liye Include chalega jo page par hain [cite: 2026-02-04]
        var data = await query
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize)
            .Include(x => x.Items)
                .ThenInclude(i => i.Product)
            .Include(x => x.GrnHeaders)
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
            // Condition: Status 'Approved' ho AUR uska koi GRN na bana ho
            .Where(po => po.Status == "Approved" && !_context.GRNHeaders.Any(grn => grn.PurchaseOrderId == po.Id))
            .Select(po => new PendingPODto
            {
                Id = po.Id,
                PoNumber = po.PoNumber,
                SupplierName = po.SupplierName,
                PoDate = po.PoDate,
                Status = po.Status
            })
            .OrderByDescending(po => po.Id)
            .ToListAsync();
    }
    public async Task<IEnumerable<POItemForGRNDto>> GetPOItemsForGRNAsync(int poId)
    {
        var poItems = await _context.PurchaseOrderItems
            .Where(poi => poi.PurchaseOrderId == poId)
            .Include(poi => poi.Product)
            .ToListAsync();

        var receivedQuantities = await _context.Set<GRNDetail>() // Entity name check karein
            .Where(gi => gi.GRNHeader.PurchaseOrderId == poId)
            .GroupBy(gi => gi.ProductId)
            .Select(g => new { ProductId = g.Key, Total = g.Sum(x => (decimal?)x.ReceivedQty) ?? 0 })
            .ToListAsync();

        // '.map' ki jagah '.Select' use karein
        return poItems.Select(poi => new POItemForGRNDto
        {
            ProductId = poi.ProductId,
            ProductName = poi.Product?.Name,
            OrderedQty = poi.Qty,
            UnitPrice = poi.Rate,
            AlreadyReceivedQty = receivedQuantities.FirstOrDefault(rq => rq.ProductId == poi.ProductId)?.Total ?? 0
        }).ToList();
    }

    public async Task<POHeaderDetailsDto?> GetPOHeaderAsync(int lastPurchaseOrderId)
    {
        return await _context.PurchaseOrders
        .Where(x => x.Id == lastPurchaseOrderId)
        
        .Select(x => new POHeaderDetailsDto
        {
            PurchaseOrderId = x.Id,
            ExpectedDeliveryDate=x.ExpectedDeliveryDate,
            SupplierId = x.SupplierId,      // int
            SupplierName = x.SupplierName,
            PriceListId = x.PriceListId, 
            
            Remarks = x.Remarks,
            PoNumber = x.PoNumber,
            PoDate = DateTime.Now           // Hamesha current date rakhein
        }).FirstOrDefaultAsync();
    }

    public async Task<ProductPriceDto?> GetPriceListRateAsync( Guid productId, Guid priceListId)
    {
        return await _context.PriceListItems
            .Where(pi => pi.PriceListId == priceListId && pi.ProductId == productId)
            .Select(pi => new ProductPriceDto
            {
                ProductId = pi.ProductId,
                Rate = pi.Rate, // From dbo.PriceListItems
                Unit = pi.Unit, // From dbo.PriceListItems
                                // From dbo.Products.DefaultGst
                GstPercent = pi.Product.DefaultGst??0
            })
            .FirstOrDefaultAsync();
    }
}