using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Suppliers.Application.DTOs;
using Suppliers.Application.Features.Suppliers.Queries;

namespace Suppliers.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierController : ControllerBase
    {
        private readonly IMediator _mediator;

        private readonly ISupplierRepository _supplierRepository;

        public SupplierController(IMediator mediator, ISupplierRepository repository)
        {
            _mediator = mediator;
            _supplierRepository = repository;
        }

        // CREATE: POST api/v1/supplier
        [HttpPost]
        [Authorize(Roles = "Manager, Admin,User, Warehouse")]
        //[AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] CreateSupplierDto dto)
        {
            var command = new CreateSupplierCommand(dto);
            var id = await _mediator.Send(command);
            return Ok(id);
        }

        // READ: GET api/v1/supplier
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,User,Warehouse")]
        public async Task<IActionResult> GetAll()
        {
            // Yahan humein GetSuppliersQuery banani hogi (Next Step)
            var query = new GetAllSuppliersQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // UPDATE: PUT api/v1/supplier/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Manager, Admin, User")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateSupplierDto dto)
        {
            var result = await _mediator.Send(new UpdateSupplierCommand(id, dto));
            return result ? NoContent() : NotFound();
        }

        // DELETE: DELETE api/v1/supplier/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager, Admin, User")]
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

        [HttpPost("get-by-ids")]
        public async Task<IActionResult> GetSuppliersByIds([FromBody] List<int> ids)
        {
            // Debugging ke liye log lagayein
            Console.WriteLine($"[SupplierService] Received IDs: {string.Join(",", ids)}"); 

            if (ids == null || ids.Count == 0)
            {
                return Ok(new List<SupplierSelectDto>());
            }

            try
            {
                var suppliers = await _supplierRepository.GetSuppliersByIdsAsync(ids);

                // Log results count
                Console.WriteLine($"[SupplierService] Found {suppliers.Count} suppliers.");

              return Ok(suppliers);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error in Supplier Microservice.",
                    error = ex.Message
                });
            }
        }

    }
}
