using Company.Application.Common.Models;
using Company.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Company.Application.Common.Interfaces
{
    public interface ICompanyRepository
    {
        Task<CompanyProfile?> GetCompanyProfileAsync();
        Task<CompanyProfile?> GetByIdAsync(int id);
        Task<bool> DeleteCompanyProfileAsync(int id);
        Task<int> InsertCompanyAsync(CompanyProfile company);
        Task<int> UpsertCompanyProfileAsync(CompanyProfile profile);
        Task<GridResponse<CompanyProfile>> GetPagedAsync(GridRequest request);
    }
}

