using Inventory.Domain.Common;

namespace Inventory.Domain.Entities;

public class Rack : BaseAuditableEntity
{
    public Guid Id { get; private set; }
    public Guid WarehouseId { get; private set; }
    public virtual Warehouse Warehouse { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    private Rack() { }

    public Rack(Guid warehouseId, string name, string? description, bool isActive)
    {
        Id = Guid.NewGuid();
        WarehouseId = warehouseId;
        Name = name;
        Description = description;
        IsActive = isActive;
    }

    public void Update(Guid warehouseId, string name, string? description, bool isActive)
    {
        WarehouseId = warehouseId;
        Name = name;
        Description = description;
        IsActive = isActive;
    }
}
