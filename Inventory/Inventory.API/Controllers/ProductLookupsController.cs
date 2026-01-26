using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/products/lookups")]
public sealed class ProductLookupsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductLookupsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ✅ Page-load API
    [HttpGet]
    [Authorize(Roles = "Manager, Admin,User")]
    public async Task<IActionResult> GetLookups()
    {
        var result = await _mediator.Send(new GetProductLookupsQuery());
        return Ok(result);
    }
}
