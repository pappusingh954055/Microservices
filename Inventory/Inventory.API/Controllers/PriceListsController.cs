using Inventory.API.Common;
using Inventory.Application.Common.Models;
using Inventory.Application.PriceLists.Commands.DeletePriceList;
using Inventory.Application.PriceLists.Commands.UpdatePriceList;
using Inventory.Application.PriceLists.Queries.GetPriceListById;
using Inventory.Application.PriceLists.Queries.GetPriceLists;
using Inventory.Application.PriceLists.Queries.Paged;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [Route("api/pricelists")]
    [ApiController]
    public class PriceListsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PriceListsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreatePriceListCommand command)
        {
            var resultId = await _mediator.Send(command);
            // Success object bhejien taaki frontend 'res.message' padh sake
            return Ok(new { success = true, message = "Price List saved successfully", id = resultId });
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, UpdatePriceListCommand command)
        {
            if (id != command.id) return BadRequest("ID Mismatch");

            var result = await _mediator.Send(command);
            return result ? Ok(new { message = "Updated successfully" }) : NotFound();
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _mediator.Send(
                new DeletePriceListCommand(id));

            return Ok(
                ApiResponse<Guid>.Ok(
                    result,
                    "Price list deleted successfully"
                )
            );
        }

        [Authorize(Roles = "Manager, Admin, User")]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetPriceListByIdQuery(id));
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Manager, Admin, User")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetPriceListsQuery());
            return Ok(result);
        }

        [HttpGet("paged")]
        [Authorize(Roles = "Manager, Admin, User")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] GridRequest request)
        {
            var result = await _mediator.Send(
                new GetPriceListsPagedQuery(request)
            );

            return Ok(result);
        }

        [HttpPost("bulk-delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkDelete([FromBody] List<Guid> ids)
        {
            try
            {
                await _mediator.Send(new BulkDeletePricelistsCommand(ids));

                return Ok(new
                {
                    success = true,
                    message = "Price lists deleted successfully"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}
