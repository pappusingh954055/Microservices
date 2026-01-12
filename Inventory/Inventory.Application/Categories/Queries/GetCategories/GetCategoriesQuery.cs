using Inventory.Application.Categories.DTOs;
using Inventory.Application.Common.Models;
using MediatR;

public sealed record GetCategoriesQuery(PagedQuery Query)
    : IRequest<PagedResult<CategoryDto>>;
