using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Suppliers.Application.DTOs;
using Suppliers.Application.Features.Suppliers.Commands;
using Suppliers.Application.Features.Suppliers.Queries;
using Suppliers.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Suppliers.API.Controllers
{
    [Route("api/suppliers/finance")]
    [ApiController]
    public class FinanceController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FinanceController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // 1. Supplier Ledger (Khaata)
        [HttpGet("ledger/{supplierId}")]
        public async Task<IActionResult> GetLedger(int supplierId)
        {
            var result = await _mediator.Send(new GetSupplierLedgerQuery(supplierId));
            return Ok(result);
        }

        // 2. Payment Entry
        [HttpPost("payment")]
        public async Task<IActionResult> RecordPayment([FromBody] SupplierPayment payment)
        {
            var command = new RecordSupplierPaymentCommand(new SupplierPaymentDto
            {
                SupplierId = payment.SupplierId,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                PaymentMode = payment.PaymentMode,
                ReferenceNumber = payment.ReferenceNumber,
                Remarks = payment.Remarks,
                CreatedBy = payment.CreatedBy
            });

            var id = await _mediator.Send(command);
            payment.Id = id;

            return Ok(payment);
        }

        // 3. Pending Dues Report
        [HttpGet("pending-dues")]
        public async Task<IActionResult> GetPendingDues()
        {
            var result = await _mediator.Send(new GetPendingDuesQuery());
            return Ok(result);
        }

        // 4. Total Payments (For P&L)
        [HttpPost("total-payments")]
        public async Task<IActionResult> GetTotalPayments([FromBody] DateRangeDto dateRange)
        {
            var totalPayments = await _mediator.Send(new GetTotalPaymentsQuery(dateRange));
            return Ok(new { TotalPayments = totalPayments });
        }
    }
}
