namespace Domain.Entities;

public class PurchaseOrderItem
{
    public Guid Id { get; private set; }
    public Guid PurchaseOrderId { get; private set; }
    public Guid ProductId { get; private set; } //
    public int Qty { get; private set; } //
    public decimal Price { get; private set; } //
    public decimal DiscountPercent { get; private set; } //
    public decimal GstPercent { get; private set; } //
    public decimal Total { get; private set; } //

    private PurchaseOrderItem() { } // EF Core ke liye

    internal PurchaseOrderItem(
        Guid purchaseOrderId,
        Guid productId,
        int qty,
        decimal price,
        decimal discountPercent,
        decimal gstPercent,
        decimal total)
    {
        Id = Guid.NewGuid();
        PurchaseOrderId = purchaseOrderId;
        ProductId = productId;
        Qty = qty;
        Price = price;
        DiscountPercent = discountPercent;
        GstPercent = gstPercent;
        Total = total;
    }
}