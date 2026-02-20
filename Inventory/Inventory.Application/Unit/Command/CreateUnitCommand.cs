using Inventory.Application.Unit.DTOs;
using MediatR;

namespace Inventory.Application.Unit.Command
{
    public record CreateBulkUnitsCommand(List<UnitRequestDto> Units) : IRequest<bool>;
    public record UpdateUnitCommand(int Id, string Name, string Description, bool IsActive) : IRequest<bool>;
    public record DeleteUnitCommand(int Id) : IRequest<bool>;
}
