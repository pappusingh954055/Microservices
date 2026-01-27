using Inventory.Application.GRN.DTOs.Stock;
using MediatR;

public record GetCurrentStockCommand(
    string? Search,
    string? SortField,
    string? SortOrder,
    int PageIndex,
    int PageSize
) : IRequest<StockPagedResponseDto>; // Return type yahan bhi badlega