using Inventory.Application.Common.Interfaces;
using Inventory.Application.GRN.DTOs.Stock;
using MediatR;

public class GetCurrentStockHandler : IRequestHandler<GetCurrentStockCommand, StockPagedResponseDto>
{
    private readonly IStockRepository _repository;
    public GetCurrentStockHandler(IStockRepository repository) => _repository = repository;

    public async Task<StockPagedResponseDto> Handle(GetCurrentStockCommand request, CancellationToken ct)
    {
        return await _repository.GetCurrentStockAsync(
            request.Search,
            request.SortField,
            request.SortOrder,
            request.PageIndex,
            request.PageSize,
            request.StartDate, // Pass to Repository
            request.EndDate    // Pass to Repository
        );
    }
}