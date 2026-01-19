namespace Inventory.Domain.PriceLists;

public class PriceListItem
{
    public Guid Id { get; private set; }
    public Guid PriceListId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Price { get; private set; }

    public int MinQty { get; private set; }
    public int MaxQty { get; private set; }
    public bool IsActive { get; private set; }
    public int? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; } = DateTime.Now;
    public DateTime? ModifiedOn { get; set; } = DateTime.UtcNow;
    public int? ModifiedBy { get; set; }
    private PriceListItem() { } // EF Core

    public PriceListItem(
       Guid id,              // 1
       Guid priceListId,     // 2
       Guid productId,       // 3
       decimal price,        // 4
       int minQty,           // 5
       int maxQty,          // 6 (Aapne int? rakha hai upar, yahan bhi same rakhein)
       bool isActive)        // 7
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
