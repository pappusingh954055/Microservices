namespace Inventory.Application.PriceLists.DTOs;

public sealed class PriceListDto
{
    public Guid id { get; set; }
    public string name { get; set; } = string.Empty;
    public string code { get; set; } = string.Empty;

    public DateTime? validfrom { get; set; }
    public DateTime? validto { get; set; }

    public bool isactive { get; set; }
    public string? description { get; set; }
    public string? pricetype { get; set; }

    public List<PriceListItemDto> Items { get; set; } = new();
}
