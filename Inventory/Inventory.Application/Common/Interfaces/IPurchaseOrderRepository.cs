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
        Task<PurchaseOrder?> GetByIdWithItemsAsync(int id, CancellationToken ct);
        void Update(PurchaseOrder po);
        void RemoveItem(PurchaseOrderItem item);
        Task<bool> DeleteItemAsync(int itemId);
        public Task<bool> BulkDeleteItemsAsync(List<int> itemIds);
        Task UpdatePOTotalsAsync(int poId);
        Task<PurchaseOrder> GetByIdAsync(int id);
        void Delete(PurchaseOrder po);
        Task<List<PurchaseOrder>> GetByIdsAsync(List<int> ids);

    }

    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken ct);
    }
}
