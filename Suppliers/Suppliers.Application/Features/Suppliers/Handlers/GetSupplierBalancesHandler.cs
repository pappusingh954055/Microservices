using MediatR;
using Suppliers.Application.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Suppliers.Application.Features.Suppliers.Handlers
{
    public class GetSupplierBalancesHandler : IRequestHandler<Queries.GetSupplierBalancesQuery, Dictionary<int, decimal>>
    {
        private readonly IFinanceRepository _repository;

        public GetSupplierBalancesHandler(IFinanceRepository repository)
        {
            _repository = repository;
        }

        public async Task<Dictionary<int, decimal>> Handle(Queries.GetSupplierBalancesQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetSupplierBalancesAsync(request.SupplierIds);
        }
    }
}
