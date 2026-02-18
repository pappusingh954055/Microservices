using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Suppliers.Application.DTOs;
using Suppliers.Application.Features.Suppliers.Commands;
using Suppliers.Application.Features.Suppliers.Queries;
using Suppliers.Domain.Entities;

namespace Suppliers.API.Controllers
{
    [Route("api/finance")]
    [ApiController]
    public class FinanceController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FinanceController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // 1. Supplier Ledger (Khaata)
        [HttpPost("ledger")]
        //[Authorize(Roles = "Admin, User, Manager, Employee, Warehouse")]
        public async Task<IActionResult> GetLedger([FromBody] SupplierLedgerRequestDto request)
        {
            var result = await _mediator.Send(new GetSupplierLedgerQuery(request));
            return Ok(result);
        }

        // 2. Payment Entry
        [HttpPost("payment-entry")]
        //[Authorize(Roles = "Admin, User, Manager, Employee, Warehouse")]
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

        // 2.1 Purchase Entry (From Inventory GRN)
        [HttpPost("purchase-entry")]
        //[Authorize(Roles = "Admin, User, Manager, Employee, Warehouse")]
        public async Task<IActionResult> RecordPurchase([FromBody] SupplierPurchaseDto purchase)
        {
            var command = new RecordSupplierPurchaseCommand(purchase);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        // 3. Pending Dues Report
        [HttpGet("pending-dues")]
        //[Authorize(Roles = "Admin, User, Manager, Employee, Warehouse")]
        public async Task<IActionResult> GetPendingDues()
        {
            var result = await _mediator.Send(new GetPendingDuesQuery());
            return Ok(result);
        }

        [HttpGet("pending-total")]
        [Authorize(Roles = "Admin, User, Manager, Employee, Warehouse")]
        public async Task<IActionResult> GetPendingTotal()
        {
            var total = await _mediator.Send(new GetTotalPendingDuesQuery());
            return Ok(new { TotalPending = total });
        }

        // 4. Total Payments (For P&L)
        [HttpPost("total-payments")]
        //[Authorize(Roles = "Admin, User, Manager, Employee, Warehouse")]
        public async Task<IActionResult> GetTotalPayments([FromBody] DateRangeDto dateRange)
        {
            var totalPayments = await _mediator.Send(new GetTotalPaymentsQuery(dateRange));
            return Ok(new { TotalPayments = totalPayments });
        }

        // 5. GRN Payment Status (For Inventory List)
        [HttpPost("get-grn-statuses")]
        //[Authorize(Roles = "Admin, User, Manager, Employee, Warehouse")]
        public async Task<IActionResult> GetGRNStatuses([FromBody] List<string> grnNumbers)
        {
            var result = await _mediator.Send(new GetGRNPaymentStatusesQuery(grnNumbers));
            return Ok(result);
        }

        // 5.1 Supplier Balances
        [HttpPost("get-balances")]
        //[Authorize(Roles = "Admin, User, Manager, Employee, Warehouse")]
        public async Task<IActionResult> GetSupplierBalances([FromBody] List<int> supplierIds)
        {
            var result = await _mediator.Send(new GetSupplierBalancesQuery(supplierIds));
            return Ok(result);
        }

        // 6. Payments Report
        [HttpPost("payments-report")]
        //[Authorize(Roles = "Admin, User, Manager, Employee, Warehouse")]
        public async Task<IActionResult> GetPaymentsReport([FromBody] PaymentReportRequestDto request)
        {
            var result = await _mediator.Send(new GetPaymentsReportQuery(request));
            return Ok(result);
        }

        [HttpGet("monthly-payments")]
        public async Task<IActionResult> GetMonthlyPayments([FromQuery] int months = 6)
        {
            var result = await _mediator.Send(new GetMonthlyPaymentsTrendQuery(months));
            return Ok(result);
        }
    }
}
