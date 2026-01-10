using MediatR;

namespace Inventory.Application.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand(
    Guid Id,
    string CategoryName,
    string CategoryCode,
    decimal DefaultGst,
    string? Description,
    bool IsActive
) : IRequest<Guid>;
