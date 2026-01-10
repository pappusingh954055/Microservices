namespace Inventory.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }

    public Guid CategoryId { get; private set; }
    public Guid SubcategoryId { get; private set; }

    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string Unit { get; private set; } = null!;

    public decimal? DefaultGst { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    private Product() { }

    public Product(
        Guid categoryId,
        Guid subcategoryId,
        string code,
        string name,
        string unit,
        decimal? defaultGst,
        string? description)
    {
        Id = Guid.NewGuid();
        CategoryId = categoryId;
        SubcategoryId = subcategoryId;
        Code = code;
        Name = name;
        Unit = unit;
        DefaultGst = defaultGst;
        Description = description;
        IsActive = true;
    }

    public void Update(
        string code,
        string name,
        string unit,
        decimal? defaultGst,
        string? description,
        bool isActive)
    {
        Code = code;
        Name = name;
        Unit = unit;
        DefaultGst = defaultGst;
        Description = description;
        IsActive = isActive;
    }
}
