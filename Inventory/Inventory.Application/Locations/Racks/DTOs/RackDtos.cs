namespace Inventory.Application.Locations.Racks.DTOs;

public record RackDto(
    Guid Id,
    Guid WarehouseId,
    string WarehouseName,
    string Name,
    string? Description,
    bool IsActive
);

public record CreateRackDto(
    Guid WarehouseId,
    string Name,
    string? Description,
    bool IsActive
);

public record UpdateRackDto(
    Guid Id,
    Guid WarehouseId,
    string Name,
    string? Description,
    bool IsActive
);
