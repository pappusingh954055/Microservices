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
        // 1. Optimized Search using AsNoTracking to improve performance
        // Hum base query ko AsNoTracking ke saath handle kar rahe hain
        var products = await _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive &&
                        (p.Name.Contains(request.Term) || p.Sku.Contains(request.Term)))
            .ToListAsync(cancellationToken);

        var productDtos = new List<ProductSearchResponseDto>();

        foreach (var p in products)
        {
            // 🆕 STEP 2: Fetch Discount from PriceListItems
            // Ye PriceList se discount uthayega, aur agar nahi hai toh 0 rakhega
            var discountPercent = await _context.PriceListItems
                .AsNoTracking()
                .Where(di => di.ProductId == p.Id)
                .Select(di => di.DiscountPercent)
                .FirstOrDefaultAsync(cancellationToken);

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

                // 🆕 GST Product Master se aur Discount PriceList se
                GstPercent = p.DefaultGst ?? 0,
                DiscountPercent = discountPercent,

                // STEP 3: DIRECT BINDING WITH DATABASE COLUMN
                CurrentStock = p.CurrentStock > 0 ? (decimal)p.CurrentStock : 0
            });
        }

        return productDtos;
    }
}