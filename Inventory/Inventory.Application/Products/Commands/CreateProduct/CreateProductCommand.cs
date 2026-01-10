using MediatR;

public sealed record CreateProductCommand(
    Guid CategoryId,
    Guid SubcategoryId,
    string Sku,
    string ProductName,
    string Unit,
    decimal? DefaultGst,
    string? Description
) : IRequest<Guid>;
