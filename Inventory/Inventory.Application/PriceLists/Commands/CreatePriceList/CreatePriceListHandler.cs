using Inventory.Application.Common.Interfaces;
using MediatR;

public class CreatePriceListHandler : IRequestHandler<CreatePriceListCommand, Guid>
{
    private readonly IPriceListRepository _repository;

    public CreatePriceListHandler(IPriceListRepository repository) => _repository = repository;

    public async Task<Guid> Handle(CreatePriceListCommand request, CancellationToken ct)
    {
        // 1. Header mapping
        var priceList = new PriceList(
            request.name, request.code, request.priceType,
            request.validFrom, request.validTo, request.isActive);

        // 2. Detail mapping
        foreach (var item in request.priceListItems)
        {
            priceList.PriceListItems.Add(new PriceListItem(
                priceList.Id, item.productId, item.price, item.minQty, item.maxQty));
        }

        // 3. Save atomic transaction
        await _repository.AddAsync(priceList, ct);
        await _repository.SaveChangesAsync(ct);

        return priceList.Id;
    }
}