using MediatR;
using Suppliers.Application.DTOs;
using Suppliers.Application.Features.Suppliers.Queries;
using Suppliers.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Suppliers.Application.Features.Suppliers.Handlers
{
    public class GetSupplierLedgerHandler(IFinanceRepository financeRepository, ISupplierRepository supplierRepository) : IRequestHandler<GetSupplierLedgerQuery, SupplierLedgerResultDto>
    {
        private readonly IFinanceRepository _financeRepository = financeRepository;
        private readonly ISupplierRepository _supplierRepository = supplierRepository;

        public async Task<SupplierLedgerResultDto> Handle(GetSupplierLedgerQuery request, CancellationToken cancellationToken)
        {
            var ledger = await _financeRepository.GetLedgerAsync(request.SupplierId);
            var supplier = await _supplierRepository.GetByIdAsync(request.SupplierId);

            return new SupplierLedgerResultDto
            {
                SupplierName = supplier?.Name ?? "Unknown",
                Ledger = ledger
            };
        }
    }
}
