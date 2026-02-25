using MediatR;

namespace Inventory.Application.Locations.Warehouses.Commands.CreateWarehouse;

public record CreateWarehouseCommand(
    string Name,
    string? City,
    string? Description,
    bool IsActive
) : IRequest<Guid>;
