using MediatR;

namespace Inventory.Application.PriceLists.Commands.UpdatePriceList;

public sealed record UpdatePriceListCommand(
    Guid Id,
    string Name,
    string Code,
    DateTime ValidFrom,
    DateTime? ValidTo,
    bool IsActive
) : IRequest<Guid>;
