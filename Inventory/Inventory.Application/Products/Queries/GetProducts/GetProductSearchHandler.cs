using Inventory.Application.Common.Interfaces;
using Inventory.Application.Products.DTOs;
using Inventory.Application.Products.Queries.GetProducts;
using MediatR;

public class GetProductSearchHandler : IRequestHandler<GetProductSearchQuery, List<ProductSearchResponseDto>>
{
    private readonly IProductRepository _repository;

    public GetProductSearchHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ProductSearchResponseDto>> Handle(GetProductSearchQuery request, CancellationToken cancellationToken)
    {
        // Repository se domain objects fetch karein
        var products = await _repository.SearchActiveProductsAsync(request.Term);

        // Domain objects ko DTO mein map karein
        return products.Select(p => new ProductSearchResponseDto
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
            SaleRate = p.SaleRate??0,
            CurrentStock = p.CurrentStock
        }).ToList();
    }
}