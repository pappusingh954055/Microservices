using Inventory.Application.Common.Models;
using Inventory.Application.PurchaseOrders.Commands.Create;
using MediatR;

public sealed class CreatePurchaseOrderCommand : IRequest<Result<Guid>>
{
    public Guid SupplierId { get; init; }
    public DateTime PoDate { get; init; }
    public List<CreatePurchaseOrderItemDto> Items { get; init; } = [];
}
