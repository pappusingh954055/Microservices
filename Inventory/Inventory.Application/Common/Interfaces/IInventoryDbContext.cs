using Inventory.Domain.PriceLists;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Common.Interfaces
{
    public interface IInventoryDbContext
    {
        DbSet<PriceList> PriceLists { get; }
        DbSet<PriceListItem> PriceListItems { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
