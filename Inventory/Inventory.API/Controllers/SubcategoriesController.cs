using Inventory.API.Common;
using Inventory.Application.Subcategories.Commands.CreateSubcategory;
using Inventory.Application.Subcategories.Commands.DeleteSubcategory;
using Inventory.Application.Subcategories.Commands.UpdateSubcategory;
using Inventory.Application.Subcategories.Queries.GetSubcategories;
using Inventory.Application.Subcategories.Queries.GetSubcategoryById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubcategoriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SubcategoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
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
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetSubcategoryByIdQuery(id));
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetSubcategoriesQuery());
            return Ok(result);
        }
    }
}
