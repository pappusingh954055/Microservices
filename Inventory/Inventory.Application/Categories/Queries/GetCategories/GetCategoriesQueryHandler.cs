using Inventory.Application.Categories.DTOs;
using Inventory.Application.Common.Interfaces;
using Inventory.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

internal sealed class GetCategoriesQueryHandler
    : IRequestHandler<GetCategoriesQuery, PagedResult<CategoryDto>>
{
    private readonly ICategoryRepository _repository;

    public GetCategoriesQueryHandler(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<CategoryDto>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _repository.Query();

        // 🔍 SEARCH
        if (!string.IsNullOrWhiteSpace(request.Query.Search))
        {
            query = query.Where(x =>
                x.CategoryName.Contains(request.Query.Search));
        }

        // 🔃 SORTING
        query = request.Query.SortBy switch
        {
            "categoryName" =>
                request.Query.SortDirection == "asc"
                    ? query.OrderBy(x => x.CategoryName)
                    : query.OrderByDescending(x => x.CategoryName),

            "categoryCode" =>
                request.Query.SortDirection == "asc"
                    ? query.OrderBy(x => x.CategoryCode)
                    : query.OrderByDescending(x => x.CategoryCode),

            _ => query.OrderByDescending(x => x.CreatedOn)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Query.PageNumber - 1) * request.Query.PageSize)
            .Take(request.Query.PageSize)
            .Select(x => new CategoryDto
            {
                Id = x.Id,
                CategoryName = x.CategoryName,
                CategoryCode = x.CategoryCode,
                DefaultGst = x.DefaultGst,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<CategoryDto>
        {
            TotalCount = totalCount,
            Items = items
        };
    }
}
