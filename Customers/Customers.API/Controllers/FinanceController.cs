using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Customers.Domain.Entities;
using Customers.Infrastructure.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                    TotalBalance = g.OrderByDescending(l => l.Id).First().Balance
                })
                .Where(x => x.TotalBalance > 0)
                .ToListAsync();

            return Ok(outstanding);
        }
    }
}
