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
            // Base query + Category join (required for categoryName filter & sort)
            var query = _repository
                .Query()
                .Include(x => x.Category)
                .AsQueryable();

            // ============================
            // 🔍 GLOBAL SEARCH
            // ============================
            if (!string.IsNullOrWhiteSpace(request.Query.Search))
            {
                var search = request.Query.Search.ToLower();

                query = query.Where(x =>
                    x.SubcategoryName.ToLower().Contains(search) ||
                    x.SubcategoryCode.ToLower().Contains(search) ||
                    x.Category.CategoryName.ToLower().Contains(search));
            }

            // ============================
            // 🎯 COLUMN FILTERS (FIX)
            // ============================
            // 🎯 COLUMN FILTERS
            if (request.Query.Filters != null && request.Query.Filters.Any())
            {
                foreach (var filter in request.Query.Filters)
                {
                    var value = filter.Value?.ToLower();

                    if (string.IsNullOrWhiteSpace(value))
                        continue;

                    query = filter.Key switch
                    {
                        "subcategoryName" =>
                            query.Where(x => x.SubcategoryName.ToLower().Contains(value)),

                        "subcategoryCode" =>
                            query.Where(x => x.SubcategoryCode.ToLower().Contains(value)),

                        "categoryName" =>
                            query.Where(x => x.Category.CategoryName.ToLower().Contains(value)),

                        "defaultGst" =>
                            query.Where(x => x.DefaultGst.ToString().Contains(value)),

                        "isActive" =>
                            query.Where(x => x.IsActive == (value == "true" || value == "yes")),

                        _ => query
                    };
                }
            }


            // ============================
            // 🔃 SORTING
            // ============================
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

                "categoryName" =>
                    request.Query.SortDirection == "asc"
                        ? query.OrderBy(x => x.Category.CategoryName)
                        : query.OrderByDescending(x => x.Category.CategoryName),

                "defaultGst" =>
                    request.Query.SortDirection == "asc"
                        ? query.OrderBy(x => x.DefaultGst)
                        : query.OrderByDescending(x => x.DefaultGst),

                "createdOn" =>
                    request.Query.SortDirection == "asc"
                        ? query.OrderBy(x => x.CreatedOn)
                        : query.OrderByDescending(x => x.CreatedOn),

                _ => query.OrderByDescending(x => x.CreatedOn)
            };

            // ============================
            // 📊 COUNT (AFTER FILTERS)
            // ============================
            var totalCount = await query.CountAsync(cancellationToken);

            // ============================
            // 📄 PAGING + PROJECTION
            // ============================
            var items = await query
                .Skip((request.Query.PageNumber - 1) * request.Query.PageSize)
                .Take(request.Query.PageSize)
                .Select(x => new SubcategoryDto
                {
                    Id = x.Id,
                    CategoryName = x.Category.CategoryName,
                    SubcategoryName = x.SubcategoryName,
                    SubcategoryCode = x.SubcategoryCode,
                    DefaultGst = x.DefaultGst,
                    IsActive = x.IsActive,
                    CreatedOn = x.CreatedOn,
                    CreatedBy = x.CreatedBy
                })
                .ToListAsync(cancellationToken);

            return new GridResponse<SubcategoryDto>(items, totalCount);
        }
    }
}
