using Inventory.Application.Clients;
using Inventory.Application.SaleOrders.SaleReturn.Command;
using Inventory.Application.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class SaleReturnController : ControllerBase
{
    private readonly ISaleReturnRepository _repo;
    private readonly ISaleReturnService _service;
    private readonly ICustomerClient _customerClient;
    private readonly IMediator _mediator;
    public SaleReturnController(
        ISaleReturnRepository repo, 
        ISaleReturnService saleReturnService,
        ICustomerClient customerClient,
        IMediator mediator
        ) 
    { 
    _repo = repo; 
    _service = saleReturnService;
        _customerClient = customerClient;
        _mediator = mediator;
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetSaleReturns(
        [FromQuery] string? search, // Yahan 'filter' ki jagah 'search' karein [cite: 2026-02-05]
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string sortField = "ReturnDate",
        [FromQuery] string sortOrder = "desc")
    {
        // Repository ko search pass karein [cite: 2026-02-05]
        var result = await _repo.GetSaleReturnsAsync(search, pageIndex, pageSize, fromDate, toDate, sortField, sortOrder);
        return Ok(result);
    }

    //[HttpPost("create")]
    //public async Task<IActionResult> CreateSaleReturn([FromBody] CreateSaleReturnDto dto)
    //{
    //    if (dto == null || !dto.Items.Any()) return BadRequest("Invalid Data");

    //    var result = await _service.SaveReturnAsync(dto);

    //    if (result) return Ok(new { message = "Sale Return Saved & Stock Updated (+)" });
    //    return BadRequest("Error saving return");
    //}
    //[HttpGet("customers-lookup")]
    //public async Task<IActionResult> GetCustomersLookup()
    //{
    //    var customers = await _customerClient.GetCustomersForLookupAsync();
    //    return Ok(customers);
    //}

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateSaleReturnDto dto)
    {
        var result = await _mediator.Send(new CreateSaleReturnCommand(dto));
        return result ? Ok(new { message = "Return Saved & Stock Updated" }) : BadRequest();
    }
}