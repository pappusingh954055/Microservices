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

        //public async Task<bool> CreateSaleReturnAsync(SaleReturnHeader header)
        //{
        //    using var transaction = await _context.Database.BeginTransactionAsync();
        //    try
        //    {
        //        // 1. Sale Return entry save karo
        //        _context.SaleReturnHeaders.Add(header);

        //        // 2. Product Table mein CurrentStock update karo
        //        foreach (var item in header.ReturnItems)
        //        {
        //            // Yahan 'Id' column use hoga (schema dekho)
        //            var product = await _context.Products
        //                .FirstOrDefaultAsync(p => p.Id == item.ProductId);

        //            if (product != null)
        //            {
        //                // Sales Return = Stock increase (+)
        //                product.CurrentStock += item.ReturnQty;
        //                product.ModifiedOn = DateTime.Now; // Schema requirement
        //                product.ModifiedBy = item.ModifiedBy;
        //            }
        //        }

        //        await _context.SaveChangesAsync();
        //        await transaction.CommitAsync();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        return false;
        //    }
        //}

        //public async Task<bool> CreateSaleReturnAsync(SaleReturnHeader header)
        //{
        //    using var transaction = await _context.Database.BeginTransactionAsync();
        //    try
        //    {

        //        header.ReturnNumber = $"SR-{DateTime.Now:yyyyMMddHHmmss}";
        //        header.CreatedOn = DateTime.Now;
        //        header.Status = "CONFIRMED"; 

        //        decimal totalHeaderAmount = 0;


        //        foreach (var item in header.ReturnItems)
        //        {


        //            decimal baseAmt = item.ReturnQty * item.UnitPrice;
        //            item.TaxAmount = baseAmt * (item.TaxPercentage / 100m);


        //            totalHeaderAmount += item.TotalAmount;

        //            var product = await _context.Products
        //                .FirstOrDefaultAsync(p => p.Id == item.ProductId);

        //            if (product != null)
        //            {

        //                product.CurrentStock += item.ReturnQty;
        //                product.ModifiedOn = DateTime.Now; 
        //                product.ModifiedBy = header.ModifiedBy ?? "system";
        //            }

        //            item.CreatedOn = DateTime.Now;
        //        }

        //        header.TotalAmount = totalHeaderAmount;

        //        _context.SaleReturnHeaders.Add(header);

        //        await _context.SaveChangesAsync();
        //        await transaction.CommitAsync();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        return false;
        //    }
        //}

        //public async Task<bool> CreateSaleReturnAsync(SaleReturnHeader header)
        //{
        //    using var transaction = await _context.Database.BeginTransactionAsync();
        //    try
        //    {
        //        // 1. Save Sale Return
        //        _context.SaleReturnHeaders.Add(header);

        //        // 2. Stock Recovery (Restock logic)
        //        foreach (var item in header.ReturnItems)
        //        {
        //            var product = await _context.Products.FindAsync(item.ProductId);
        //            if (product != null)
        //            {
        //                // Return = Increase Current Stock
        //                product.CurrentStock += item.ReturnQty;
        //                product.ModifiedOn = DateTime.Now;
        //            }
        //        }

        //        await _context.SaveChangesAsync();
        //        await transaction.CommitAsync();
        //        return true;
        //    }
        //    catch (Exception)
        //    {
        //        await transaction.RollbackAsync();
        //        return false;
        //    }
        //}

        public async Task<bool> CreateSaleReturnAsync(SaleReturnHeader header)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Financial totals calculate karne ke liye variables
                decimal calculatedSubTotal = 0;
                decimal calculatedTaxAmount = 0;

                // 2. Stock Recovery aur Calculations Loop
                foreach (var item in header.ReturnItems)
                {
                    // Item level financial calculations
                    // UnitPrice (95.00) * ReturnQty (1) = 95.00
                    decimal itemSubTotal = item.ReturnQty * item.UnitPrice;
                    decimal itemTax = itemSubTotal * (item.TaxPercentage / 100m);

                    // Item level financial fields update
                    item.TaxAmount = itemTax;
                    item.TotalAmount = itemSubTotal + itemTax;
                    item.CreatedOn = DateTime.Now;

                    // Header totals accumulate karein
                    calculatedSubTotal += itemSubTotal;
                    calculatedTaxAmount += itemTax;

                    // STOCK LOGIC (No changes here, as requested)
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        // Sales Return = Increase Current Stock
                        product.CurrentStock += item.ReturnQty;
                        product.ModifiedOn = DateTime.Now;
                        product.ModifiedBy = header.CreatedBy ?? "system";
                    }
                }

                // 3. Header table columns update (0.00 fix karne ke liye)
                header.SubTotal = calculatedSubTotal;
                header.TaxAmount = calculatedTaxAmount;
                header.DiscountAmount = header.DiscountAmount;
                // TotalAmount final sync
                header.TotalAmount = calculatedSubTotal + calculatedTaxAmount - (header.DiscountAmount);
                header.CreatedOn = DateTime.Now;

                // 4. Save Sale Return
                _context.SaleReturnHeaders.Add(header);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<decimal> GetRemainingReturnableQtyAsync(int saleOrderId, Guid productId)
        {
            // 1. Sale Order mein kitni quantity bechi gayi thi (e.g., 6)
            var totalSold = await _context.SaleOrderItems
                .AsNoTracking()
                .Where(soi => soi.SaleOrderId == saleOrderId && soi.ProductId == productId)
                .Select(soi => soi.Qty)
                .FirstOrDefaultAsync();

            // 2. Iss Order ke liye ab tak kitna return ho chuka hai (e.g., 2)
            // Kewal "Confirmed" status wale returns ginein taaki logic dashboard se match kare
            var totalReturned = await _context.SaleReturnItems
                .AsNoTracking()
                .Where(sri => sri.SaleReturnHeader.SaleOrderId == saleOrderId &&
                              sri.ProductId == productId &&
                              sri.SaleReturnHeader.Status == "Confirmed")
                .SumAsync(sri => (decimal?)sri.ReturnQty) ?? 0;

            // 3. Calculation (6 - 2 = 4 pieces available for return)
            // Agar result 0 se niche jaye, toh 0 hi return karein taaki UI minus na dikhaye
            var remaining = totalSold - totalReturned;

            return remaining > 0 ? remaining : 0;
        }
    }
}