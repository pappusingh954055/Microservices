using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly IMediator _mediator;
        public StockController(IMediator mediator) => _mediator = mediator;
        [HttpGet("current-stock")]
        public async Task<IActionResult> GetStock(
            [FromQuery] string? search,
            [FromQuery] string? sortField,
            [FromQuery] string? sortOrder,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 10)
        {
            // Frontend se aane wale parameters ko Command mein pass karein [cite: 2026-01-22]
            var command = new GetCurrentStockCommand(search, sortField, sortOrder, pageIndex, pageSize);

            var result = await _mediator.Send(command);

            return Ok(result);
        }
    }
}
