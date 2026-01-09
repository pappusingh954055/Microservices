using MediatR;

using Inventory.Application.Categories.DTOs;

namespace Inventory.Application.Categories.Queries.GetCategories;

public sealed class GetCategoriesQueryHandler
    : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly ICategoryRepository _repository;

    public GetCategoriesQueryHandler(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<CategoryDto>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await _repository.GetAllAsync();

        return categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            CategoryName = c.Name,
            CategoryCode = c.Code,
            DefaultGst = c.DefaultGst,
            Description = c.Description,
            IsActive = c.IsActive
        }).ToList();
    }
}
