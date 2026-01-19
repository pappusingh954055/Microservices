// PriceList.cs (Aggregate Root)
public class PriceList
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Code { get; private set; }
    public string PriceType { get; private set; }
    public DateTime ValidFrom { get; private set; }
    public DateTime CreatedOn { get; private set; }
    public DateTime? ValidTo { get; private set; }
    public bool IsActive { get; private set; }

    // Relationship: One-to-Many
    public List<PriceListItem> PriceListItems { get; private set; } = new();

    private PriceList() { } // For EF Core

    public PriceList(string name, string code, string priceType, DateTime validFrom, DateTime? validTo, bool isActive)
    {
        Id = Guid.NewGuid();
        Name = name;
        Code = code;
        PriceType = priceType;
        ValidFrom = validFrom;
        ValidTo = validTo;
        IsActive = isActive;
    }
}

// PriceListItem.cs
public class PriceListItem
{
    public Guid Id { get; private set; }
    public Guid PriceListId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Price { get; private set; }
    public int MinQty { get; private set; }
    public int MaxQty { get; private set; }

    private PriceListItem() { }

    public PriceListItem(Guid priceListId, Guid productId, decimal price, int minQty, int maxQty)
    {
        Id = Guid.NewGuid();
        PriceListId = priceListId;
        ProductId = productId;
        Price = price;
        MinQty = minQty;
        MaxQty = maxQty;
    }
}