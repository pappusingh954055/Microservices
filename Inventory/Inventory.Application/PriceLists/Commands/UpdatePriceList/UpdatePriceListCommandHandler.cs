using Inventory.Application.Common.Interfaces;
using Inventory.Application.PriceLists.Commands.UpdatePriceList;
using Inventory.Domain.PriceLists;
using MediatR;

public class UpdatePriceListCommandHandler : IRequestHandler<UpdatePriceListCommand, bool>
{
    private readonly IPriceListRepository _repository;

    public UpdatePriceListCommandHandler(IPriceListRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(UpdatePriceListCommand request, CancellationToken cancellationToken)
    {
        // 1. Existing entity fetch karein
        var entity = await _repository.GetByIdWithItemsAsync(request.id, cancellationToken);
        if (entity == null) return false;

        // 2. Header Update karein (Ensure request properties match your DTO)
        entity.Name = request.name;
        entity.Code = request.code;
        entity.PriceType = request.priceType; // Entity property 'PriceType' check karein
        entity.ValidFrom = request.validFrom;
        entity.ValidTo = request.validTo;
        entity.IsActive = request.isActive;
        entity.Remarks = request.remarks;

        // 3. Child Items Update
        entity.PriceListItems.Clear();

        foreach (var item in request.priceListItems)
        {
            entity.PriceListItems.Add(new PriceListItem
            {
                ProductId = item.productId,
                Rate = item.rate,
                DiscountPercent = item.discountPercent,
                MinQty = item.minQty,
                MaxQty = item.maxQty,
                Unit = item.unit
            });
        }

        await _repository.UpdatePriceListAsync(entity, cancellationToken);
        return true;
    }
}