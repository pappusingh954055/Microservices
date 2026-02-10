using Inventory.Application.Clients.DTOs;

namespace Inventory.Application.Clients
{
    public interface ICompanyClient
    {
        Task<CompanyProfileDto> GetCompanyProfileAsync();
    }
}
