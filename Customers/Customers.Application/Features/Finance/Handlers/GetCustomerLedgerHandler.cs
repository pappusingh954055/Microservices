using MediatR;
using Customers.Application.Common.Interfaces;
using Customers.Application.DTOs;
using Customers.Application.Features.Finance.Queries;
using System.Threading;
using System.Threading.Tasks;

namespace Customers.Application.Features.Finance.Handlers
{
    public class GetCustomerLedgerHandler : IRequestHandler<GetCustomerLedgerQuery, CustomerLedgerPagedResultDto>
    {
        private readonly IFinanceRepository _financeRepository;

        public GetCustomerLedgerHandler(IFinanceRepository financeRepository)
        {
            _financeRepository = financeRepository;
        }

        public async Task<CustomerLedgerPagedResultDto> Handle(GetCustomerLedgerQuery request, CancellationToken cancellationToken)
        {
            return await _financeRepository.GetLedgerAsync(request.Request);
        }
    }
}
