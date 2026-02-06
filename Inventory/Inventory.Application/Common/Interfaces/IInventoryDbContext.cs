using Inventory.Domain.Entities;
using Inventory.Domain.PriceLists;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using YourProjectNamespace.Entities;

namespace Inventory.Application.Common.Interfaces
{
    public interface IInventoryDbContext
    {
        DbSet<GRNDetail> GRNDetails { get; }

        DbSet<SaleOrder> SaleOrders { get; }
        DbSet<SaleOrderItem> SaleOrderItems { get; }
        DbSet<PurchaseReturnItem> PurchaseReturnItems { get; }

        DbSet<SaleReturnHeader> SaleReturnHeaders { get; }
        DbSet<SaleReturnItem> SaleReturnItems { get; }
        DbSet<PriceList> PriceLists { get; }
        DbSet<PriceListItem> PriceListItems { get; }

        // PurchaseOrder Entities
        // Interface mein 'public' keyword hatayein
        DbSet<PurchaseOrder> PurchaseOrders { get; }
        DbSet<PurchaseOrderItem> PurchaseOrderItems { get; }

        // Is property se Handler ka error fix ho jayega
        DatabaseFacade Database { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
