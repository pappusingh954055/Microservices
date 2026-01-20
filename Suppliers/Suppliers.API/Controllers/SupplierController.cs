using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Suppliers.Application.Features.Suppliers.Queries;

namespace Suppliers.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SupplierController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // CREATE: POST api/v1/supplier
        [HttpPost]
       [Authorize(Roles = "Admin,Manager")]
        //[AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] CreateSupplierDto dto)
        {
            var command = new CreateSupplierCommand(dto);
            var id = await _mediator.Send(command);
            return Ok(id);
        }

        // READ: GET api/v1/supplier
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> GetAll()
        {
            // Yahan humein GetSuppliersQuery banani hogi (Next Step)
            var query = new GetAllSuppliersQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // UPDATE: PUT api/v1/supplier/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateSupplierDto dto)
        {
            var result = await _mediator.Send(new UpdateSupplierCommand(id, dto));
            return result ? NoContent() : NotFound();
        }

        // DELETE: DELETE api/v1/supplier/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteSupplierCommand(id));
            return result ? NoContent() : NotFound();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> GetById(int id)
        {
            // 1. Query create karna
            var query = new GetSupplierByIdQuery(id);

            // 2. Mediator ke through handler ko call karna
            var result = await _mediator.Send(query);

            // 3. Agar supplier nahi mila toh 404, warna data ke saath 200 OK
            if (result == null)
            {
                return NotFound(new { message = "Supplier not found" });
            }

            return Ok(result);
        }
    }
}
