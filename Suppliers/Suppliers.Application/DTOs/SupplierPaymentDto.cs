
public class SupplierPaymentDto
{
    public int SupplierId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMode { get; set; } // Cash, Bank, etc.
    public string? ReferenceNumber { get; set; }
    public string? Remarks { get; set; }
    public string CreatedBy { get; set; }
}
