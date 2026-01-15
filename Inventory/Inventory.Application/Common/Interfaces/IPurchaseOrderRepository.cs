using Inventory.Domain.Entities;

namespace Inventory.Application.Common.Interfaces
{
    public interface IPurchaseOrderRepository
    {
        Task AddAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken);
    }
}
