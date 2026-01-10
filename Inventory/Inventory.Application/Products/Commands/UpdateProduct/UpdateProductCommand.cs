using MediatR;

namespace Inventory.Application.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid Id,
    string Sku,
    string ProductName,
    string Unit,
    decimal? DefaultGst,
    string? Description,
    bool IsActive
) : IRequest;
