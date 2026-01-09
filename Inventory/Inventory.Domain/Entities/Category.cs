namespace Inventory.Domain.Entities;

public class Category
{
    public Guid Id { get; private set; }

    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public decimal DefaultGst { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    private Category() { } // EF Core

    public Category(
        string code,
        string name,
        decimal defaultGst,
        string? description, bool IsActive)
    {
        Id = Guid.NewGuid();
        Code = code;
        Name = name;
        DefaultGst = defaultGst;
        Description = description;
        IsActive = true;
    }

    public void Update(
        string code,
        string name,
        decimal defaultGst,
        string? description,
        bool isActive)
    {
        Code = code;
        Name = name;
        DefaultGst = defaultGst;
        Description = description;
        IsActive = isActive;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
