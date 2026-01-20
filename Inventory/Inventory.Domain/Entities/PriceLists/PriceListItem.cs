namespace Inventory.Domain.PriceLists;

public class PriceListItem
{
    public Guid Id { get;  set; }
    public Guid PriceListId { get;  set; }
    public Guid ProductId { get;  set; }
    public Product Product { get;  set; }    
    public decimal Price { get;  set; }
    public string Unit { get; set; }
    public decimal DiscountPercent { get;  set; } // UI: Disc (%)
    public int MinQty { get;  set; }
    public int MaxQty { get;  set; }

    public PriceListItem() { }

    public PriceListItem(Guid priceListId, Guid productId, decimal price,
                         decimal discountPercent, int minQty, int maxQty)
    {
        Id = Guid.NewGuid();
        PriceListId = priceListId;
        ProductId = productId;
        Price = price;
        DiscountPercent = discountPercent;
        MinQty = minQty;
        MaxQty = maxQty;
    }
}