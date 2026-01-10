public sealed class ProductDto
{
    public Guid Id { get; set; }

    public Guid CategoryId { get; set; }
    public Guid SubcategoryId { get; set; }

    public string Sku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;

    public decimal? DefaultGst { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
