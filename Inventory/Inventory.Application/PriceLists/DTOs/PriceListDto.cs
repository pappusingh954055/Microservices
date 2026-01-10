namespace Inventory.Application.PriceLists.DTOs;

public sealed class PriceListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;

    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }

    public bool IsActive { get; set; }

    public List<PriceListItemDto> Items { get; set; } = new();
}
