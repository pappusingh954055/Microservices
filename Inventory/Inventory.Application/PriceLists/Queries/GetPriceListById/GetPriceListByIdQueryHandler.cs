using Inventory.Application.Common.Interfaces;
using Inventory.Application.PriceLists.DTOs;
using MediatR;

namespace Inventory.Application.PriceLists.Queries.GetPriceListById;

public sealed class GetPriceListByIdQueryHandler
    : IRequestHandler<GetPriceListByIdQuery, PriceListDto>
{
    private readonly IPriceListRepository _repository;

    public GetPriceListByIdQueryHandler(IPriceListRepository repository)
    {
        _repository = repository;
    }

    public async Task<PriceListDto> Handle(
        GetPriceListByIdQuery request,
        CancellationToken cancellationToken)
    {
        var priceList = await _repository.GetByIdAsync(request.Id);

        if (priceList is null)
            throw new KeyNotFoundException("Price list not found");

        return new PriceListDto
        {
            id = priceList.Id,
            name = priceList.Name,
            code = priceList.Code,
            validfrom = priceList.ValidFrom,
            validto = priceList.ValidTo,
            isactive = priceList.IsActive,
            Items = priceList.Items.Select(i => new PriceListItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                Price = i.Price,
                MinQty = i.MinQty,
                MaxQty = i.MaxQty,
                IsActive = i.IsActive
            }).ToList()
        };
    }
}
