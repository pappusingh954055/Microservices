using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Customers.Application.DTOs;
using Customers.Application.Features.Finance.Commands;
using Customers.Application.Features.Finance.Queries;
using Customers.Domain.Entities;

namespace Customers.API.Controllers
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

        // 1. Customer Ledger
        [HttpGet("ledger/{customerId}")]
        [Authorize(Roles = "Admin, User, Manager, Employee, Warehouse")]
        public async Task<IActionResult> GetLedger(int customerId)
        {
            var result = await _mediator.Send(new GetCustomerLedgerQuery(customerId));
            return Ok(result);
        }

        // 2. Receipt Entry
        [HttpPost("receipt")]
        [Authorize(Roles = "Admin, User, Manager, Employee, Warehouse")]
        public async Task<IActionResult> RecordReceipt([FromBody] CustomerReceiptDto receiptDto)
        {
            var command = new RecordCustomerReceiptCommand(receiptDto);
            var id = await _mediator.Send(command);
            
            return Ok(new { Id = id }); // Returning object to be consistent
        }

        // 2b. Sale Entry (called from Inventory when Sale is confirmed)
        [HttpPost("sale")]
        [Authorize(Roles = "Admin, User, Manager, Employee, Warehouse")]
        public async Task<IActionResult> RecordSale([FromBody] CustomerSaleDto saleDto)
        {
            var command = new RecordCustomerSaleCommand(saleDto);
            var id = await _mediator.Send(command);
            return Ok(new { Id = id });
        }

        // 3. Outstanding Tracker
        [HttpGet("outstanding")]
        [Authorize(Roles = "Admin, User, Manager, Employee, Warehouse")]
        public async Task<IActionResult> GetOutstanding()
        {
            var result = await _mediator.Send(new GetOutstandingQuery());
            return Ok(result);
        }

        [HttpGet("outstanding-total")]
        [Authorize(Roles = "Admin, User, Manager, Employee, Warehouse")]
        public async Task<IActionResult> GetOutstandingTotal()
        {
            var total = await _mediator.Send(new GetTotalOutstandingQuery());
            return Ok(new { TotalOutstanding = total });
        }

        // 4. Total Receipts (For P&L)
        [HttpPost("total-receipts")]
        [Authorize(Roles = "Admin, User, Manager, Employee, Warehouse")]
        public async Task<IActionResult> GetTotalReceipts([FromBody] DateRangeDto dateRange)
        {
            var total = await _mediator.Send(new GetTotalReceiptsQuery(dateRange));
            return Ok(new { TotalReceipts = total });
        }
    }
}
