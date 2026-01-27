using Inventory.Application.GRN.Command;
using Inventory.Application.GRN.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GRNController : ControllerBase
    {
        private readonly IMediator _mediator;
        public GRNController(IMediator mediator) => _mediator = mediator;

        [HttpPost("Save")]
        [Authorize(Roles = "Warehouse")]
        public async Task<IActionResult> Save([FromBody] CreateGRNCommand command)
        {
            // Fix: result ab bool nahi balki string (GRN Number) hai
            string newGrnNumber = await _mediator.Send(command);

            if (!string.IsNullOrEmpty(newGrnNumber))
            {
                return Ok(new
                {
                    success = true,
                    message = "Stock Updated Successfully",
                    grnNumber = newGrnNumber // Frontend isse 'AUTO-GEN' ko replace karega
                });
            }

            return BadRequest(new { success = false, message = "Failed to update stock" });
        }

        [HttpGet("GetPOData/{poId}")]
        public async Task<IActionResult> GetPOData(int poId)
        {
            var data = await _mediator.Send(new GetPOForGRNQuery(poId));
            return data != null ? Ok(data) : NotFound("PO Not Found");
        }

        [HttpGet("grn-list")]
        public async Task<IActionResult> GetGRNList(
    [FromQuery] string? search = "",
    [FromQuery] string? sortField = "id", // Default value rakhein
    [FromQuery] string? sortOrder = "desc",
    [FromQuery] int pageIndex = 0,
    [FromQuery] int pageSize = 10)
        {
            var result = await _mediator.Send(new GetGRNListQuery(search ?? "", sortField ?? "id", sortOrder ?? "desc", pageIndex, pageSize));
            return Ok(result);
        }
    }

}
