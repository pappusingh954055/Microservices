namespace Inventory.Domain.Entities;

public class Subcategory
{
    public Guid Id { get; private set; }

    public Guid CategoryId { get; private set; }

    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public decimal DefaultGst { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    private Subcategory() { }

    public Subcategory(
        Guid categoryId,
        string code,
        string name,
        decimal defaultGst,
        string? description)
    {
        Id = Guid.NewGuid();
        CategoryId = categoryId;
        Code = code;
        Name = name;
        DefaultGst = defaultGst;
        Description = description;
        IsActive = true;
    }

    public void Update(
        string code,
        string name,
        Guid categoryId,
        decimal defaultGst,
        string? description,
        bool isActive)
    {
        Code = code;
        Name = name;
        CategoryId = categoryId;
        DefaultGst = defaultGst;
        Description = description;
        IsActive = isActive;
    }
}
