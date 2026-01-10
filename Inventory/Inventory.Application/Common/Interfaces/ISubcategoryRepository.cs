using Inventory.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Common.Interfaces
{
    public interface ISubcategoryRepository
    {
        Task AddAsync(Subcategory subcategory);
        Task UpdateAsync(Subcategory subcategory);
        Task DeleteAsync(Subcategory subcategory);

        Task<Subcategory?> GetByIdAsync(Guid id);
        Task<List<Subcategory>> GetAllAsync();
    }
}
