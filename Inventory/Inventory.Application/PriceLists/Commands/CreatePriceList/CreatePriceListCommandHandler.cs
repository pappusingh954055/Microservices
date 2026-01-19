using Inventory.Application.Common.Interfaces;
using Inventory.Domain.PriceLists;
using MediatR;

namespace Inventory.Application.PriceLists.Commands.CreatePriceList;

public sealed class CreatePriceListCommandHandler
    : IRequestHandler<CreatePriceListCommand, Guid>
{
    private readonly IPriceListRepository _repository;
    private readonly IInventoryDbContext _context;

    public CreatePriceListCommandHandler(
        IPriceListRepository repository
        , IInventoryDbContext context)
    {
        _repository = repository;
        _context = context;
    }

    public async Task<Guid> Handle(
     CreatePriceListCommand request,
     CancellationToken cancellationToken)
    {
        // 1. PriceList Header Create Karein
        var priceList = new PriceList(
            request.name,
            request.code,
            request.priceType,
            request.validFrom,
            request.validTo,            
            request.isActive
        );

        // 2. Items loop
        foreach (var itemDto in request.priceListItems)
        {
            
            var item = new PriceListItem(     
                priceList.Id,        
                itemDto.productId,   
                itemDto.price,       
                itemDto.minQty,      
                itemDto.maxQty  
            );

            priceList.PriceListItems.Add(item);
        }

        await _repository.AddAsync(priceList);
        await _context.SaveChangesAsync(cancellationToken);

        return priceList.Id;
    }
}
