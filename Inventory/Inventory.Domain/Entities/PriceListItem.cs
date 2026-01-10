namespace Inventory.Domain.PriceLists;

public class PriceListItem
{
    public Guid Id { get; private set; }
    public Guid PriceListId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Price { get; private set; }

    public int MinQty { get; private set; }
    public int? MaxQty { get; private set; }
    public bool IsActive { get; private set; }

    private PriceListItem() { } // EF Core

    internal PriceListItem(
        Guid id,
        Guid priceListId,
        Guid productId,
        decimal price,
        int minQty,
        int maxQty,
        bool isActive)
    {
        Id = id;
        PriceListId = priceListId;
        ProductId = productId;
        Price = price;
        MinQty = minQty;
        MaxQty = maxQty;
        IsActive = isActive;
    }
}
