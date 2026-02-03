using Inventory.Application.PurchaseReturn;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IPurchaseReturnRepository
{
    // 1. Supplier select karte hi uske rejected items lane ke liye
    Task<List<RejectedItemDto>> GetRejectedItemsBySupplierAsync(int supplierId);

    Task<List<SupplierSelectDto>> GetSuppliersWithRejectionsAsync();

    // 2. Form se pura data save karne aur stock update karne ke liye [cite: 2026-02-03]
    Task<bool> CreatePurchaseReturnAsync(PurchaseReturn returnData);
}