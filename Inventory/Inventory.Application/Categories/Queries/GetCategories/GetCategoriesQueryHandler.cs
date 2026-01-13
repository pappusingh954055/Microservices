using Inventory.Application.Categories.DTOs;
using Inventory.Application.Common.Interfaces;
using MediatR;

namespace Inventory.Application.Categories.Queries.GetCategories
{
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
            var list = await _repository.GetAllAsync();

            return list.Select(s => new CategoryDto
            {
                Id = s.Id,
                CategoryName = s.CategoryName,               
            }).ToList();
        }
    }
}
