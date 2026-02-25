using MediatR;

namespace Inventory.Application.Locations.Warehouses.Commands.DeleteWarehouse;

public record DeleteWarehouseCommand(Guid Id) : IRequest<Unit>;
