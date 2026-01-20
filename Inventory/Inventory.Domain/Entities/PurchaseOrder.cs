namespace Domain.Entities;

public class PurchaseOrder
{
    public Guid Id { get; private set; }
    public string PoNumber { get; private set; } //
    public int SupplierId { get; private set; } //
    public DateTime PoDate { get; private set; } //
    public DateTime? ExpectedDeliveryDate { get; private set; } // Added as per requirement
    public string? ReferenceNumber { get; private set; } // Added as per requirement
    public string? Remarks { get; private set; } // Added as per requirement
    public decimal GrandTotal { get; private set; } //
    public string Status { get; private set; } // e.g., "Draft", "Pending", "Received"

    // Navigation Property for Details
    private readonly List<PurchaseOrderItem> _items = new();
    public IReadOnlyCollection<PurchaseOrderItem> Items => _items.AsReadOnly();

    private PurchaseOrder() { } // EF Core ke liye

    public PurchaseOrder(
        string poNumber,
        int supplierId,
        DateTime poDate,
        DateTime? expectedDeliveryDate,
        string? referenceNumber,
        string? remarks,
        decimal grandTotal)
    {
        Id = Guid.NewGuid();
        PoNumber = poNumber;
        SupplierId = supplierId;
        PoDate = poDate;
        ExpectedDeliveryDate = expectedDeliveryDate;
        ReferenceNumber = referenceNumber;
        Remarks = remarks;
        GrandTotal = grandTotal;
        Status = "Draft"; // Initial status
    }

    public void AddItem(Guid productId, int qty, decimal price, decimal discountPercent, decimal gstPercent, decimal total)
    {
        var item = new PurchaseOrderItem(Id, productId, qty, price, discountPercent, gstPercent, total);
        _items.Add(item);
    }
}