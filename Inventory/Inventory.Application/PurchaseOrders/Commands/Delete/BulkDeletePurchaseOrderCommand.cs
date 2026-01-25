using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.PurchaseOrders.Commands.Delete
{
    public record BulkDeletePurchaseOrderCommand(List<int> Ids) : IRequest<bool>;
}
