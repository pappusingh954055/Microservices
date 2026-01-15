using Inventory.Application.Common.Interfaces;
using Inventory.Application.Common.Models;
using Inventory.Domain.Entities;
using MediatR;

internal sealed class CreatePurchaseOrderCommandHandler
    : IRequestHandler<CreatePurchaseOrderCommand, Result<Guid>>
{
    private readonly IPurchaseOrderRepository _repository;
    private readonly IInventoryDbContext _context;

    public CreatePurchaseOrderCommandHandler(
        IPurchaseOrderRepository repository, IInventoryDbContext context)
    {
        _repository = repository;
        _context = context;
    }

    public async Task<Result<Guid>> Handle(
        CreatePurchaseOrderCommand request,
        CancellationToken cancellationToken)
    {
        if (request.SupplierId == Guid.Empty)
            return Result<Guid>.Failure("Supplier is required");

        if (request.Items.Count == 0)
            return Result<Guid>.Failure("At least one item is required");

        var poNumber = $"PO-{DateTime.UtcNow:yyyyMMddHHmmss}";

        var po = new PurchaseOrder(
            request.SupplierId,
            request.PoDate,
            poNumber
        );

        foreach (var item in request.Items)
        {
            po.AddItem(
                item.ProductId,
                item.Quantity,
                item.UnitPrice,
                item.DiscountPercent,
                item.GstPercent
            );
        }

        await _repository.AddAsync(po, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);  

        return Result<Guid>.Success(po.Id);
    }
}
