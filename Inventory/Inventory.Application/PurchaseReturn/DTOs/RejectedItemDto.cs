public class RejectedItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public string GrnRef { get; set; } // e.g., PO/26-27/0004
    public decimal RejectedQty { get; set; } // Available for return
    public decimal Rate { get; set; }
    public decimal GstPercent { get; set; }
    public decimal DiscountPercent { get; set; }
}