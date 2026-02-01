using Inventory.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly IMediator _mediator;
        IStockRepository _stockRepo;
        public StockController(IMediator mediator, IStockRepository stockRepo)
        {
            _mediator = mediator;
            _stockRepo = stockRepo;

        }

        [HttpGet("current-stock")]
        public async Task<IActionResult> GetStock(
           [FromQuery] string? search,
           [FromQuery] string? sortField,
           [FromQuery] string? sortOrder,
           [FromQuery] DateTime? startDate, // Naya parameter
           [FromQuery] DateTime? endDate,   // Naya parameter
           [FromQuery] int pageIndex = 0,
           [FromQuery] int pageSize = 10)
        {
            // Dates ko bhi command mein pass karein
            var command = new GetCurrentStockCommand(search, sortField, sortOrder, pageIndex, pageSize, startDate, endDate);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("ExportExcel")]
        public async Task<IActionResult> ExportExcel([FromBody] List<Guid> productIds)
        {
            if (productIds == null || !productIds.Any())
                return BadRequest("No products selected.");

            try
            {
                var fileContent = await _stockRepo.GenerateStockExcel(productIds);
                string fileName = $"StockReport_{DateTime.Now:yyyyMMdd}.xlsx";

                // File content type for Excel
                return File(
                    fileContent,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
