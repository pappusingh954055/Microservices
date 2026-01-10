using Inventory.API.Common;
using Inventory.Application.Products.Commands.DeleteProduct;
using Inventory.Application.Products.Commands.UpdateProduct;
using Inventory.Application.Products.Queries.GetProductById;
using Inventory.Application.Products.Queries.GetProducts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductCommand command)
        {
            var id = await _mediator.Send(command);
            return Ok(
           ApiResponse<Guid>.Ok(
               id,
               "Product is created successfully"
           ));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
    Guid id,
    UpdateProductCommand command)
        {
            if (id != command.Id)
                return BadRequest(
                    ApiResponse<string>.Fail("Id mismatch"));

            var result = await _mediator.Send(command);

            return Ok(
                ApiResponse<Guid>.Ok(
                    result,
                    "Product updated successfully"
                )
            );
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _mediator.Send(
                new DeleteProductCommand(id));

            return Ok(
                ApiResponse<Guid>.Ok(
                    result,
                    "Product deleted successfully"
                )
            );
        }


        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetProductByIdQuery(id));
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetProductsQuery());
            return Ok(result);
        }
    }
}
