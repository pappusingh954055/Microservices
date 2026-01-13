using Inventory.Application.Common.Interfaces;
using Inventory.Application.Common.Models;
using Inventory.Application.Subcategories.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Subcategories.Queries.Searching
{
    internal sealed class GetSubcategoriesPagedHandler
     : IRequestHandler<GetSubcategoriesPagedQuery, GridResponse<SubcategoryDto>>
    {
        private readonly ISubcategoryRepository _repository;

        public GetSubcategoriesPagedHandler(ISubcategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<GridResponse<SubcategoryDto>> Handle(
            GetSubcategoriesPagedQuery request,
            CancellationToken cancellationToken)
        {
            var query = _repository.Query();

            // 🔍 SEARCH
            if (!string.IsNullOrWhiteSpace(request.Query.Search))
            {
                query = query.Where(x =>
                    x.SubcategoryName.Contains(request.Query.Search));
            }

            // 🔃 SORTING
            query = request.Query.SortBy switch
            {
                "subcategoryName" =>
                    request.Query.SortDirection == "asc"
                        ? query.OrderBy(x => x.SubcategoryName)
                        : query.OrderByDescending(x => x.SubcategoryName),

                "subcategoryCode" =>
                    request.Query.SortDirection == "asc"
                        ? query.OrderBy(x => x.SubcategoryCode)
                        : query.OrderByDescending(x => x.SubcategoryCode),

                _ => query.OrderByDescending(x => x.CreatedOn),

            
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((request.Query.PageNumber - 1) * request.Query.PageSize)
                .Take(request.Query.PageSize)
                .Select(x => new SubcategoryDto
                {
                    Id = x.Id,
                    CategoryName=x.Category.CategoryName,
                    SubcategoryName = x.SubcategoryName,
                    SubcategoryCode = x.SubcategoryCode,
                    CreatedOn = x.CreatedOn,
                    CreatedBy = x.CreatedBy,
                    DefaultGst = x.DefaultGst,
                    IsActive = x.IsActive
                })
                .ToListAsync(cancellationToken);

            return new GridResponse<SubcategoryDto>(items, totalCount);
        }
    }
}