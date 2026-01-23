using System.ComponentModel.DataAnnotations.Schema;

public class PurchaseOrderItem
{
    public int Id { get; set; }
    public int PurchaseOrderId { get; set; }
    public Guid ProductId { get; set; } //
    public Product? Product { get; set; } //

    [NotMapped]
    public string ProductName { get; set; } = string.Empty;
    public decimal Qty { get; set; } //
    public string Unit { get; set; } //
    public decimal Rate { get; set; } //
    public decimal DiscountPercent { get; set; }  
    public decimal GstPercent { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; } //
}