namespace Inventory.Domain.Entities;

public class ExpenseEntry
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public virtual ExpenseCategory? Category { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string PaymentMode { get; set; } = null!; // Cash, Bank, UPI, etc.
    public string? ReferenceNo { get; set; }
    public string? Remarks { get; set; }
    public string? AttachmentPath { get; set; }

    public DateTime? CreatedDate { get; set; } = DateTime.Now;
    public DateTime? UpdatedDate { get; set; } = DateTime.Now;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
