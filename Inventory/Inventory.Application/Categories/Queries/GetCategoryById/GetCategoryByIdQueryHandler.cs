using Inventory.Application.Categories.DTOs;
using MediatR;

namespace Inventory.Application.Categories.Queries.GetCategoryById;

public sealed class GetCategoryByIdQueryHandler
    : IRequestHandler<GetCategoryByIdQuery, CategoryDto?>
{
    private readonly ICategoryRepository _repository;

    public GetCategoryByIdQueryHandler(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<CategoryDto?> Handle(
        GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var c = await _repository.GetByIdAsync(request.Id);
        if (c is null) return null;

        return new CategoryDto
        {
            Id = c.Id,
            CategoryName = c.CategoryName,
            CategoryCode = c.CategoryCode,
            DefaultGst = c.DefaultGst,
            Description = c.Description,
            IsActive = c.IsActive
        };
    }
}
