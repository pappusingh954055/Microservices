using MediatR;
using Customers.Application.Common.Interfaces;
using Customers.Application.DTOs;
using Customers.Application.Features.Finance.Queries;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Customers.Application.Features.Finance.Handlers
{
    public class GetOutstandingHandler : IRequestHandler<GetOutstandingQuery, OutstandingPagedResultDto>
    {
        private readonly IFinanceRepository _repository;

        public GetOutstandingHandler(IFinanceRepository repository)
        {
            _repository = repository;
        }

        public async Task<OutstandingPagedResultDto> Handle(GetOutstandingQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetOutstandingAsync(request.Request);
        }
    }
}
