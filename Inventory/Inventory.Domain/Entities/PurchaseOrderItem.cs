public class PurchaseOrderItem
{
    public int Id { get; set; }
    public int PurchaseOrderId { get; set; }
    public Guid ProductId { get; set; } //
    public Product? Product { get; set; } //
    public decimal Qty { get; set; } //
    public string Unit { get; set; } //
    public decimal Rate { get; set; } //
    public decimal DiscountPercent { get; set; }  
    public decimal GstPercent { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; } //
}