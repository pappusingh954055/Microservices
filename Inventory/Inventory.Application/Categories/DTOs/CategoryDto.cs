namespace Inventory.Application.Categories.DTOs;

public sealed class CategoryDto
{
    public Guid Id { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryCode { get; set; } = string.Empty;
    public decimal DefaultGst { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
