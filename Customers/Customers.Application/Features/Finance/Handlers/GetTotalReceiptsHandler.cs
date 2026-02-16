using MediatR;
using Customers.Application.Common.Interfaces;
using Customers.Application.Features.Finance.Queries;
using System.Threading;
using System.Threading.Tasks;

namespace Customers.Application.Features.Finance.Handlers
{
    public class GetTotalReceiptsHandler : IRequestHandler<GetTotalReceiptsQuery, decimal>
    {
        private readonly IFinanceRepository _repository;

        public GetTotalReceiptsHandler(IFinanceRepository repository)
        {
            _repository = repository;
        }

        public async Task<decimal> Handle(GetTotalReceiptsQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetTotalReceiptsAsync(request.DateRange);
        }
    }
}
