using Customers.Domain.Entities;
using Customers.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Customers.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FinanceController : ControllerBase
    {
        private readonly CustomerDbContext _context;

        public FinanceController(CustomerDbContext context)
        {
            _context = context;
        }

        // 1. Customer Ledger
        [HttpGet("ledger/{customerId}")]
        public async Task<IActionResult> GetLedger(int customerId)
        {
            var ledger = await _context.CustomerLedgers
                .Where(l => l.CustomerId == customerId)
                .OrderByDescending(l => l.TransactionDate)
                .ToListAsync();

            var customer = await _context.Customers.FindAsync(customerId);
            
            return Ok(new { 
                CustomerName = customer?.CustomerName,
                Ledger = ledger 
            });
        }

        // 2. Receipt Entry
        [HttpPost("receipt")]
        public async Task<IActionResult> RecordReceipt([FromBody] CustomerReceipt receipt)
        {
            _context.CustomerReceipts.Add(receipt);

            // Update Ledger
            var lastLedger = await _context.CustomerLedgers
                .Where(l => l.CustomerId == receipt.CustomerId)
                .OrderByDescending(l => l.Id)
                .FirstOrDefaultAsync();

            // For customers, Receipt decreases the balance (Credit)
            decimal currentBalance = (lastLedger?.Balance ?? 0) - receipt.Amount;

            var ledgerEntry = new CustomerLedger
            {
                CustomerId = receipt.CustomerId,
                TransactionType = "Receipt",
                ReferenceId = receipt.ReferenceNumber ?? "REC-" + System.Guid.NewGuid().ToString().Substring(0,8),
                Debit = 0,
                Credit = receipt.Amount,
                Balance = currentBalance,
                TransactionDate = receipt.ReceiptDate,
                Description = "Receipt Received: " + receipt.ReceiptMode
            };

            _context.CustomerLedgers.Add(ledgerEntry);
            await _context.SaveChangesAsync();

            return Ok(receipt);
        }

        // 3. Outstanding Tracker
        [HttpGet("outstanding")]
        public async Task<IActionResult> GetOutstanding()
        {
            var outstanding = await _context.CustomerLedgers
                .GroupBy(l => l.CustomerId)
                .Select(g => new
                {
                    CustomerId = g.Key,
                    PendingAmount = g.OrderByDescending(l => l.Id).First().Balance
                })
                .Where(x => x.PendingAmount > 0)
                .ToListAsync();

            var customerIds = outstanding.Select(o => o.CustomerId).ToList();
            var customers = await _context.Customers.Where(c => customerIds.Contains(c.Id)).ToListAsync();

            var result = outstanding.Select(o => new
            {
                o.CustomerId,
                o.PendingAmount,
                TotalAmount = o.PendingAmount * 1.2m, // Placeholder logic for Total vs Pending
                CustomerName = customers.FirstOrDefault(c => c.Id == o.CustomerId)?.CustomerName ?? "Unknown",
                Status = "Overdue",
                DueDate = DateTime.Now.AddDays(7)
            });

            return Ok(result);
        }

        // 4. Total Receipts (For P&L)
        [HttpPost("total-receipts")]
        public async Task<IActionResult> GetTotalReceipts([FromBody] DateRangeDto dateRange)
        {
            var totalReceipts = await _context.CustomerReceipts
                .Where(r => r.ReceiptDate >= dateRange.StartDate && r.ReceiptDate <= dateRange.EndDate)
                .SumAsync(r => r.Amount);

            return Ok(new { TotalReceipts = totalReceipts });
        }
    }

    public class DateRangeDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    
}
