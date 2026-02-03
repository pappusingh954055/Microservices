public class PurchaseReturnItem
{
    public Guid Id { get; set; }
    public Guid PurchaseReturnId { get; set; }
    public Guid ProductId { get; set; } // Kept as Guid [cite: 2026-02-03]
    public string GrnRef { get; set; } // UI se matching reference
    public decimal ReturnQty { get; set; }
    public decimal Rate { get; set; }
    public decimal TotalAmount { get; set; }

    // Navigation Property
    public PurchaseReturn PurchaseReturn { get; set; }
}