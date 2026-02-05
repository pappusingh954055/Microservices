using Inventory.Application.Clients;
using Inventory.Domain.Entities;

namespace Inventory.Application.Services;

public class SaleReturnService : ISaleReturnService
{
    private readonly ISaleReturnRepository _repository;
    private readonly ICustomerClient _customerClient;

    public SaleReturnService(ISaleReturnRepository repository, ICustomerClient customerClient)
    {
        _repository = repository;
        _customerClient = customerClient;
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
                var customerMap = await _customerClient.GetCustomerNamesAsync(uniqueIds);

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
        var entity = new SaleReturnHeader
        {
            // Auto-generate Return Number (Example logic)
            ReturnNumber = "SR-" + DateTime.Now.ToString("yyyyMMddHHmm"),
            ReturnDate = dto.ReturnDate,
            SaleOrderId = dto.SaleOrderId,
            CustomerId = dto.CustomerId,
            Remarks = dto.Remarks,
            Status = "Confirmed", //
            TotalAmount = dto.Items.Sum(x => x.ReturnQty * x.UnitPrice),
            ReturnItems = dto.Items.Select(i => new SaleReturnItem
            {
                ProductId = i.ProductId,
                ReturnQty = i.ReturnQty,
                UnitPrice = i.UnitPrice,
                TaxPercentage = i.TaxPercentage,
                Reason = i.Reason,
                ItemCondition = i.ItemCondition
            }).ToList()
        };

        return await _repository.CreateSaleReturnAsync(entity);
    }
}