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
                    PendingAmount = g.OrderByDescending(l => l.Id).First().Balance
                })
                .Where(x => x.PendingAmount > 0)
                .ToListAsync();

            var supplierIds = dues.Select(d => d.SupplierId).ToList();
            var suppliers = await _context.Suppliers.Where(s => supplierIds.Contains(s.Id)).ToListAsync();

            var result = dues.Select(d => new 
            {
                d.SupplierId,
                d.PendingAmount,
                SupplierName = suppliers.FirstOrDefault(s => s.Id == d.SupplierId)?.Name ?? "Unknown",
                Status = "Overdue",
                DueDate = DateTime.Now.AddDays(7)
            });

            return Ok(result);
        }

        // 4. Total Payments (For P&L)
        [HttpPost("total-payments")]
        public async Task<IActionResult> GetTotalPayments([FromBody] DateRangeDto dateRange)
        {
            var totalPayments = await _context.SupplierPayments
                .Where(p => p.PaymentDate >= dateRange.StartDate && p.PaymentDate <= dateRange.EndDate)
                .SumAsync(p => p.Amount);

            return Ok(new { TotalPayments = totalPayments });
        }
    }

    public class DateRangeDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
