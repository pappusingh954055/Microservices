using Inventory.Application.Common.Interfaces;
using Inventory.Application.Products.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetProductSearchHandler : IRequestHandler<GetProductSearchQuery, List<ProductSearchResponseDto>>
{
    private readonly IProductRepository _repository;
    private readonly IInventoryDbContext _context;

    public GetProductSearchHandler(IProductRepository repository, IInventoryDbContext context)
    {
        _repository = repository;
        _context = context;
    }

    public async Task<List<ProductSearchResponseDto>> Handle(GetProductSearchQuery request, CancellationToken cancellationToken)
    {
        // 1. Repository se Active Products fetch karein
        var products = await _repository.SearchActiveProductsAsync(request.Term);
        var productDtos = new List<ProductSearchResponseDto>();

        foreach (var p in products)
        {
            // 2. Real-time Calculation (Bypassing static '223' from DB)

            // Total Received (Jo Audit Trail mein '8' dikha raha hai)
            var totalInward = await _context.GRNDetails
                .Where(x => x.ProductId == p.Id)
                .SumAsync(x => x.ReceivedQty, cancellationToken);

            // Total Sold (Sales Outward)
            var totalSold = await _context.SaleOrderItems
                .Where(x => x.ProductId == p.Id)
                .SumAsync(x => x.Qty, cancellationToken);

            // Total Returned (Debit Note Outward) [cite: 2026-02-04]
            var totalReturned = await _context.PurchaseReturnItems
                .Where(x => x.ProductId == p.Id)
                .SumAsync(x => x.ReturnQty, cancellationToken);

            // 3. Current Available Stock Logic [cite: 2026-02-04]
            var calculatedStock = totalInward - (totalSold + totalReturned);

            productDtos.Add(new ProductSearchResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                IsActive = p.IsActive,
                BasePurchasePrice = p.BasePurchasePrice,
                unit = p.Unit,
                brand = p.Brand,
                sku = p.Sku,
                hsncode = p.HSNCode,
                mrp = p.MRP,
                SaleRate = p.SaleRate ?? 0,

                // Final Verified Sync with Audit Trail
                CurrentStock = calculatedStock > 0 ? (decimal)calculatedStock : 0
            });
        }

        return productDtos;
    }
}