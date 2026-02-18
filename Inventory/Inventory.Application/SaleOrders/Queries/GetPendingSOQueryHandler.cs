using Inventory.Application.Common.Interfaces;
using Inventory.Application.SaleOrders.DTOs;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Inventory.Application.SaleOrders.Queries
{
    public class GetPendingSOQueryHandler : IRequestHandler<GetPendingSOQuery, IEnumerable<PendingSODto>>
    {
        private readonly ISaleOrderRepository _repository;

        public GetPendingSOQueryHandler(ISaleOrderRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<PendingSODto>> Handle(GetPendingSOQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetPendingSaleOrdersAsync();
        }
    }
}
