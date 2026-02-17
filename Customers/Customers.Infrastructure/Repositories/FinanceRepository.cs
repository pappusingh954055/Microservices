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

        public async Task<CustomerLedgerPagedResultDto> GetLedgerAsync(CustomerLedgerRequestDto request)
        {
            var customer = await _context.Customers.FindAsync(request.CustomerId);
            var query = _context.CustomerLedgers.Where(l => l.CustomerId == request.CustomerId);

            // 1. Date Filtering
            if (request.StartDate.HasValue)
                query = query.Where(l => l.TransactionDate >= request.StartDate.Value);
            if (request.EndDate.HasValue)
                query = query.Where(l => l.TransactionDate <= request.EndDate.Value);

            // 1.1 Column Specific Filters
            if (!string.IsNullOrWhiteSpace(request.TypeFilter))
            {
                var type = request.TypeFilter.ToLower();
                query = query.Where(l => l.TransactionType.ToLower().Contains(type));
            }

            if (!string.IsNullOrWhiteSpace(request.ReferenceFilter))
            {
                var refId = request.ReferenceFilter.ToLower();
                query = query.Where(l => l.ReferenceId != null && l.ReferenceId.ToLower().Contains(refId));
            }

            // 2. Searching (Global)
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                query = query.Where(l => 
                    l.TransactionType.ToLower().Contains(term) || 
                    (l.ReferenceId != null && l.ReferenceId.ToLower().Contains(term)) || 
                    (l.Description != null && l.Description.ToLower().Contains(term))
                );
            }

            // 3. Sorting
            query = request.SortBy.ToLower() switch
            {
                "transactiondate" => request.SortOrder == "desc" ? query.OrderByDescending(l => l.TransactionDate) : query.OrderBy(l => l.TransactionDate),
                "transactiontype" => request.SortOrder == "desc" ? query.OrderByDescending(l => l.TransactionType) : query.OrderBy(l => l.TransactionType),
                "referenceid" => request.SortOrder == "desc" ? query.OrderByDescending(l => l.ReferenceId) : query.OrderBy(l => l.ReferenceId),
                "debit" => request.SortOrder == "desc" ? query.OrderByDescending(l => l.Debit) : query.OrderBy(l => l.Debit),
                "credit" => request.SortOrder == "desc" ? query.OrderByDescending(l => l.Credit) : query.OrderBy(l => l.Credit),
                "balance" => request.SortOrder == "desc" ? query.OrderByDescending(l => l.Balance) : query.OrderBy(l => l.Balance),
                _ => query.OrderByDescending(l => l.TransactionDate)
            };

            // 4. Counts and Summaries
            var totalCount = await query.CountAsync();
            var currentBalance = await _context.CustomerLedgers
                .Where(l => l.CustomerId == request.CustomerId)
                .OrderByDescending(l => l.Id)
                .Select(l => l.Balance)
                .FirstOrDefaultAsync();

            // 5. Pagination
            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new CustomerLedgerPagedResultDto
            {
                CustomerName = customer?.CustomerName ?? "Unknown",
                CurrentBalance = currentBalance,
                Ledger = new PaginatedListDto<CustomerLedger>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                }
            };
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
