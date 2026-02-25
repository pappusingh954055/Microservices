using MediatR;

namespace Inventory.Application.Locations.Warehouses.Commands.UpdateWarehouse;

public record UpdateWarehouseCommand(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive
) : IRequest<Unit>;
