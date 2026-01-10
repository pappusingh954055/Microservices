using MediatR;
using Inventory.Application.Common.Interfaces;
using Inventory.Domain.Entities;

namespace Inventory.Application.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler
    : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _repository;
    private readonly IInventoryDbContext _context;
    public CreateProductCommandHandler(IProductRepository repository, 
        IInventoryDbContext context)
    {
        _repository = repository;
        _context = context;
    }

    public async Task<Guid> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = new Product(
            request.CategoryId,
            request.SubcategoryId,
            request.Sku,
            request.ProductName,
            request.Unit,
            request.DefaultGst,
            request.Description
        );

        await _repository.AddAsync(product);

        await _context.SaveChangesAsync(cancellationToken);  

        return product.Id;
    }
}
