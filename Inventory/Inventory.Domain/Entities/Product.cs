public class Product
{
    public Guid Id { get; private set; }

    public Guid CategoryId { get; private set; }
    public Guid SubcategoryId { get; private set; }

    public string Sku { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string Unit { get; private set; } = null!;
    public string HSNCode { get; private set; } = null!;
    public int MinStock { get; private set; }

    public decimal? DefaultGst { get; private set; }
    public string? Description { get; private set; }
    public bool TrackInventory { get; private set; }

    private Product() { }

    public Product(
        Guid categoryid,
        Guid subcategoryid,
        string sku,
        string name,
        string unit,
        string hsncode,
        int minstock,
        decimal? defaultgst,
        string? description,
        bool trackinventory)
    {
        Id = Guid.NewGuid();
        CategoryId = categoryid;
        SubcategoryId = subcategoryid;
        Sku = sku;
        Name = name;
        Unit = unit;
        HSNCode = hsncode;  
        DefaultGst = defaultgst;
        Description = description;
        TrackInventory = trackinventory;
    }

    public void Update(
        string sku,
        string name,
        Guid categoryid,
        Guid subcategoryid,
        string unit,
        string hsncode,
        int minstock,
        decimal? defaultGst,
        string? description,
        bool trackinventory
        )
    {
        Sku = sku;
        Name = name;
        CategoryId = categoryid;
        SubcategoryId = subcategoryid;
        Unit = unit;
        HSNCode = hsncode;
        MinStock= minstock;
        DefaultGst = defaultGst;
        Description = description;
        TrackInventory = trackinventory;
    }
}
