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
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 10)
        {
            // Frontend se aane wale parameters ko Command mein pass karein [cite: 2026-01-22]
            var command = new GetCurrentStockCommand(search, sortField, sortOrder, pageIndex, pageSize);

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
