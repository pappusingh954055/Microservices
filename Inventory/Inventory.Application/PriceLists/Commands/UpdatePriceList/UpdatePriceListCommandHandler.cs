using Inventory.Application.Common.Interfaces;
using MediatR;

namespace Inventory.Application.PriceLists.Commands.UpdatePriceList;

internal sealed class UpdatePriceListCommandHandler
    : IRequestHandler<UpdatePriceListCommand>
{
    private readonly IPriceListRepository _repository;

    public UpdatePriceListCommandHandler(IPriceListRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(
        UpdatePriceListCommand request,
        CancellationToken cancellationToken)
    {
        var priceList = await _repository.GetByIdAsync(request.Id);

        if (priceList is null)
            throw new KeyNotFoundException("Price list not found");

        priceList.Update(
            request.Name,
            request.Code,
            request.ValidFrom,
            request.ValidTo,
            request.IsActive
        );

        await _repository.UpdateAsync(priceList);
    }
}
