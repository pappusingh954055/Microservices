using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Suppliers.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Suppliers.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FinanceController : ControllerBase
    {
        private readonly SupplierDbContext _context;

        public FinanceController(SupplierDbContext context)
        {
            _context = context;
        }

        // 1. Supplier Ledger (Khaata)
        [HttpGet("ledger/{supplierId}")]
        public async Task<IActionResult> GetLedger(int supplierId)
        {
            var ledger = await _context.SupplierLedgers
                .Where(l => l.SupplierId == supplierId)
                .OrderByDescending(l => l.TransactionDate)
                .ToListAsync();

            var supplier = await _context.Suppliers.FindAsync(supplierId);
            
            return Ok(new { 
                SupplierName = supplier?.Name,
                Ledger = ledger 
            });
        }

        // 2. Payment Entry
        [HttpPost("payment")]
        public async Task<IActionResult> RecordPayment([FromBody] SupplierPayment payment)
        {
            _context.SupplierPayments.Add(payment);

            // Update Ledger
            var lastLedger = await _context.SupplierLedgers
                .Where(l => l.SupplierId == payment.SupplierId)
                .OrderByDescending(l => l.Id)
                .FirstOrDefaultAsync();

            decimal currentBalance = (lastLedger?.Balance ?? 0) - payment.Amount;

            var ledgerEntry = new SupplierLedger
            {
                SupplierId = payment.SupplierId,
                TransactionType = "Payment",
                ReferenceId = payment.ReferenceNumber ?? "PAY-" + System.Guid.NewGuid().ToString().Substring(0,8),
                Debit = payment.Amount,
                Credit = 0,
                Balance = currentBalance,
                TransactionDate = payment.PaymentDate,
                Description = "Payment Made: " + payment.PaymentMode
            };

            _context.SupplierLedgers.Add(ledgerEntry);
            await _context.SaveChangesAsync();

            return Ok(payment);
        }

        // 3. Pending Dues Report
        [HttpGet("pending-dues")]
        public async Task<IActionResult> GetPendingDues()
        {
            var dues = await _context.SupplierLedgers
                .GroupBy(l => l.SupplierId)
                .Select(g => new
                {
                    SupplierId = g.Key,
                    TotalBalance = g.OrderByDescending(l => l.Id).First().Balance
                })
                .Where(x => x.TotalBalance > 0)
                .ToListAsync();

            return Ok(dues);
        }
    }
}
