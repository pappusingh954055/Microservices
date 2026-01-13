using Inventory.Application.Common.Interfaces;
using Inventory.Application.Common.Models;
using Inventory.Application.Products.Queries.GetProducts;
using Inventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

internal sealed class GetProductsPagedQueryHandler
    : IRequestHandler<GetProductsPagedQuery, GridResponse<ProductDto>>
{
    private readonly IProductRepository _repository;

    public GetProductsPagedQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<GridResponse<ProductDto>> Handle(
            GetProductsPagedQuery request,
            CancellationToken cancellationToken)
    {
        var query = _repository.Query();

        // 🔍 SEARCH
        if (!string.IsNullOrWhiteSpace(request.Request.Search))
        {
            var search = request.Request.Search.ToLower();
            query = query.Where(x =>
                x.Name.ToLower().Contains(search) ||
                x.HSNCode.ToLower().Contains(search)||
                x.Sku.ToLower().Contains(search)||
                x.Category.CategoryName.ToLower().Contains(search)||
                x.Subcategory.SubcategoryName.ToLower().Contains(search)
                );
        }

        // 🔃 SORT
        query = request.Request.SortBy switch
        {
            "name" => request.Request.SortDirection == "asc"
                ? query.OrderBy(x => x.Name)
                : query.OrderByDescending(x => x.Name),

            "HSNCode" => request.Request.SortDirection == "asc"
                ? query.OrderBy(x => x.HSNCode)
                : query.OrderByDescending(x => x.HSNCode),

            "sku" => request.Request.SortDirection == "asc"
                ? query.OrderBy(x => x.Sku)
                : query.OrderByDescending(x => x.Sku),

            "categoryname" => request.Request.SortDirection == "asc"
                ? query.OrderBy(x => x.Category.CategoryName)
                : query.OrderByDescending(x => x.Category.CategoryName),

            "subcategoryname" => request.Request.SortDirection == "asc"
                ? query.OrderBy(x => x.Subcategory.SubcategoryName)
                : query.OrderByDescending(x => x.Subcategory.SubcategoryName),

            _ => query.OrderByDescending(x => x.CreatedOn)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Request.PageNumber - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                CategoryId = p.CategoryId,               
                CategoryName= p.Category.CategoryName,
                SubcategoryId = p.SubcategoryId,
                SubcategoryName=p.Subcategory.SubcategoryName,
                Sku = p.Sku,
                ProductName = p.Name,
                Unit = p.Unit,
                HsnCode = p.HSNCode,
                MinStock = p.MinStock,
                DefaultGst = p.DefaultGst,
                Description = p.Description,
                CreatedBy = p.CreatedBy,
                CreatedOn = p.CreatedOn,
                ModifiedBy = p.ModifiedBy,
                ModifiedOn= p.ModifiedOn,   
                TrackInventory = p.TrackInventory
            })
            .ToListAsync(cancellationToken);

        return new GridResponse<ProductDto>(items, totalCount);
    }
}
