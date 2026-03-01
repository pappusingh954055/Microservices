using Customers.Application.Common.Interfaces;
using Customers.Application.DTOs;
using Customers.Application.Features.Finance.Queries;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Customers.Application.Features.Finance.Handlers
{
    public class GetReceiptsReportHandler(IFinanceRepository repository) : IRequestHandler<GetReceiptsReportQuery, PaginatedListDto<ReceiptReportDto>>
    {
        private readonly IFinanceRepository _repository = repository;

        public async Task<PaginatedListDto<ReceiptReportDto>> Handle(GetReceiptsReportQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetReceiptsReportAsync(request.Request);
        }
    }
}
