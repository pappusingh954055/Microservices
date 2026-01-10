using MediatR;

namespace Inventory.Application.PriceLists.Commands.CreatePriceList
{
    public sealed record CreatePriceListCommand(
    string Name,
    string Code,
    DateTime ValidFrom,
    DateTime ValidTo,
    Boolean isActive
) : IRequest<Guid>;

}
