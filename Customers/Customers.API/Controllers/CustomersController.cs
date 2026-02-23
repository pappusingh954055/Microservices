using Customers.Application.Common.Interfaces;
using Customers.Application.Common.Models;
using Customers.Application.DTOs;
using Customers.Application.Features.Commands;
using Customers.Application.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Customers.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICustomerRepository _customerRepo;

    public CustomersController(IMediator mediator, ICustomerRepository customerRepo)
    {
        _mediator = mediator;
        _customerRepo = customerRepo;
    }

    [HttpPost]
    [Authorize(Roles = "Admin, User, Manager, Employee, Warehouse")]
    public async Task<IActionResult> Create(CreateCustomerDto dto)
    {
        var id = await _mediator.Send(
            new CreateCustomerCommand(dto));

        return Ok(id);
    }

    [HttpPost("paged")]
    [Authorize(Roles = "Admin, User, Manager, Employee, Warehouse")]
    public async Task<IActionResult> GetCustomers([FromBody] GridRequest request)
    {
        var result = await _mediator.Send(new GetCustomersPagedQuery(request));
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(
            new GetCustomersQuery());

        return Ok(result);
    }

    /// <summary>
    /// bilk
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [HttpPost("get-names")]
    public async Task<IActionResult> GetCustomerNames([FromBody] List<int> ids)
    {
        if (ids == null || !ids.Any()) return BadRequest("No IDs provided");

        var names = await _customerRepo.GetCustomerNamesByIdsAsync(ids);
        return Ok(names);
    }

    /// <summary>
    /// Single
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/name")]
    public async Task<IActionResult> GetCustomerNameById(int id)
    {
        var name = await _customerRepo.GetCustomerNameByIdAsync(id);

        if (string.IsNullOrEmpty(name))
        {
            return NotFound("Customer not found");
        }

        // Plain string return karein taaki Inventory API ka ReadAsStringAsync() ise handle kar sake
        return Ok(name);
    }

    [HttpGet("lookup")]
    [Authorize(Roles = "Admin, User, Manager, Employee, Warehouse")]
    public async Task<IActionResult> GetCustomerLookup()
    {
        // Repository se data mangwana [cite: 2026-02-05]
        var customers = await _customerRepo.GetCustomersLookupAsync();
        return Ok(customers);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin, User, Manager, Employee, Warehouse")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetCustomerByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin, User, Manager, Employee, Warehouse")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateCustomerDto dto)
    {
        var result = await _mediator.Send(new UpdateCustomerCommand(id, dto));
        if (!result) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin, User, Manager, Employee, Warehouse")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteCustomerCommand(id));
        if (!result) return NotFound();
        return Ok(result);
    }

    [HttpGet("search-ids")]
    [Authorize(Roles = "Admin, User, Manager, Employee, Warehouse")]
    public async Task<ActionResult<List<int>>> SearchIdsByName([FromQuery] string name)
    {
        var ids = await _customerRepo.GetIdsByNameAsync(name);
        return Ok(ids);
    }
}
