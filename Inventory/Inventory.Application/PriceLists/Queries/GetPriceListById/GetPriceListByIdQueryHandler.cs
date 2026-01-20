using Inventory.Application.Common.Interfaces;
using Inventory.Application.PriceLists.DTOs;
using Inventory.Application.PriceLists.Queries.GetPriceListById;
using MediatR;

public class GetPriceListByIdQueryHandler : IRequestHandler<GetPriceListByIdQuery, PriceListDto>
{
    private readonly IPriceListRepository _repository;

    public GetPriceListByIdQueryHandler(IPriceListRepository repository)
    {
        _repository = repository;
    }

    public async Task<PriceListDto> Handle(GetPriceListByIdQuery request, CancellationToken cancellationToken)
    {
        // FIX: Repository use karein na ki _context
        var entity = await _repository.GetByIdWithItemsAsync(request.Id, cancellationToken);

        if (entity == null)
        {
            // Error handling agar record na mile
            throw new Exception($"PriceList with ID {request.Id} not found");
        }

        // 2. Entity ko DTO mein map karein (Mapping logic sahi hai)
        return new PriceListDto
        {
            id = entity.Id,
            name = entity.Name,
            code = entity.Code,
            priceType = entity.PriceType,
            validFrom = entity.ValidFrom,
            validTo = entity.ValidTo,
            isActive = entity.IsActive,
            remarks = entity.Remarks,
            currency = entity.Currency,
            applicableGroup = entity.ApplicableGroup,
            // Child items binding taaki Angular grid fill ho sake
            items = entity.PriceListItems.Select(item => new PriceListItemDetailDto
            {
                productId = item.ProductId,
                productName = item.Product.Name, // Angular search box ke liye
                unit = item.Unit,
                price = item.Price,
                discountPercent = item.DiscountPercent,
                minQty = item.MinQty,
                maxQty = item.MaxQty
            }).ToList()
        };
    }
}