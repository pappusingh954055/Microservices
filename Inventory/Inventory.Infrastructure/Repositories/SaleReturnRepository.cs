using Inventory.Domain.Entities;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Repositories
{
    public class SaleReturnRepository : ISaleReturnRepository
    {
        private readonly InventoryDbContext _context;

        public SaleReturnRepository(InventoryDbContext context)
        {
            _context = context;
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
            // Optimization 1: No tracking and initial query without includes for performance
            var query = _context.SaleReturnHeaders.AsNoTracking().AsQueryable();

            // 1. Filters (Direct Table Filters)
            if (fromDate.HasValue)
                query = query.Where(x => x.ReturnDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(x => x.ReturnDate <= toDate.Value);

            // 2. Search Logic (Including Join Table Search)
            if (!string.IsNullOrEmpty(search))
            {
                // EF Core is smart enough to handle this join only when needed
                query = query.Where(x => x.ReturnNumber.Contains(search) ||
                                       x.SaleOrder.SONumber.Contains(search));
            }

            // 3. Sorting [cite: 2026-02-05]
            bool isDesc = sortOrder?.ToLower() == "desc";
            query = sortField?.ToLower() switch
            {
                "returnnumber" => isDesc ? query.OrderByDescending(x => x.ReturnNumber) : query.OrderBy(x => x.ReturnNumber),
                "totalamount" => isDesc ? query.OrderByDescending(x => x.TotalAmount) : query.OrderBy(x => x.TotalAmount),
                "status" => isDesc ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
                _ => query.OrderByDescending(x => x.ReturnDate)
            };

            // Optimization 2: CountAsync before taking results
            var totalCount = await query.CountAsync();

            // Optimization 3: Projection (Select only needed fields)
            // Isse 'SELECT *' nahi hota, sirf wahi columns aate hain jo DTO mein hain.
            var items = await query
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .Select(x => new SaleReturnListDto
                {
                    SaleReturnHeaderId = x.SaleReturnHeaderId,
                    ReturnNumber = x.ReturnNumber,
                    ReturnDate = x.ReturnDate,
                    CustomerId = x.CustomerId, // Needed for Microservice mapping
                    SoRef = x.SaleOrder != null ? x.SaleOrder.SONumber : string.Empty, //
                    TotalAmount = x.TotalAmount,
                    Status = x.Status
                }).ToListAsync();

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