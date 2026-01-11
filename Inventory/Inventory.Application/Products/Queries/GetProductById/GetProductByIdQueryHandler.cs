using Inventory.Application.Common.Interfaces;
using MediatR;

namespace Inventory.Application.Products.Queries.GetProductById;

public sealed class GetProductByIdQueryHandler
    : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IProductRepository _repository;

    public GetProductByIdQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductDto?> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var p = await _repository.GetByIdAsync(request.Id);
        if (p is null) return null;

        return new ProductDto
        {
            Id = p.Id,
            CategoryId = p.CategoryId,
            SubcategoryId = p.SubcategoryId,
            Sku = p.Sku,
            ProductName = p.Name,
            Unit = p.Unit,  
            HsnCode=p.HSNCode,
            MinStock=p.MinStock,
            DefaultGst = p.DefaultGst,
            Description = p.Description,
            TrackInventory = p.TrackInventory
        };
    }
}
