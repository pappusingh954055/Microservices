using MediatR;
using Customers.Application.DTOs;
using System.Collections.Generic;

namespace Customers.Application.Features.Finance.Queries
{
    public record GetCustomerLedgerQuery(int CustomerId) : IRequest<CustomerLedgerResultDto>;
    public record GetOutstandingQuery() : IRequest<List<OutstandingDto>>;
    public record GetTotalReceiptsQuery(DateRangeDto DateRange) : IRequest<decimal>;
}
