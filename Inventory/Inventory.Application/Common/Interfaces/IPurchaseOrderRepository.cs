using Inventory.Application.PurchaseOrders.DTOs;

namespace Inventory.Application.Common.Interfaces
{
    public interface IPurchaseOrderRepository
    {
        Task AddAsync(PurchaseOrder po, CancellationToken ct);
        Task<(IEnumerable<PurchaseOrder> Items, int TotalCount)> GetPagedOrdersAsync(
          int pageIndex,
          int pageSize,
          string sortField,
          string sortOrder,
          string filter);

        Task<(IEnumerable<PurchaseOrder> Data, int Total)> GetDateRangePagedOrdersAsync(GetPurchaseOrdersRequest request);

    }

    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken ct);
    }
}
