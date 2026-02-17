using MediatR;
using Suppliers.Application.DTOs;
using System;
using System.Collections.Generic;

namespace Suppliers.Application.Features.Suppliers.Queries
{
    public record GetSupplierLedgerQuery(int SupplierId) : IRequest<SupplierLedgerResultDto>;

    public record GetPendingDuesQuery() : IRequest<List<PendingDueDto>>;

    public record GetTotalPaymentsQuery(DateRangeDto DateRange) : IRequest<decimal>;
    
    public record GetGRNPaymentStatusesQuery(List<string> GrnNumbers) : IRequest<Dictionary<string, decimal>>;

    public record GetPaymentsReportQuery(PaymentReportRequestDto Request) : IRequest<PaginatedListDto<PaymentReportDto>>;
    public record GetTotalPendingDuesQuery() : IRequest<decimal>;
}
