using Customers.Application.Common.Interfaces;
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
    [Authorize(Roles = "Manager,Admin, User")]
    public async Task<IActionResult> Create(CreateCustomerDto dto)
    {
        var id = await _mediator.Send(
            new CreateCustomerCommand(dto));

        return Ok(id);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(
            new GetCustomersQuery());

        return Ok(result);
    }

    [HttpPost("get-names")]
    public async Task<IActionResult> GetCustomerNames([FromBody] List<int> ids)
    {
        if (ids == null || !ids.Any()) return BadRequest("No IDs provided");

        var names = await _customerRepo.GetCustomerNamesByIdsAsync(ids);
        return Ok(names);
    }
}
