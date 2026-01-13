namespace Inventory.Domain.Entities;

public sealed class Subcategory
{
    public Guid Id { get; private set; }

    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; }

    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public decimal DefaultGst { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    private Subcategory() { }

    public Subcategory(
        Guid categoryid,
        string code,
        string name,
        decimal defaultGst,
        string? description,
        bool isactive)
    {
        Id = Guid.NewGuid();
        CategoryId = categoryid;
        Code = code;
        Name = name;
        DefaultGst = defaultGst;
        Description = description;
        IsActive = isactive;
    }

    public void Update(
        string code,
        string name,
        Guid categoryid,
        decimal defaultgst,
        string? description,
        bool isActive)
    {
        Code = code;
        Name = name;
        CategoryId = categoryid;
        DefaultGst = defaultgst;
        Description = description;
        IsActive = isActive;
    }
}
