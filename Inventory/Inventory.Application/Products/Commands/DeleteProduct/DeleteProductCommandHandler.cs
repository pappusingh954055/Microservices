using MediatR;
using Inventory.Application.Common.Interfaces;

namespace Inventory.Application.Products.Commands.DeleteProduct;

public sealed class DeleteProductCommandHandler
    : IRequestHandler<DeleteProductCommand>
{
    private readonly IProductRepository _repository;

    public DeleteProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(
        DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException("Product not found");

        await _repository.DeleteAsync(product);
    }
}
