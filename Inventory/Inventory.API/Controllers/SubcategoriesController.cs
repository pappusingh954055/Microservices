using Inventory.API.Common;
using Inventory.Application.Common.Models;
using Inventory.Application.Subcategories.Commands.CreateSubcategory;
using Inventory.Application.Subcategories.Commands.Delete;
using Inventory.Application.Subcategories.Commands.UpdateSubcategory;
using Inventory.Application.Subcategories.Queries.GetSubcategories;
using Inventory.Application.Subcategories.Queries.GetSubcategoryById;
using Inventory.Application.Subcategories.Queries.Searching;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [Route("api/subcategories")]
    [ApiController]
    public class SubcategoriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SubcategoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Roles = "Manager, Admin,User")]
        public async Task<IActionResult> Create(CreateSubcategoryCommand command)
        {
            var id = await _mediator.Send(command);
            return Ok(
            ApiResponse<Guid>.Ok(
                id,
                "Sub category created successfully"
            ));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Manager, Admin, User")]
        public async Task<IActionResult> Update(
          Guid id,
          UpdateSubcategoryCommand command)
        {
            if (id != command.Id)
                return BadRequest(
                    ApiResponse<string>.Fail("Id mismatch"));

            var result = await _mediator.Send(command);

            return Ok(
                ApiResponse<Guid>.Ok(
                    result,
                    "Subcategory updated successfully"
                )
            );
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager, Admin, User")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _mediator.Send(
                new DeleteSubcategoryCommand(id));

            return Ok(
                ApiResponse<Guid>.Ok(
                    result,
                    "Subcategory deleted successfully"
                )
            );
        }


        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Manager, Admin, User")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetSubcategoryByIdQuery(id));
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Manager, Admin, User")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetSubcategoriesQuery());
            return Ok(result);
        }

        [HttpGet("by-category/{categoryId}")]
        [Authorize(Roles = "Manager, Admin, User")]
        public async Task<IActionResult> GetByCategory(Guid categoryId)
        {
            var result = await _mediator.Send(
                new GetSubcategoriesByCategoryQuery(categoryId));

            return Ok(result);
        }

        [HttpPost("paged")]
        [Authorize(Roles = "Manager, Admin,User")]
        public async Task<IActionResult> GetPaged(
            [FromBody] GridRequest request)
        {
            var result = await _mediator.Send(
                new GetSubcategoriesPagedQuery(request)
            );

            return Ok(result);
        }

        [HttpPost("bulk-delete")]
        [Authorize(Roles = "Manager, Admin")]
        public async Task<IActionResult> BulkDelete([FromBody] List<Guid> ids)
        {
            try
            {
                await _mediator.Send(new BulkDeleteSubCategoriesCommand(ids));

                return Ok(new
                {
                    success = true,
                    message = "Category deleted successfully"
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
