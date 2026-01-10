using System;

namespace Inventory.Domain.PriceLists;

public class PriceListItem
{
    public Guid Id { get; private set; }
    public Guid PriceListId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Price { get; private set; }

    private PriceListItem() { } // EF Core

    internal PriceListItem(
        Guid id,
        Guid priceListId,
        Guid productId,
        decimal price)
    {
        Id = id;
        PriceListId = priceListId;
        ProductId = productId;
        Price = price;
    }
}
