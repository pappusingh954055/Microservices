using Suppliers.Application.DTOs;
using Suppliers.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Suppliers.Application.Interfaces // Adjust namespace if needed
{
    public interface IFinanceRepository
    {
        Task AddPaymentAsync(SupplierPayment payment);
        Task<SupplierLedger?> GetLastLedgerEntryAsync(int supplierId);
        Task AddLedgerEntryAsync(SupplierLedger ledgerEntry);
        Task SaveChangesAsync();

        Task<SupplierLedgerPagedResultDto> GetLedgerAsync(SupplierLedgerRequestDto request);
        Task<List<PendingDueDto>> GetPendingDuesAsync();
        Task<decimal> GetTotalPaymentsAsync(DateRangeDto dateRange);
        Task<Dictionary<string, decimal>> GetGRNPaymentStatusesAsync(List<string> grnNumbers);
        Task<PaginatedListDto<PaymentReportDto>> GetPaymentsReportAsync(PaymentReportRequestDto request);
        Task<decimal> GetTotalPendingDuesAsync();
        Task<Dictionary<int, decimal>> GetSupplierBalancesAsync(List<int> supplierIds);
        Task<List<MonthlyTrendDto>> GetMonthlyTrendAsync(int months);
    }
}
