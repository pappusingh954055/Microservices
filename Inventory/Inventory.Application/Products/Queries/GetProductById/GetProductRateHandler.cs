// Products.Application/Handlers/GetProductRateHandler.cs
using Inventory.Application.Common.Interfaces;
using Inventory.Application.Products.DTOs;
using Inventory.Application.Products.Queries.GetProductById;
using MediatR;

public class GetProductRateHandler : IRequestHandler<GetProductRateQuery, ProductRateDto>
{
    private readonly IProductRepository _repository;

    public GetProductRateHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductRateDto> Handle(GetProductRateQuery request, CancellationToken ct)
    {
        var rate = await _repository.GetProductRateAsync(request.ProductId, request.PriceListId);

        // Yahan request ki IDs ko wapas assign karein taaki response mein 000-000 na aaye
        return new ProductRateDto(request.ProductId, request.PriceListId, rate);
    }
}