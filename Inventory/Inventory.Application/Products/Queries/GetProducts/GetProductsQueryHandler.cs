using MediatR;
using Inventory.Application.Common.Interfaces;
using Inventory.Application.Products.DTOs;

namespace Inventory.Application.Products.Queries.GetProducts;

public sealed class GetProductsQueryHandler
    : IRequestHandler<GetProductsQuery, List<ProductDto>>
{
    private readonly IProductRepository _repository;

    public GetProductsQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ProductDto>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var list = await _repository.GetAllAsync();

        return list.Select(p => new ProductDto
        {
            Id = p.Id,
            CategoryId = p.CategoryId,
            SubcategoryId = p.SubcategoryId,
            ProductCode = p.Code,
            ProductName = p.Name,
            Unit = p.Unit,
            DefaultGst = p.DefaultGst,
            Description = p.Description,
            IsActive = p.IsActive
        }).ToList();
    }
}
