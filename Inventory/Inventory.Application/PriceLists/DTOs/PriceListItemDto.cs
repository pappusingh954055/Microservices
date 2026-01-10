namespace Inventory.Application.PriceLists.DTOs;

public sealed class PriceListItemDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; set; }
    public decimal Price { get; set; }
    public int MinQty { get; set; }
    public int? MaxQty { get; set; }
    public bool IsActive { get; set; }
}
