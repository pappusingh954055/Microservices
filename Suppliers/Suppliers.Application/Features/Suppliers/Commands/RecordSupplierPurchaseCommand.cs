using MediatR;
using Suppliers.Application.DTOs;

namespace Suppliers.Application.Features.Suppliers.Commands
{
    public class RecordSupplierPurchaseCommand(SupplierPurchaseDto purchaseData) : IRequest<bool>
    {
        public SupplierPurchaseDto PurchaseData { get; } = purchaseData;
    }
}
