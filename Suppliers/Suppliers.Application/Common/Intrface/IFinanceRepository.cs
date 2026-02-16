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

        Task<List<SupplierLedger>> GetLedgerAsync(int supplierId);
        Task<List<PendingDueDto>> GetPendingDuesAsync();
        Task<decimal> GetTotalPaymentsAsync(DateRangeDto dateRange);
    }
}
