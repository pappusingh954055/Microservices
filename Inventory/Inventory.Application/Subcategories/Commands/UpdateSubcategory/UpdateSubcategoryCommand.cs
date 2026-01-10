using MediatR;

namespace Inventory.Application.Subcategories.Commands.UpdateSubcategory;

public sealed record UpdateSubcategoryCommand(
    Guid Id,
    string SubcategoryCode,
    string SubcategoryName,
    decimal DefaultGst,
    string? Description,
    bool IsActive
) : IRequest;
