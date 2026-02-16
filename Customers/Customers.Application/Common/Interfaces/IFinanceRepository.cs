using Customers.Application.DTOs;
using Customers.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Customers.Application.Common.Interfaces
{
    public interface IFinanceRepository
    {
        Task AddReceiptAsync(CustomerReceipt receipt);
        Task<CustomerLedger?> GetLastLedgerEntryAsync(int customerId);
        Task AddLedgerEntryAsync(CustomerLedger ledgerEntry);
        Task SaveChangesAsync();

        Task<List<CustomerLedger>> GetLedgerAsync(int customerId);
        Task<List<OutstandingDto>> GetOutstandingAsync();
        Task<decimal> GetTotalReceiptsAsync(DateRangeDto dateRange);
    }
}
