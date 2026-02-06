using Inventory.Application.Clients;
using Inventory.Domain.Entities;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Repositories
{
    public class SaleReturnRepository : ISaleReturnRepository
    {
        private readonly InventoryDbContext _context;
        private readonly ICustomerClient _customerClient;   

        public SaleReturnRepository(InventoryDbContext context, ICustomerClient customerClient
            )
        {
            _context = context;
            _customerClient = customerClient;
        }

        public async Task<SaleReturnPagedResponse> GetSaleReturnsAsync(
     string? search,
     int pageIndex,
     int pageSize,
     DateTime? fromDate,
     DateTime? toDate,
     string sortField,
     string sortOrder)
        {
            var query = _context.SaleReturnHeaders.AsNoTracking().AsQueryable();

            // 1. Filters
            if (fromDate.HasValue)
                query = query.Where(x => x.ReturnDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(x => x.ReturnDate <= toDate.Value);

            // 2. Search Logic
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => x.ReturnNumber.Contains(search) ||
                                        x.SaleOrder.SONumber.Contains(search));
            }

            // 3. Sorting
            bool isDesc = sortOrder?.ToLower() == "desc";
            query = sortField?.ToLower() switch
            {
                "returnnumber" => isDesc ? query.OrderByDescending(x => x.ReturnNumber) : query.OrderBy(x => x.ReturnNumber),
                "totalamount" => isDesc ? query.OrderByDescending(x => x.TotalAmount) : query.OrderBy(x => x.TotalAmount),
                "status" => isDesc ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
                _ => query.OrderByDescending(x => x.ReturnDate)
            };

            var totalCount = await query.CountAsync();

            // 4. Projection
            var items = await query
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .Select(x => new SaleReturnListDto
                {
                    SaleReturnHeaderId = x.SaleReturnHeaderId,
                    ReturnNumber = x.ReturnNumber,
                    ReturnDate = x.ReturnDate,
                    CustomerId = x.CustomerId,
                    SoRef = x.SaleOrder != null ? x.SaleOrder.SONumber : string.Empty,
                    TotalAmount = x.TotalAmount,
                    Status = x.Status
                }).ToListAsync();

            // ==========================================================
            // STEP 5: MICROSERVICE MAPPING LOGIC (The Fix)
            // ==========================================================
            // 1. Saare Unique CustomerIds nikaalein
            var customerIds = items.Select(i => i.CustomerId).Distinct().ToList();

            // 2. CustomerMicroservice se Names fetch karein [cite: 2026-02-06]
            // Note: Yahan aapka existing internal service call aayega
            var customerMap = await _customerClient.GetCustomerNamesAsync(customerIds);

            // 3. Names ko list mein bind karein
            foreach (var item in items)
            {
                if (customerMap.ContainsKey(item.CustomerId))
                {
                    item.CustomerName = customerMap[item.CustomerId];
                }
                else
                {
                    item.CustomerName = "Unknown Customer"; // Fallback
                }
            }
            // ==========================================================

            return new SaleReturnPagedResponse { Items = items, TotalCount = totalCount };
        }

        public async Task<bool> CreateSaleReturnAsync(SaleReturnHeader header)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Sale Return entry save karo
                _context.SaleReturnHeaders.Add(header);

                // 2. Product Table mein CurrentStock update karo
                foreach (var item in header.ReturnItems)
                {
                    // Yahan 'Id' column use hoga (schema dekho)
                    var product = await _context.Products
                        .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                    if (product != null)
                    {
                        // Sales Return = Stock increase (+)
                        product.CurrentStock += item.ReturnQty;
                        product.ModifiedOn = DateTime.Now; // Schema requirement
                        product.ModifiedBy = item.ModifiedBy;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}