using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Services
{
    public interface ISaleReturnService
    {
        // Number 1: List API ka contract
        Task<SaleReturnPagedResponse> GetSaleReturnListAsync(
            string? search,
            int pageIndex,
            int pageSize,
            DateTime? fromDate,
            DateTime? toDate,
            string sortField,
            string sortOrder);

        Task<bool> SaveReturnAsync(CreateSaleReturnDto dto);
    }
}
