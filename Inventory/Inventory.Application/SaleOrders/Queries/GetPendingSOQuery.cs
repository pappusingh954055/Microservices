using Inventory.Application.SaleOrders.DTOs;
using MediatR;
using System.Collections.Generic;

namespace Inventory.Application.SaleOrders.Queries
{
    public record GetPendingSOQuery() : IRequest<IEnumerable<PendingSODto>>;
}
