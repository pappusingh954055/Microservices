using MediatR;
using Inventory.Application.Common.Interfaces;
using Inventory.Domain.Entities;

namespace Inventory.Application.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler
    : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = new Product(
            request.CategoryId,
            request.SubcategoryId,
            request.ProductCode,
            request.ProductName,
            request.Unit,
            request.DefaultGst,
            request.Description
        );

        await _repository.AddAsync(product);
        return product.Id;
    }
}
