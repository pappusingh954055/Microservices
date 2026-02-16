using Inventory.Application.PurchaseReturn;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Clients
{
    public interface ISupplierClient
    {
        Task<List<SupplierSelectDto>> GetSuppliersByIdsAsync(List<int> supplierIds);
        Task<bool> RecordPurchaseAsync(int supplierId, decimal amount, string referenceId, string description, string createdBy);
        Task<Dictionary<string, decimal>> GetGRNPaymentStatusesAsync(List<string> grnNumbers);
    }
}
