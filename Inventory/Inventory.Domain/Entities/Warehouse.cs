using Inventory.Domain.Common;

namespace Inventory.Domain.Entities;

public class Warehouse : BaseAuditableEntity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public virtual ICollection<Rack> Racks { get; private set; } = new List<Rack>();

    private Warehouse() { }

    public Warehouse(string name, string? description, bool isActive)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        IsActive = isActive;
    }

    public void Update(string name, string? description, bool isActive)
    {
        Name = name;
        Description = description;
        IsActive = isActive;
    }
}
