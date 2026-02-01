using Inventory.Application.GRN.DTOs.Stock;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Common.Interfaces
{
    public interface IStockRepository
    {
        Task<StockPagedResponseDto> GetCurrentStockAsync(
            string? search,
            string? sortField,
            string? sortOrder,
            int pageIndex,
            int pageSize);

        Task<StockRefillDetailsDto> GetRefillDetailsAsync(Guid productId);

        Task<byte[]> GenerateStockExcel(List<Guid> productIds);
    }
}
