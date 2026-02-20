using Inventory.Application.Unit.DTOs;
using MediatR;

namespace Inventory.Application.Unit.Command
{
    public record CreateBulkUnitsCommand(List<UnitRequestDto> Units) : IRequest<bool>;
}
