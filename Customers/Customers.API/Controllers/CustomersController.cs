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
[Route("api/customers")]
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
    [Authorize(Roles = "Admin,Manager,User,Warehouse")]
    public async Task<IActionResult> Create(CreateCustomerDto dto)
    {
        var id = await _mediator.Send(
            new CreateCustomerCommand(dto));

        return Ok(id);
    }

    [HttpPost("paged")]
    [Authorize(Roles = "Admin,Manager,User,Warehouse")]
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
    [Authorize(Roles = "Admin,Manager,User,Warehouse")]
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
    [Authorize(Roles = "Admin,Manager,User,Warehouse")]
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
    [Authorize(Roles = "Admin,Manager,User,Warehouse")]
    public async Task<IActionResult> GetCustomerLookup()
    {
        // Repository se data mangwana [cite: 2026-02-05]
        var customers = await _customerRepo.GetCustomersLookupAsync();
        return Ok(customers);
    }

    [HttpGet("search-ids")]
    [Authorize(Roles = "Admin,Manager,User,Warehouse")]
    public async Task<ActionResult<List<int>>> SearchIdsByName([FromQuery] string name)
    {
        var ids = await _customerRepo.GetIdsByNameAsync(name);
        return Ok(ids);
    }
}
