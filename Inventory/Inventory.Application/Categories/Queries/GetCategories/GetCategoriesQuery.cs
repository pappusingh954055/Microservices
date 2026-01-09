using MediatR;
using Inventory.Application.Categories.DTOs;

namespace Inventory.Application.Categories.Queries.GetCategories;

public sealed record GetCategoriesQuery
    : IRequest<List<CategoryDto>>;
