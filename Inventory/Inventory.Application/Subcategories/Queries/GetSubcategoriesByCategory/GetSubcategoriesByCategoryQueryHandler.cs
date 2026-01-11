using Inventory.Application.Common.Interfaces;
using Inventory.Application.Subcategories.DTOs;
using MediatR;

internal sealed class GetSubcategoriesByCategoryQueryHandler
    : IRequestHandler<GetSubcategoriesByCategoryQuery, List<SubcategoryDto>>
{
    private readonly ISubcategoryRepository _repository;

    public GetSubcategoriesByCategoryQueryHandler(
        ISubcategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<SubcategoryDto>> Handle(
        GetSubcategoriesByCategoryQuery request,
        CancellationToken cancellationToken)
    {
        var subcategories = await _repository.GetByCategoryIdAsync(request.CategoryId);

        return subcategories.Select(s => new SubcategoryDto
        {
            Id = s.Id,
            CategoryId = s.CategoryId,
            CategoryName = s.Category.Name,
            SubcategoryName = s.Name,
            DefaultGst = s.DefaultGst,
            IsActive = s.IsActive
        }).ToList();
    }
}
