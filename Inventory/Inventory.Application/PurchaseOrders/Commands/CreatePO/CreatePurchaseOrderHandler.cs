using Domain.Entities;
using Inventory.Application.Common.Interfaces;
using MediatR;

namespace Application.Features.PurchaseOrders.Handlers;

public class CreatePurchaseOrderHandler : IRequestHandler<CreatePurchaseOrderCommand, Guid>
{
    private readonly IPurchaseOrderRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePurchaseOrderHandler(IPurchaseOrderRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreatePurchaseOrderCommand request, CancellationToken ct)
    {
        // 1. Calculate Grand Total from DTO items
        decimal grandTotal = request.Items.Sum(x => x.total);

        // 2. Map Command to Domain Aggregate Root
        var purchaseOrder = new PurchaseOrder(
            request.PoNumber,
            request.SupplierId,
            request.PoDate,
            request.ExpectedDeliveryDate,
            request.ReferenceNumber,
            request.Remarks,
            grandTotal
        );

        // 3. Add Items to the Aggregate Root
        foreach (var item in request.Items)
        {
            purchaseOrder.AddItem(
                item.productId,
                item.qty,
                item.price,
                item.discountPercent,
                item.gstPercent,
                item.total
            );
        }

        // 4. Persist using Repository
        await _repository.AddAsync(purchaseOrder, ct);

        // 5. Commit Transaction (SaveChangesAsync)
        // Note: Ise bhulna mat, warna 400 error aayega!
        await _unitOfWork.SaveChangesAsync(ct);

        return purchaseOrder.Id;
    }
}