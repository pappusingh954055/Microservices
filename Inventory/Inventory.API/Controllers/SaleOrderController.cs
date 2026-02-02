using ClosedXML.Excel;
using Inventory.Application.Common.Interfaces;
using Inventory.Application.SaleOrders.Commands;
using Inventory.Application.SaleOrders.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class SaleOrderController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ISaleOrderRepository _saleRepo;
    public SaleOrderController(IMediator mediator, 
        ISaleOrderRepository stockRepo) 
    {  
        _mediator = mediator;
        _saleRepo = stockRepo;
    }

    [HttpPost("save")]
    [Authorize(Roles = "Manager,Admin,User")]
    public async Task<IActionResult> Save([FromBody] CreateSaleOrderDto dto)
    {
        // 1. Mediator ab pura object return karega (Id aur SONumber)
        var result = await _mediator.Send(new CreateSaleOrderCommand(dto));

        // 2. Result ko as it is return karein taaki frontend ko result.soNumber mil sake
        return Ok(result);
    }

    [HttpPost("export")]
    public async Task<IActionResult> ExportStockReport([FromBody] List<Guid> productIds)
    {
        if (productIds == null || !productIds.Any())
            return BadRequest("Kripya products select karein.");

        var data = await _saleRepo.GetStockReportDataAsync(productIds);

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Stock Report");

            // Header Row
            var headers = new string[] { "Product Name", "Unit", "Received", "Rejected", "Available Stock" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
            }

            // Data Rows
            for (int i = 0; i < data.Count; i++)
            {
                worksheet.Cell(i + 2, 1).Value = data[i].ProductName;
                worksheet.Cell(i + 2, 2).Value = data[i].Unit;
                worksheet.Cell(i + 2, 3).Value = data[i].TotalReceived;
                worksheet.Cell(i + 2, 4).Value = data[i].TotalRejected;
                worksheet.Cell(i + 2, 5).Value = data[i].AvailableStock;
            }

            worksheet.Columns().AdjustToContents();

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();
                return File(content,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Stock_Report_{DateTime.Now:yyyyMMdd}.xlsx");
            }
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<SaleOrderListDto>>> GetSaleOrders()
    {
        var orders = await _saleRepo.GetAllSaleOrdersAsync();
        return Ok(orders);
    }
}