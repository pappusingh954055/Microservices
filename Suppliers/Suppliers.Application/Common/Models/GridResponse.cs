namespace Suppliers.Application.Common.Models;

public sealed class GridResponse<T>
{
    public GridResponse(List<T> items, int totalCount)
    {
        Items = items;
        TotalCount = totalCount;
    }

    public List<T> Items { get; }
    public int TotalCount { get; }
}
