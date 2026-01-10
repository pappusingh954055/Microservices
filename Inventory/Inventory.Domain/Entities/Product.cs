public class Product
{
    public Guid Id { get; private set; }

    public Guid CategoryId { get; private set; }
    public Guid SubcategoryId { get; private set; }

    public string Sku { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string Unit { get; private set; } = null!;

    public decimal? DefaultGst { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    private Product() { }

    public Product(
        Guid categoryId,
        Guid subcategoryId,
        string sku,
        string name,
        string unit,
        decimal? defaultGst,
        string? description)
    {
        Id = Guid.NewGuid();
        CategoryId = categoryId;
        SubcategoryId = subcategoryId;
        Sku = sku;
        Name = name;
        Unit = unit;
        DefaultGst = defaultGst;
        Description = description;
        IsActive = true;
    }

    public void Update(
        string sku,
        string name,
        string unit,
        decimal? defaultGst,
        string? description,
        bool isActive)
    {
        Sku = sku;
        Name = name;
        Unit = unit;
        DefaultGst = defaultGst;
        Description = description;
        IsActive = isActive;
    }
}
