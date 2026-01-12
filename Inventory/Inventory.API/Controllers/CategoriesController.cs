using Inventory.API.Common;
using Inventory.Application.Categories.Commands.CreateCategory;
using Inventory.Application.Categories.Commands.DeleteCategory;
using Inventory.Application.Categories.Commands.UpdateCategory;
using Inventory.Application.Categories.Queries.GetCategoryById;
using Inventory.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public sealed class CategoriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryCommand command)
        {
            var id = await _mediator.Send(command);
            return Ok(
           ApiResponse<Guid>.Ok(
               id,
               "Categories created successfully"
           ));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            Guid id,
            UpdateCategoryCommand command)
        {
            if (id != command.Id)
                return BadRequest(
                    ApiResponse<string>.Fail("Id mismatch"));

            var result = await _mediator.Send(command);

            return Ok(
                ApiResponse<Guid>.Ok(
                    result,
                    "Category updated successfully"
                )
            );
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _mediator.Send(
                new DeleteCategoryCommand(id));

            return Ok(
                ApiResponse<Guid>.Ok(
                    result,
                    "Category deleted successfully"
                )
            );
        }

        //[HttpPost("bulk-delete")]
        //public async Task<IActionResult> BulkDelete([FromBody] List<Guid> ids)
        //{
        //    await _mediator.Send(new DeleteCategoryCommand(ids));
        //    return Ok(new { success = true });
        //}

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetCategoryByIdQuery(id));
            return result is null ? NotFound() : Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories(
            [FromQuery] PagedQuery query)
        {
            var result = await _mediator.Send(
                new GetCategoriesQuery(query));

            return Ok(result);
        }

    }
}
