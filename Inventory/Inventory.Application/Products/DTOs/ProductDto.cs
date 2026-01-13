using Inventory.Domain.Entities;

public sealed class ProductDto
{
    public Guid Id { get; set; }

    public Guid CategoryId { get; set; }
    
    public Guid SubcategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string SubcategoryName { get; set; } = string.Empty;
    public int? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; } = DateTime.Now;
    public DateTime? ModifiedOn { get; set; } = DateTime.UtcNow;
    public int? ModifiedBy { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string HsnCode { get; set; }
    public int MinStock { get; set; }

    public decimal? DefaultGst { get; set; }
    public string? Description { get; set; }
    public bool TrackInventory { get; set; }
}
