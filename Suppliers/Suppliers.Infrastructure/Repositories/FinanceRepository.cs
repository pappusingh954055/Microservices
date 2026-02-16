using Microsoft.EntityFrameworkCore;
using Suppliers.Application.DTOs;
using Suppliers.Application.Interfaces;
using Suppliers.Domain.Entities;
using Suppliers.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Suppliers.Infrastructure.Repositories // Adjust namespace if needed
{
    public class FinanceRepository(SupplierDbContext context) : IFinanceRepository
    {
        private readonly SupplierDbContext _context = context;

        public async Task AddPaymentAsync(SupplierPayment payment)
        {
            await _context.SupplierPayments.AddAsync(payment);
        }

        public async Task<SupplierLedger?> GetLastLedgerEntryAsync(int supplierId)
        {
            return await _context.SupplierLedgers
                .Where(l => l.SupplierId == supplierId)
                .OrderByDescending(l => l.Id)
                .FirstOrDefaultAsync();
        }

        public async Task AddLedgerEntryAsync(SupplierLedger ledgerEntry)
        {
            await _context.SupplierLedgers.AddAsync(ledgerEntry);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<SupplierLedger>> GetLedgerAsync(int supplierId)
        {
            return await _context.SupplierLedgers
                .Where(l => l.SupplierId == supplierId)
                .OrderByDescending(l => l.TransactionDate)
                .ToListAsync();
        }

        public async Task<List<PendingDueDto>> GetPendingDuesAsync()
        {
            var dues = await _context.SupplierLedgers
                .GroupBy(l => l.SupplierId)
                .Select(g => new
                {
                    SupplierId = g.Key,
                    PendingAmount = g.OrderByDescending(l => l.Id).First().Balance
                })
                .Where(x => x.PendingAmount > 0)
                .ToListAsync();

            var supplierIds = dues.Select(d => d.SupplierId).ToList();
            var suppliers = await _context.Suppliers.Where(s => supplierIds.Contains(s.Id)).ToListAsync();

            return dues.Select(d => new PendingDueDto
            {
                SupplierId = d.SupplierId,
                PendingAmount = d.PendingAmount,
                SupplierName = suppliers.FirstOrDefault(s => s.Id == d.SupplierId)?.Name ?? "Unknown",
                Status = "Overdue",
                DueDate = System.DateTime.Now.AddDays(7)
            }).ToList();
        }

        public async Task<decimal> GetTotalPaymentsAsync(DateRangeDto dateRange)
        {
            return await _context.SupplierPayments
                .Where(p => p.PaymentDate >= dateRange.StartDate && p.PaymentDate <= dateRange.EndDate)
                .SumAsync(p => p.Amount);
        }
    }
}
