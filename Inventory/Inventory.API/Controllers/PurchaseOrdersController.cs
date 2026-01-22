using Inventory.Application.PurchaseOrders.Queries.GetNextPoNumber;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseOrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PurchaseOrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("next-number")]
        [Authorize(Roles = "Manager, Admin")]
        public async Task<IActionResult> GetNextNumber()
        {
            // MediatR command bhej raha hai handler ko
            var result = await _mediator.Send(new GetNextPoNumberQuery());
            return Ok(new { poNumber = result });
        }

        [HttpPost("save-po")]
        [Authorize(Roles = "Manager, Admin")]
        public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderDto dto)
        {
           
            var result = await _mediator.Send(new CreatePurchaseOrderCommand(dto));

            if (result)
                return Ok(new { success = true, message = "Purchase Order Draft saved successfully!" });

            return BadRequest(new { success = false, message = "Failed to save PO." });
        }

        [HttpGet]
        [Authorize(Roles = "Manager, Admin")]
        public async Task<ActionResult> GetOrders([FromQuery] GetPurchaseOrdersQuery query)
        {
            // Ensure 'query' is not null
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
