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

    }

    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken ct);
    }
}
