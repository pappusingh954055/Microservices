using Inventory.API.Common;
using Inventory.Application.Common.Models;
using Inventory.Application.PriceLists.Commands.CreatePriceList;
using Inventory.Application.PriceLists.Commands.DeletePriceList;
using Inventory.Application.PriceLists.Commands.UpdatePriceList;
using Inventory.Application.PriceLists.Queries.GetPriceListById;
using Inventory.Application.PriceLists.Queries.GetPriceLists;
using Inventory.Application.PriceLists.Queries.Paged;
using Inventory.Application.Subcategories.Commands.Delete;
using Inventory.Application.Subcategories.Queries.Searching;
using MediatR;
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
        public async Task<IActionResult> Create(CreatePriceListCommand command)
        {
            var id = await _mediator.Send(command);
            return Ok(
           ApiResponse<Guid>.Ok(
               id,
               "Price list created successfully"
           ));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
                    Guid id,
                    UpdatePriceListCommand command)
        {
            if (id != command.Id)
                return BadRequest(
                    ApiResponse<string>.Fail("Id mismatch"));

            var result = await _mediator.Send(command);

            return Ok(
                ApiResponse<Guid>.Ok(
                    result,
                    "Price list updated successfully"
                )
            );
        }


        [HttpDelete("{id}")]
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


        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetPriceListByIdQuery(id));
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetPriceListsQuery());
            return Ok(result);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] GridRequest request)
        {
            var result = await _mediator.Send(
                new GetPriceListsPagedQuery(request)
            );

            return Ok(result);
        }

        [HttpPost("bulk-delete")]
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
