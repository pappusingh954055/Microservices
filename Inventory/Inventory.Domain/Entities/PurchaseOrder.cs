using Inventory.Domain.PriceLists;

public class PurchaseOrder
{
    public int Id { get; set; }
    public string PoNumber { get; set; } // PO/26-27/0001
    public int SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public Guid PriceListId { get; set; }
    public PriceList? PriceList { get; set; }
    public DateTime PoDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string ? Remarks { get; set; }
    public decimal TotalTax { get; set; } //
    public decimal GrandTotal { get; set; } //
    public string Status { get; set; } = "Draft";
    public string? CreatedBy { get; set; } //
    public string? UpdatedBy { get; set; } //
    public DateTime? CreatedDate { get; set; } = DateTime.Now;
    public DateTime? UpdatedDate { get; set; } = DateTime.Now;
    public virtual ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
}