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
    [Authorize(Roles = "Manager,Admin,User")]
    public async Task<IActionResult> ExportSaleOrderReport([FromBody] List<int> orderIds) // Guid se int mein badla
    {
        // 1. Validation check karein taaki 400 error handle ho sake
        if (orderIds == null || !orderIds.Any())
            return BadRequest("Kripya orders select karein.");

        // 2. Repository se selected integer IDs ka data fetch karein
        var data = await _saleRepo.GetSaleReportDataAsync(orderIds);

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Stock Report");

            // Header Row Setup
            var headers = new string[] { "Product Name", "Unit", "Received", "Rejected", "Available Stock" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
            }

            // Data Rows filling
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

                // Excel file return logic [cite: 2026-02-03]
                return File(content,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Stock_Report_{DateTime.Now:yyyyMMdd}.xlsx");
            }
        }
    }

    [HttpGet]
    [Authorize(Roles = "Manager,Admin,User")]
    public async Task<IActionResult> GetSaleOrders(
     [FromQuery] string searchTerm = "",
     [FromQuery] int pageNumber = 1,
     [FromQuery] int pageSize = 10,
     [FromQuery] string sortBy = "SODate",
     [FromQuery] string sortOrder = "desc")
    {
        // 1. Repository method call with parameters [cite: 2026-02-03]
        var (orders, totalCount) = await _saleRepo.GetAllSaleOrdersAsync(
            searchTerm,
            pageNumber,
            pageSize,
            sortBy,
            sortOrder
        );

        // 2. Return data along with total count for frontend pagination
        return Ok(new
        {
            data = orders,
            totalCount = totalCount
        });
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Manager,Admin,User")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] StatusUpdateDto request)
    {
        if (request == null || string.IsNullOrEmpty(request.Status))
            return BadRequest("Status data is missing.");

        var result = await _saleRepo.UpdateSaleOrderStatusAsync(id, request.Status);

        if (result)
        {
            return Ok(new { message = "Order Confirmed! Inventory has been updated." });
        }

        return BadRequest(new { message = "Status update is failed." });
    }

    // Ye DTO binding ke liye zaroori hai
    public class StatusUpdateDto
    {
        public string Status { get; set; } = null!;
    }

    // Simple DTO for binding
    public class UpdateStatusRequest
    {
        public string Status { get; set; } = null!;
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Manager,Admin,User")]
    public async Task<ActionResult<SaleOrderDetailDto>> GetOrder(int id)
    {
        var order = await _saleRepo.GetSaleOrderByIdAsync(id);

        if (order == null)
        {
            return NotFound(new { message = "Order nahi mila bhai!" });
        }

        return Ok(order);
    }

    [HttpGet("export-list")]
    [Authorize(Roles = "Manager,Admin,User")]
    public async Task<IActionResult> ExportSaleOrderList()
    {
        // Excel export ke liye hum pagination bypass karenge
        // Hum pageNumber 1 aur pageSize bahut bada (e.g. 1000000) bhejenge taaki sab mil jaye [cite: 2026-02-03]
        var (orders, totalCount) = await _saleRepo.GetAllSaleOrdersAsync(
            searchTerm: "",
            pageNumber: 1,
            pageSize: 1000000, // Saare records lene ke liye
            sortBy: "SODate",
            sortOrder: "desc"
        );

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Sale Orders");

            // Headers setup [cite: 2026-02-03]
            string[] headers = { "Order #", "Date", "Customer", "Amount", "Status" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#3f51b5"));
                cell.Style.Font.FontColor = XLColor.White;
            }

            // Data fill logic
            int row = 2;
            foreach (var order in orders)
            {
                worksheet.Cell(row, 1).Value = order.SoNumber;
                worksheet.Cell(row, 2).Value = order.SoDate.ToShortDateString();
                worksheet.Cell(row, 3).Value = order.CustomerName;
                worksheet.Cell(row, 4).Value = order.GrandTotal;
                worksheet.Cell(row, 5).Value = order.Status;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                // Excel file return logic [cite: 2026-02-03]
                return File(stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Sale_Orders.xlsx");
            }
        }
    }
}