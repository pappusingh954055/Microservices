using Inventory.Application.Common.Interfaces;
using MediatR;

namespace Inventory.Application.PriceLists.Commands.DeletePriceList;

internal sealed class DeletePriceListCommandHandler
    : IRequestHandler<DeletePriceListCommand>
{
    private readonly IPriceListRepository _repository;

    public DeletePriceListCommandHandler(IPriceListRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(
        DeletePriceListCommand request,
        CancellationToken cancellationToken)
    {
        var priceList = await _repository.GetByIdAsync(request.Id);

        if (priceList is null)
            throw new KeyNotFoundException("Price list not found");

        await _repository.DeleteAsync(priceList);
    }
}
