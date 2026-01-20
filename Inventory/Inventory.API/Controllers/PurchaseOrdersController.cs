using Inventory.Application.PurchaseOrders.Queries.GetNextPoNumber;
using MediatR;
using Microsoft.AspNetCore.Http;
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
        public async Task<IActionResult> GetNextNumber()
        {
            // MediatR command bhej raha hai handler ko
            var result = await _mediator.Send(new GetNextPoNumberQuery());
            return Ok(new { poNumber = result });
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderCommand command)
        {
            if (command == null) return BadRequest("Invalid payload");

            try
            {
                // Mediator command ko handler tak bhejega
                var resultId = await _mediator.Send(command);

                // Response structure jo Angular expect kar raha hai
                return Ok(new
                {
                    success = true,
                    message = "Purchase Order created successfully!",
                    id = resultId
                });
            }
            catch (Exception ex)
            {
                // Logging yahan karein
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
