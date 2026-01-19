namespace Inventory.Application.PriceLists.DTOs;

public sealed class PriceListDto
{
    public Guid id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;

    public int? CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public DateTime? ModifiedOn { get; set; } = DateTime.UtcNow;
    public int? ModifiedBy { get; set; }
    public DateTime? validfrom { get; set; }
    public DateTime? validto { get; set; }

    public bool isactive { get; set; }
    public string? description { get; set; }
    public string? pricetype { get; set; }

    public List<PriceListItemDto> Items { get; set; } = new();
}
