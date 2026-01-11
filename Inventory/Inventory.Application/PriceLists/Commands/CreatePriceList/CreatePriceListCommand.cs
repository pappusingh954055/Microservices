using MediatR;

namespace Inventory.Application.PriceLists.Commands.CreatePriceList
{
    public sealed record CreatePriceListCommand(
    string name,
    string code,
    string pricetype,
    DateTime validfrom,
    DateTime validto,
    string description,
    Boolean isactive
) : IRequest<Guid>;

}
