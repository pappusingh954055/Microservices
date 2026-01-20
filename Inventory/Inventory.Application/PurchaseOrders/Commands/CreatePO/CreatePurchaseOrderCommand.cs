using Application.DTOs;
using MediatR;

public record CreatePurchaseOrderCommand(
    int SupplierId,
    string PoNumber,
    DateTime PoDate,
    DateTime? ExpectedDeliveryDate,
    string ReferenceNumber,
    string Remarks,
    List<PurchaseOrderItemDto> Items
) : IRequest<Guid>;