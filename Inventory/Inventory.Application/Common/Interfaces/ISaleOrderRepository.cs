using Inventory.Application.GRN.DTOs.Stock;
using Inventory.Application.SaleOrders.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using YourProjectNamespace.Entities;

namespace Inventory.Application.Common.Interfaces
{
    public interface ISaleOrderRepository
    {
        Task<int> SaveAsync(SaleOrder order);
        Task<string> GetLastSONumberAsync();


        Task<decimal> GetAvailableStockAsync(Guid productId);
        Task UpdateProductStockAsync(Guid productId, decimal adjustmentQty);
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

        Task<List<StockExportDto>> GetSaleReportDataAsync(List<int> orderIds);

        // Naya signature jo parameters ke saath total count bhi return karega
        Task<(List<SaleOrderListDto> Data, int TotalCount)> GetAllSaleOrdersAsync(
            string searchTerm,
            int pageNumber,
            int pageSize,
            string sortBy,
            string sortOrder);

        Task<bool> UpdateSaleOrderStatusAsync(int id, string status);

        Task<SaleOrderDetailDto?> GetSaleOrderByIdAsync(int id);
    }
}
