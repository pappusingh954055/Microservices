using MediatR;
using Customers.Application.DTOs;
using System.Collections.Generic;

namespace Customers.Application.Features.Finance.Queries
{
    public record GetCustomerLedgerQuery(CustomerLedgerRequestDto Request) : IRequest<CustomerLedgerPagedResultDto>;
    public record GetOutstandingQuery(OutstandingRequestDto Request) : IRequest<OutstandingPagedResultDto>;
    public record GetTotalReceiptsQuery(DateRangeDto DateRange) : IRequest<decimal>;
    public record GetTotalOutstandingQuery() : IRequest<decimal>;
}
