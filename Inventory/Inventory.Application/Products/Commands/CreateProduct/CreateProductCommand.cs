using MediatR;

namespace Inventory.Application.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(
    Guid CategoryId,
    Guid SubcategoryId,
    string ProductCode,
    string ProductName,
    string Unit,
    decimal? DefaultGst,
    string? Description
) : IRequest<Guid>;
