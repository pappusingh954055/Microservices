public sealed class ProductDto
{
    public Guid Id { get; set; }

    public Guid CategoryId { get; set; }
    public Guid CategoryName { get; set; }
    public Guid SubcategoryId { get; set; }
    public Guid SubcategoryName { get; set; }

    public string Sku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string HsnCode { get; set; } = string.Empty;
    public int MinStock { get; set; }

    public decimal? DefaultGst { get; set; }
    public string? Description { get; set; }
    public bool TrackInventory { get; set; }
}
