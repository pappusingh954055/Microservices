using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Services
{
    public interface ICustomerHttpService
    {
        Task<Dictionary<int, string>> GetCustomerNamesAsync(List<int> customerIds);
    }
}
