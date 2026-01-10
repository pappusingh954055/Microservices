using MediatR;
using Inventory.Application.Common.Interfaces;

namespace Inventory.Application.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandler
    : IRequestHandler<UpdateProductCommand>
{
    private readonly IProductRepository _repository;

    public UpdateProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException("Product not found");

        product.Update(
            request.Sku,
            request.ProductName,
            request.Unit,
            request.DefaultGst,
            request.Description,
            request.IsActive
        );

        await _repository.UpdateAsync(product);
    }
}
