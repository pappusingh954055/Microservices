using MediatR;
using Inventory.Application.Common.Interfaces;
using Inventory.Application.PriceLists.DTOs;

namespace Inventory.Application.PriceLists.Queries.GetPriceLists;

public sealed class GetPriceListsQueryHandler
    : IRequestHandler<GetPriceListsQuery, List<PriceListDto>>
{
    private readonly IPriceListRepository _repository;

    public GetPriceListsQueryHandler(IPriceListRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<PriceListDto>> Handle(
        GetPriceListsQuery request,
        CancellationToken cancellationToken)
    {
        var lists = await _repository.GetAllAsync();

        return lists.Select(pl => new PriceListDto
        {
            id = pl.Id,
            Name = pl.Name,
            Code = pl.Code,
            pricetype=pl.PriceType,
            description= pl.Description,    
            validfrom = pl.ValidFrom,
            validto = pl.ValidTo,
            isactive = pl.IsActive
        }).ToList();
    }
}
