using Customers.Application.Common.Interfaces;
using Customers.Application.DTOs;
using Customers.Domain.Entities;
using Customers.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Customers.Infrastructure.Repositories
{
    public class FinanceRepository : IFinanceRepository
    {
        private readonly CustomerDbContext _context;

        public FinanceRepository(CustomerDbContext context)
        {
            _context = context;
        }

        public async Task AddReceiptAsync(CustomerReceipt receipt)
        {
            await _context.CustomerReceipts.AddAsync(receipt);
        }

        public async Task<CustomerLedger?> GetLastLedgerEntryAsync(int customerId)
        {
            return await _context.CustomerLedgers
                .Where(l => l.CustomerId == customerId)
                .OrderByDescending(l => l.Id)
                .FirstOrDefaultAsync();
        }

        public async Task AddLedgerEntryAsync(CustomerLedger ledgerEntry)
        {
            await _context.CustomerLedgers.AddAsync(ledgerEntry);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<CustomerLedger>> GetLedgerAsync(int customerId)
        {
            return await _context.CustomerLedgers
                .Where(l => l.CustomerId == customerId)
                .OrderByDescending(l => l.TransactionDate)
                .ToListAsync();
        }

        public async Task<List<OutstandingDto>> GetOutstandingAsync()
        {
            // Get last entry for each customer
            var latestEntries = await _context.CustomerLedgers
                .GroupBy(l => l.CustomerId)
                .Select(g => g.OrderByDescending(l => l.Id).FirstOrDefault())
                .ToListAsync();

            var outstanding = latestEntries
                .Where(l => l != null && l.Balance > 0)
                .Select(l => new { l.CustomerId, PendingAmount = l.Balance })
                .ToList();

            var customerIds = outstanding.Select(o => o.CustomerId).ToList();
            var customers = await _context.Customers.Where(c => customerIds.Contains(c.Id)).ToListAsync();

            return outstanding.Select(o => new OutstandingDto
            {
                CustomerId = o.CustomerId,
                PendingAmount = o.PendingAmount,
                TotalAmount = o.PendingAmount,
                CustomerName = customers.FirstOrDefault(c => c.Id == o.CustomerId)?.CustomerName ?? "Unknown",
                Status = "Active",
                DueDate = System.DateTime.Now.AddDays(7)
            }).ToList();
        }

        public async Task<decimal> GetTotalReceiptsAsync(DateRangeDto dateRange)
        {
            return await _context.CustomerReceipts
                .Where(r => r.ReceiptDate >= dateRange.StartDate && r.ReceiptDate <= dateRange.EndDate)
                .SumAsync(r => r.Amount);
        }

        public async Task<decimal> GetTotalOutstandingAsync()
        {
            // Get all customer IDs
            var customerIds = await _context.CustomerLedgers
                .Select(l => l.CustomerId)
                .Distinct()
                .ToListAsync();

            decimal total = 0;
            foreach (var id in customerIds)
            {
                var lastBalance = await _context.CustomerLedgers
                    .Where(l => l.CustomerId == id)
                    .OrderByDescending(l => l.Id)
                    .Select(l => l.Balance)
                    .FirstOrDefaultAsync();
                
                total += lastBalance;
            }

            return total;
        }
    }
}
