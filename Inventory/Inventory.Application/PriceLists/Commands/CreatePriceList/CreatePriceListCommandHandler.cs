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
        var priceList = new PriceList(            
            request.name,
            request.code,
            request.pricetype,
            request.validfrom,
            request.validto,
            request.description,
            request.isactive
        );

        await _repository.AddAsync(priceList);

        await _context.SaveChangesAsync(cancellationToken);  

        return priceList.Id;
    }
}
