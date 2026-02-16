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

        public async Task<Dictionary<string, decimal>> GetGRNPaymentStatusesAsync(List<string> grnNumbers)
        {
            if (grnNumbers == null || !grnNumbers.Any()) return new Dictionary<string, decimal>();

            var result = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            // Fetch all payments that might be relevant. 
            var relevantPayments = await _context.SupplierLedgers
                .Where(l => l.TransactionType == "Payment" && l.Description != null)
                .Select(l => new { l.Description, l.ReferenceId, l.Debit })
                .ToListAsync();

            foreach (var grn in grnNumbers)
            {
                // Calculate total paid for this GRN
                decimal totalPaid = relevantPayments
                    .Where(p => 
                        (p.Description != null && p.Description.Contains(grn, StringComparison.OrdinalIgnoreCase)) ||
                        (p.ReferenceId != null && p.ReferenceId.Contains(grn, StringComparison.OrdinalIgnoreCase))
                    )
                    .Sum(p => p.Debit);

                result[grn] = totalPaid;
            }

            return result;
        }
        public async Task<List<PaymentReportDto>> GetPaymentsReportAsync(DateRangeDto dateRange)
        {
            var payments = await _context.SupplierPayments
                .Where(p => p.PaymentDate >= dateRange.StartDate && p.PaymentDate <= dateRange.EndDate)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            var supplierIds = payments.Select(p => p.SupplierId).Distinct().ToList();
            var suppliers = await _context.Suppliers.Where(s => supplierIds.Contains(s.Id)).ToListAsync();

            return payments.Select(p => new PaymentReportDto
            {
                Id = p.Id,
                SupplierId = p.SupplierId,
                SupplierName = suppliers.FirstOrDefault(s => s.Id == p.SupplierId)?.Name ?? "Unknown",
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                PaymentMode = p.PaymentMode,
                ReferenceNumber = p.ReferenceNumber,
                Remarks = p.Remarks,
                CreatedBy = p.CreatedBy
            }).ToList();
        }
    }
}
