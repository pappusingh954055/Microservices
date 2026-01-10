using Inventory.Application.Common.Interfaces;
using Inventory.Domain.PriceLists;
using MediatR;

namespace Inventory.Application.PriceLists.Commands.CreatePriceList;

public sealed class CreatePriceListCommandHandler
    : IRequestHandler<CreatePriceListCommand, Guid>
{
    private readonly IPriceListRepository _repository;

    public CreatePriceListCommandHandler(IPriceListRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(
        CreatePriceListCommand request,
        CancellationToken cancellationToken)
    {
        var priceList = new PriceList(            
            request.Name,
            request.Code,
            request.ValidFrom,
            request.ValidTo,
            request.isActive
        );

        await _repository.AddAsync(priceList);
        return priceList.Id;
    }
}
