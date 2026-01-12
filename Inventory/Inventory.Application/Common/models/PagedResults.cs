namespace Inventory.Application.Common.Models;

public sealed class PagedResult<T>
{
    public int TotalCount { get; set; }
    public List<T> Items { get; set; } = new();
}
