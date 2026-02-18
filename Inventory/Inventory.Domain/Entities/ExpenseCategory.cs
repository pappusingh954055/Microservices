using System.ComponentModel.DataAnnotations;

namespace Inventory.Domain.Entities;

public class ExpenseCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime? CreatedDate { get; set; } = DateTime.Now;
    public DateTime? UpdatedDate { get; set; } = DateTime.Now;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    public virtual ICollection<ExpenseEntry> Expenses { get; set; } = new List<ExpenseEntry>();
}
