using Inventory.Application.Clients;
using Inventory.Application.SaleOrders.DTOs;
using Inventory.Application.SaleOrders.SaleReturn.Command;
using Inventory.Application.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class SaleReturnController : ControllerBase
{
    private readonly ISaleReturnRepository _repo;
    private readonly ISaleReturnService _service;
    private readonly ICustomerClient _customerClient;
    private readonly IMediator _mediator;
    private readonly IPdfService _pdfService;

    private readonly ICustomerHttpService _customerHttpService;
    public SaleReturnController(
        ISaleReturnRepository repo, 
        ISaleReturnService saleReturnService,
        ICustomerClient customerClient,
        IMediator mediator,
        IPdfService pdfService,
        ICustomerHttpService customerHttpService

        ) 
    { 
    _repo = repo; 
    _service = saleReturnService;
        _customerClient = customerClient;
        _mediator = mediator;
        _pdfService = pdfService;
        _customerHttpService = customerHttpService;
    }

    [HttpGet("list")]
    [Authorize(Roles = "Admin, User, Manager, Employee, Warehouse")]
    public async Task<IActionResult> GetSaleReturns(
    [FromQuery] string? search,
    [FromQuery] string? status, // Naya Parameter
    [FromQuery] int pageIndex = 0,
    [FromQuery] int pageSize = 10,
    [FromQuery] DateTime? fromDate = null,
    [FromQuery] DateTime? toDate = null,
    [FromQuery] string sortField = "ReturnDate",
    [FromQuery] string sortOrder = "desc")
    {
        var result = await _repo.GetSaleReturnsAsync(search, status, pageIndex, pageSize, fromDate, toDate, sortField, sortOrder);
        return Ok(result);
    }



    [HttpPost("create")]
    [Authorize(Roles = "Admin, User, Manager, Employee, Warehouse")]
    public async Task<IActionResult> Create([FromBody] CreateSaleReturnDto dto)
    {
        var result = await _mediator.Send(new CreateSaleReturnCommand(dto));
        return result ? Ok(new { message = "Return Saved & Stock Updated" }) : BadRequest();
    }

    [HttpGet("print/{id}")]
    [Authorize(Roles = "Admin, User, Manager, Employee, Warehouse")]
    public async Task<IActionResult> Print(int id)
    {
        var printData = await _service.GetPrintDataAsync(id);

        if (printData == null) return NotFound("Credit Note not found");

        // HTML Template generate karke PDF mein convert karenge
        var html = ConvertThtmlToPdf.GenerateHtmlTemplate(printData);
        var pdf = _pdfService.Convert(html);

        return File(pdf, "application/pdf", $"CreditNote_{printData.ReturnNumber}.pdf");
    }

    [HttpGet("print-data/{id}")]
    [Authorize(Roles = "Admin, User, Manager, Employee, Warehouse")]
    public async Task<IActionResult> GetPrintData(int id)
    {
        var data = await _service.GetPrintDataAsync(id);

        if (data == null)
        {
            return NotFound("Bhai, data nahi mila!");
        }
        return Ok(data);
    }

    [HttpGet("export-excel")]
    [Authorize(Roles = "Admin, User, Manager, Employee, Warehouse")]
    public async Task<IActionResult> ExportExcel([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var content = await _service.GenerateExcelExportAsync(fromDate, toDate);
        var fileName = $"SaleReturns_{DateTime.Now:yyyyMMdd}.xlsx";

        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpGet("summary")]
    [Authorize(Roles = "Admin, User, Manager, Employee, Warehouse")]
    public async Task<ActionResult<SaleReturnSummaryDto>> GetSummary()
    {
        var summary = await _repo.GetDashboardSummaryAsync();
        return Ok(summary);
    }
}