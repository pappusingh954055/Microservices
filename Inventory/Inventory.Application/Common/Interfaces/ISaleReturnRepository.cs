using Inventory.Application.SaleOrders.SaleReturn.DTOs;
using Inventory.Domain.Entities;

public interface ISaleReturnRepository
{
    Task<SaleReturnPagedResponse> GetSaleReturnsAsync(
         string? search,
         int pageIndex,
         int pageSize,
         DateTime? fromDate,
         DateTime? toDate,
         string sortField,
         string sortOrder);

    Task<bool> CreateSaleReturnAsync(SaleReturnHeader returnHeader);
    Task<decimal> GetRemainingReturnableQtyAsync(int saleOrderId, Guid productId);

    Task<List<SaleReturnExportDto>> GetExportDataAsync(DateTime? fromDate, DateTime? toDate);
}