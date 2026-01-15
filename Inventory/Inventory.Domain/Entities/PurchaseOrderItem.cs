using Inventory.Domain.Common;

namespace Inventory.Domain.Entities;

public sealed class PurchaseOrderItem : BaseEntity
{
    private PurchaseOrderItem() { } // EF

    internal PurchaseOrderItem(
        Guid productId,
        decimal quantity,
        decimal unitPrice,
        decimal discountPercent,
        decimal gstPercent)
    {
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        DiscountPercent = discountPercent;
        GstPercent = gstPercent;
    }

    public Guid ProductId { get; private set; }
    public Product Product { get; private set; }

    public Guid PurchaseOrderId { get; private set; }
    public PurchaseOrder PurchaseOrder { get; private set; }

    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public string Unit { get; private set; }
    public decimal DiscountPercent { get; private set; }
    public decimal GstPercent { get; private set; }
     
    public decimal SubTotal => Quantity * UnitPrice;
    public decimal DiscountAmount => SubTotal * (DiscountPercent / 100);
    public decimal GstAmount => (SubTotal - DiscountAmount) * (GstPercent / 100);
    public decimal TotalAmount => SubTotal - DiscountAmount + GstAmount;
}
