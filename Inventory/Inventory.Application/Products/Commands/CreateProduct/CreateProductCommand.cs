using MediatR;

public sealed record CreateProductCommand(
    Guid categoryid,
    Guid subcategoryid,
    string sku,
    string productname,
    string unit,
    decimal? defaultgst,
    string hsncode,
    int minstock,
    bool trackinventory,
    string? description
) : IRequest<Guid>;
