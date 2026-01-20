using Inventory.Application.Common.Interfaces;
using Inventory.Domain.PriceLists;
using MediatR;

namespace Inventory.Application.PriceLists.Commands.CreatePriceList;

public sealed class CreatePriceListCommandHandler
    : IRequestHandler<CreatePriceListCommand, Guid>
{
    private readonly IPriceListRepository _repository;
    private readonly IInventoryDbContext _context;

    public CreatePriceListCommandHandler(IPriceListRepository repository, IInventoryDbContext context)
    {
        _repository = repository;
        _context = context;
    }

    public async Task<Guid> Handle(CreatePriceListCommand request, CancellationToken ct)
    {
        // 1. Header mapping (UI ke naye fields ke sath)
        var priceList = new PriceList(
            request.name,
            request.code,
            request.priceType,
            request.applicableGroup,
            request.currency,
            request.validFrom,
            request.validTo,
            request.remarks,
            request.isActive,
            request.createdBy
        );

        // 2. Details mapping
        foreach (var itemDto in request.priceListItems)
        {
            var item = new PriceListItem(
                priceList.Id,
                itemDto.productId,
                itemDto.price,
                itemDto.discountPercent, // UI column Disc (%) mapping
                itemDto.minQty,
                itemDto.maxQty
            );
            priceList.PriceListItems.Add(item);
        }

        // 3. Save Atomic Transaction
        await _repository.AddAsync(priceList);
        await _context.SaveChangesAsync(ct);

        return priceList.Id;
    }
}