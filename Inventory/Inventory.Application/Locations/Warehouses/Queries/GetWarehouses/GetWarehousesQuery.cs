using Inventory.Application.Locations.Warehouses.DTOs;
using MediatR;

namespace Inventory.Application.Locations.Warehouses.Queries.GetWarehouses;

public record GetWarehousesQuery() : IRequest<List<WarehouseDto>>;
