using Inventory.Application.Common.Interfaces;
using Inventory.Application.Common.Models;
using Inventory.Application.PriceLists.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.PriceLists.Queries.Paged
{
    internal sealed class GetPriceListsPagedQueryHandler
    : IRequestHandler<GetPriceListsPagedQuery, GridResponse<PriceListDto>>
    {
        private readonly IPriceListRepository _repository;

        public GetPriceListsPagedQueryHandler(IPriceListRepository repository)
        {
            _repository = repository;
        }

        public async Task<GridResponse<PriceListDto>> Handle(
            GetPriceListsPagedQuery request,
            CancellationToken cancellationToken)
        {
            var query = _repository.Query();

            // 🔍 SEARCH
            if (!string.IsNullOrWhiteSpace(request.Request.Search))
            {
                var search = request.Request.Search.ToLower();
                query = query.Where(x =>
                    x.Name.ToLower().Contains(search) ||
                    x.Code.ToLower().Contains(search));
            }

            // 🔃 SORT
            query = request.Request.SortBy switch
            {
                "name" => request.Request.SortDirection == "asc"
                    ? query.OrderBy(x => x.Name)
                    : query.OrderByDescending(x => x.Name),

                "code" => request.Request.SortDirection == "asc"
                    ? query.OrderBy(x => x.Code)
                    : query.OrderByDescending(x => x.Code),

                "validfrom" => request.Request.SortDirection == "asc"
                    ? query.OrderBy(x => x.ValidFrom)
                    : query.OrderByDescending(x => x.ValidFrom),

                "validto" => request.Request.SortDirection == "asc"
                    ? query.OrderBy(x => x.ValidTo)
                    : query.OrderByDescending(x => x.ValidTo),

                "description" => request.Request.SortDirection == "asc"
                    ? query.OrderBy(x => x.Description)
                    : query.OrderByDescending(x => x.Description),

                "createdon" => request.Request.SortDirection == "asc"
                    ? query.OrderBy(x => x.CreatedOn)
                    : query.OrderByDescending(x => x.CreatedOn),

                _ => query.OrderByDescending(x => x.CreatedOn)
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((request.Request.PageNumber - 1) * request.Request.PageSize)
                .Take(request.Request.PageSize)
                .Select(x => new PriceListDto
                {
                    id = x.Id,
                    Name = x.Name,
                    Code = x.Code,
                    CreatedOn = x.CreatedOn,
                    pricetype=x.PriceType,
                    description=x.Description,
                    validfrom = x.ValidFrom,
                    validto = x.ValidTo,
                    isactive = x.IsActive
                })
                .ToListAsync(cancellationToken);

            return new GridResponse<PriceListDto>(items, totalCount);
        }
    }
}