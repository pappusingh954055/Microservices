using Inventory.Application.GRN.DTOs;
using Inventory.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Common.Interfaces
{
    public interface IGRNRepository
    {
        Task<POForGRNDTO> GetPODataForGRN(int poId);
        Task<string> GenerateGRNNumber();
        Task<string> SaveGRNWithStockUpdate(GRNHeader header, List<GRNDetail> details);
        Task<GRNPagedResponseDto> GetGRNPagedListAsync(string search, string sortField, string sortOrder, int pageIndex, int pageSize);
    }
}
