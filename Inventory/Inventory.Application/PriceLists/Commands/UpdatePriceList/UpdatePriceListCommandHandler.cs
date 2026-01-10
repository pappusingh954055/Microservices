using Inventory.Application.Common.Interfaces;
using MediatR;

namespace Inventory.Application.PriceLists.Commands.UpdatePriceList;

internal sealed class UpdatePriceListCommandHandler
    : IRequestHandler<UpdatePriceListCommand, Guid>
{
    private readonly IPriceListRepository _repository;
    private readonly IInventoryDbContext _context;

    public UpdatePriceListCommandHandler(
        IPriceListRepository repository,
        IInventoryDbContext context)
    {
        _repository = repository;
        _context = context;
    }

    public async Task<Guid> Handle(
        UpdatePriceListCommand request,
        CancellationToken cancellationToken)
    {
        var priceList = await _repository.GetByIdAsync(request.Id);

        if (priceList is null)
            throw new KeyNotFoundException("PriceList not found");

        priceList.Update(
            request.Name,
            request.Code,
            request.ValidFrom,
            request.ValidTo,
            request.IsActive
        );

        await _context.SaveChangesAsync(cancellationToken);

        return priceList.Id;
    }
}
