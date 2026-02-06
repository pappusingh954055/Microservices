using ClosedXML.Excel;
using Inventory.Application.Common.Interfaces;
using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Services;

public class SaleReturnService : ISaleReturnService
{
    private readonly ISaleReturnRepository _repository;
    private readonly IInventoryDbContext _context;
   
    private readonly ICustomerHttpService _customerHttpService;

    public SaleReturnService(
        ISaleReturnRepository repository,
        IInventoryDbContext context,
        ICustomerHttpService httpService
       

        )
    {
        _repository = repository;
        _context = context;
        _customerHttpService = httpService;
       
    }

    public async Task<SaleReturnPagedResponse> GetSaleReturnListAsync(string? search, int pageIndex, int pageSize, DateTime? fromDate, DateTime? toDate, string sortField, string sortOrder)
    {
        // 1. Inventory Database se returns lo (Paging ke saath) [cite: 2026-02-05]
        var response = await _repository.GetSaleReturnsAsync(search, pageIndex, pageSize, fromDate, toDate, sortField, sortOrder);

        if (response.Items != null && response.Items.Any())
        {
            // 2. Sirf unique Customer IDs nikalien jo is page par dikh rahe hain [cite: 2026-02-05]
            var uniqueIds = response.Items.Select(x => x.CustomerId).Distinct().ToList();

            try
            {
                // 3. Batch call to Customer Microservice [cite: 2026-02-05]
                //var customerMap = await _customerClient.GetCustomerNamesAsync(uniqueIds);

                var customerMap = await _customerHttpService.GetCustomerNamesAsync(uniqueIds);

                // 4. Map names back to our DTO [cite: 2026-02-05]
                foreach (var item in response.Items)
                {
                    if (customerMap.TryGetValue(item.CustomerId, out var name))
                    {
                        item.CustomerName = name; // UI par name dikhane ke liye
                    }
                    else
                    {
                        item.CustomerName = "Unknown Customer";
                    }
                }
            }
            catch (Exception)
            {
                // Agar Customer Service down hai, toh default text dikhao taaki page load ho jaye
                response.Items.ForEach(x => x.CustomerName = "Name Unavailable");
            }
        }

        return response;
    }

    public async Task<bool> SaveReturnAsync(CreateSaleReturnDto dto)
    {
        // Item level calculations pehle kar lete hain
        var returnItems = dto.Items.Where(i => i.ReturnQty > 0).Select(i => {
            var subTotal = i.ReturnQty * i.UnitPrice;
            var taxAmount = subTotal * (i.TaxPercentage / 100);

            return new SaleReturnItem
            {
                ProductId = i.ProductId,
                ReturnQty = i.ReturnQty,
                UnitPrice = i.UnitPrice,
                TaxPercentage = i.TaxPercentage,
                TaxAmount = taxAmount,   
                TotalAmount = subTotal + taxAmount, 
                Reason = i.Reason,
                ItemCondition = i.ItemCondition
            };
        }).ToList();

        var entity = new SaleReturnHeader
        {
            ReturnNumber = "SR-" + DateTime.Now.ToString("yyyyMMddHHmm"),
            ReturnDate = dto.ReturnDate,
            SaleOrderId = dto.SaleOrderId,
            CustomerId = dto.CustomerId,
            Remarks = dto.Remarks,
            Status = "Confirmed",

            // Header Level Calculations
            SubTotal = returnItems.Sum(x => x.ReturnQty * x.UnitPrice),
            TaxAmount = returnItems.Sum(x => x.TaxAmount),
            TotalAmount = returnItems.Sum(x => x.TotalAmount), // Final Amount

            ReturnItems = returnItems
        };

        return await _repository.CreateSaleReturnAsync(entity);
    }

    public async Task<CreditNotePrintDto?> GetPrintDataAsync(int id)
    {
        // 1. Database se core data fetch karein (SaleOrders ke saath Join karke)
        var data = await _context.SaleReturnHeaders
            .AsNoTracking()
            .Include(h => h.ReturnItems)
            .ThenInclude(i => i.Product)
            .Where(h => h.SaleReturnHeaderId == id)
            .Select(h => new CreditNotePrintDto
            {
                ReturnNumber = h.ReturnNumber,
                ReturnDate = h.ReturnDate,
                CustomerId = h.CustomerId,

                // Fix: SONumber asali table 'SaleOrders' se fetch ho raha hai
                SONumber = _context.SaleOrders
                            .Where(so => so.Id == h.SaleOrderId)
                            .Select(so => so.SONumber)
                            .FirstOrDefault() ?? "N/A",

                SubTotal = h.SubTotal,     // Database mapping
                TotalTax = h.TaxAmount,    // Database mapping
                GrandTotal = h.TotalAmount, // Database mapping

                Items = h.ReturnItems.Select(i => new ReturnItemPrintDto
                {
                    ProductName = i.Product.Name,
                    Qty = i.ReturnQty,
                    Rate = i.UnitPrice,
                    TaxPercent = i.TaxPercentage,
                    Total = i.TotalAmount
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (data == null) return null;

        // 2. Customer Name laane ke liye Helper Method (Dictionary Logic)
        var customerIds = new List<int> { data.CustomerId };
        var customerNames = await _customerHttpService.GetCustomerNamesAsync(customerIds);

        if (customerNames != null && customerNames.ContainsKey(data.CustomerId))
        {
            data.CustomerName = customerNames[data.CustomerId];
        }
        else
        {
            data.CustomerName = "Unknown Customer";
        }

        return data;
    }



    public async Task<byte[]> GenerateExcelExportAsync(DateTime? fromDate, DateTime? toDate)
    {
        var data = await _repository.GetExportDataAsync(fromDate, toDate);

        // Microservice se names laao [cite: 2026-02-06]
        var customerIds = data.Select(x => int.Parse(x.CustomerName)).Distinct().ToList();
        var customerNames = await _customerHttpService.GetCustomerNamesAsync(customerIds);

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Sale Returns");

            // Header Row
            worksheet.Cell(1, 1).Value = "Return No.";
            worksheet.Cell(1, 2).Value = "Date";
            worksheet.Cell(1, 3).Value = "Customer";
            worksheet.Cell(1, 4).Value = "SO Ref";
            worksheet.Cell(1, 5).Value = "Total";
            worksheet.Cell(1, 6).Value = "Status";

            int row = 2;
            foreach (var item in data)
            {
                int cId = int.Parse(item.CustomerName);
                worksheet.Cell(row, 1).Value = item.ReturnNumber;
                worksheet.Cell(row, 2).Value = item.ReturnDate;
                // Name replace karein [cite: 2026-02-06]
                worksheet.Cell(row, 3).Value = customerNames.GetValueOrDefault(cId, "Unknown");
                worksheet.Cell(row, 4).Value = item.SONumber;
                worksheet.Cell(row, 5).Value = item.TotalAmount;
                worksheet.Cell(row, 6).Value = item.Status;
                row++;
            }

            worksheet.Columns().AdjustToContents();
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
        }
    }
}