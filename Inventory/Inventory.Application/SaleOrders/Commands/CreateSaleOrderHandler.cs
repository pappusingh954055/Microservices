// Note: Command definition mein bhi return type 'int' se badal kar 'object' ya custom class karein
using Inventory.Application.Common.Interfaces;
using Inventory.Application.SaleOrders.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;
using YourProjectNamespace.Entities;

public class CreateSaleOrderHandler : IRequestHandler<CreateSaleOrderCommand, object> // Change int to object
{
    private readonly ISaleOrderRepository _repo;
    private readonly IInventoryDbContext _context;

    public CreateSaleOrderHandler(ISaleOrderRepository repo, IInventoryDbContext context)
    {
        _repo = repo;
        _context = context;
    }

    public async Task<object> Handle(CreateSaleOrderCommand request, CancellationToken cancellationToken)
    {
        var dto = request.OrderDto;

        // 1. SONumber Generate Karein
        string lastNo = await _repo.GetLastSONumberAsync();
        int nextId = lastNo == null ? 1 : int.Parse(lastNo.Split('-').Last()) + 1;
        string generatedSONo = $"SO-{DateTime.Now.Year}-{nextId:D4}";

        // 2. SaleOrder Object Mapping
        var saleOrder = new SaleOrder
        {
            SONumber = generatedSONo,
            CustomerId = dto.CustomerId,
            SODate = dto.SoDate,
            ExpectedDeliveryDate = dto.ExpectedDeliveryDate,
            SubTotal = dto.SubTotal,
            TotalTax = dto.TotalTax,
            GrandTotal = dto.GrandTotal,
            Remarks = dto.Remarks,
            Status = dto.Status,
            CreatedBy = dto.CreatedBy,
            Items = dto.Items.Select(i => new SaleOrderItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Qty = i.Qty,
                Unit = i.Unit,
                Rate = i.Rate,
                DiscountPercent = i.DiscountPercent,
                GSTPercent = i.GstPercent,
                TaxAmount = i.TaxAmount,
                Total = i.Total
            }).ToList()
        };

        // 3. Conditional Logic: Confirm & Reduce Stock vs Save as Draft
        if (dto.Status == "Confirmed")
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await _repo.BeginTransactionAsync();
                try
                {
                    var savedId = await _repo.SaveAsync(saleOrder);

                    foreach (var item in saleOrder.Items)
                    {
                        decimal availableStock = await _repo.GetAvailableStockAsync(item.ProductId);
                        if (availableStock < item.Qty)
                        {
                            throw new Exception($"Insufficient stock for {item.ProductName}. Available: {availableStock}");
                        }
                        await _repo.UpdateProductStockAsync(item.ProductId, -item.Qty);
                    }

                    await _repo.CommitTransactionAsync();

                    // ✅ YAHAN FIX HAI: ID ke saath SONumber bhi return karein
                    return new { Id = savedId, SONumber = generatedSONo };
                }
                catch (Exception)
                {
                    await _repo.RollbackTransactionAsync();
                    throw;
                }
            });
        }
        else
        {
            var savedId = await _repo.SaveAsync(saleOrder);
            // ✅ YAHAN FIX HAI: Draft ke liye bhi dono return karein
            return new { Id = savedId, SONumber = generatedSONo };
        }
    }
}