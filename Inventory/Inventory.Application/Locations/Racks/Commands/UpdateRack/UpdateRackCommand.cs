using MediatR;

namespace Inventory.Application.Locations.Racks.Commands.UpdateRack;

public record UpdateRackCommand(
    Guid Id,
    Guid WarehouseId,
    string Name,
    string? Description,
    bool IsActive
) : IRequest<Unit>;
