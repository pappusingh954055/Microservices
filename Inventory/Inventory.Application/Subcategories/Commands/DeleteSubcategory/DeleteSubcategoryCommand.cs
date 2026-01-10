using MediatR;

namespace Inventory.Application.Subcategories.Commands.DeleteSubcategory;

public sealed record DeleteSubcategoryCommand(Guid Id) : IRequest;
