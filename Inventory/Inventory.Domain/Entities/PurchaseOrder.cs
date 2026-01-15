

using Inventory.Domain.Common;
using Inventory.Domain.Enums;

namespace Inventory.Domain.Entities;

public sealed class PurchaseOrder : BaseAuditableEntity
{
    private readonly List<PurchaseOrderItem> _items = new();

    private PurchaseOrder() { } // EF

    public PurchaseOrder(
        Guid supplierId,
        DateTime poDate,
        string poNumber)
    {
        SupplierId = supplierId;
        PoDate = poDate;
        PoNumber = poNumber;
        Status = PurchaseOrderStatus.Draft;
    }

    public Guid SupplierId { get; private set; }
    public DateTime PoDate { get; private set; }
    public string PoNumber { get; private set; }
    public PurchaseOrderStatus Status { get; private set; }

    public IReadOnlyCollection<PurchaseOrderItem> Items => _items.AsReadOnly();

    public decimal TotalAmount => _items.Sum(x => x.TotalAmount);

    public void AddItem(
        Guid productId,
        decimal quantity,
        decimal unitPrice,
        decimal discountPercent,
        decimal gstPercent)
    {
        var item = new PurchaseOrderItem(
            productId,
            quantity,
            unitPrice,
            discountPercent,
            gstPercent
        );

        _items.Add(item);
    }
}
