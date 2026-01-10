using MediatR;

namespace Inventory.Application.Subcategories.Commands.CreateSubcategory;

public sealed record CreateSubcategoryCommand(
    Guid CategoryId,
    string SubcategoryCode,
    string SubcategoryName,
    decimal DefaultGst,
    string? Description
) : IRequest<Guid>;
