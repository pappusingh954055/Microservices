using Inventory.Application.PurchaseOrders.DTOs;
using MediatR;

public record GetPOHeaderDetailsQuery(int PurchaseOrderId) : IRequest<POHeaderDetailsDto>;