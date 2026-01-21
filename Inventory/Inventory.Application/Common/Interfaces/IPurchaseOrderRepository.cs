namespace Inventory.Application.Common.Interfaces
{
    public interface IPurchaseOrderRepository
    {
        Task AddAsync(PurchaseOrder po, CancellationToken ct);


    }

    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken ct);
    }

}
