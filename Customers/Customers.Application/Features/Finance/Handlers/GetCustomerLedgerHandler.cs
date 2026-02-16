using MediatR;
using Customers.Application.Common.Interfaces;
using Customers.Application.DTOs;
using Customers.Application.Features.Finance.Queries;
using System.Threading;
using System.Threading.Tasks;

namespace Customers.Application.Features.Finance.Handlers
{
    public class GetCustomerLedgerHandler : IRequestHandler<GetCustomerLedgerQuery, CustomerLedgerResultDto>
    {
        private readonly IFinanceRepository _financeRepository;
        private readonly ICustomerRepository _customerRepository;

        public GetCustomerLedgerHandler(IFinanceRepository financeRepository, ICustomerRepository customerRepository)
        {
            _financeRepository = financeRepository;
            _customerRepository = customerRepository;
        }

        public async Task<CustomerLedgerResultDto> Handle(GetCustomerLedgerQuery request, CancellationToken cancellationToken)
        {
            var ledger = await _financeRepository.GetLedgerAsync(request.CustomerId);
            var customerName = await _customerRepository.GetCustomerNameByIdAsync(request.CustomerId);

            return new CustomerLedgerResultDto
            {
                CustomerName = customerName ?? "Unknown",
                Ledger = ledger
            };
        }
    }
}
