// poId integer hai lekin returns Guid based items
using Inventory.Application.PurchaseOrders.DTOs;
using MediatR;

public record GetPOItemsForGRNQuery(int PoId) : IRequest<IEnumerable<POItemForGRNDto>>;