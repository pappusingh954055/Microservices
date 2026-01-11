using MediatR;

namespace Inventory.Application.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string Sku,
    Guid CategoryId,
    Guid SubCategoryId,
    string Unit,
    string hsncode,
    int minstock,
    decimal DefaultPrice,
    decimal DefaultGst,
    string Description,
    bool TrackInventory
) : IRequest<Guid>;
