using Inventory.Application.Common.Interfaces;
using MediatR;

namespace Inventory.Application.PurchaseOrders.Commands.Update
{
    public class UpdatePurchaseOrderHandler : IRequestHandler<UpdatePurchaseOrderCommand, bool>
    {
        private readonly IPurchaseOrderRepository _repo;
        private readonly IUnitOfWork _uow;

        public UpdatePurchaseOrderHandler(IPurchaseOrderRepository repo, IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public async Task<bool> Handle(UpdatePurchaseOrderCommand request, CancellationToken ct)
        {
            var dto = request.Dto;
            // Repo items ke sath data la raha hai
            var po = await _repo.GetByIdWithItemsAsync(dto.Id, ct);

            if (po == null) return false;

            // 1. Update Header (Do not update po.Id as it is the Primary Key)
            po.SupplierId = dto.SupplierId;
            po.SupplierName = dto.SupplierName;
            po.PriceListId = dto.PriceList.Id; // Simplified mapping
            po.PoDate = dto.PoDate;
            po.ExpectedDeliveryDate = dto.ExpectedDeliveryDate;
            po.Remarks = dto.Remarks;
            po.PoNumber = dto.PoNumber;
            po.TotalTax = dto.TotalTax;
            po.GrandTotal = dto.GrandTotal;
            po.UpdatedDate = DateTime.UtcNow;

            // 2. Syncing Logic: Delete removed items
            var itemsToRemove = po.Items
                .Where(existing => !dto.Items.Any(d => d.Id == existing.Id))
                .ToList();

            foreach (var item in itemsToRemove)
            {
                _repo.RemoveItem(item);
            }

            // 3. Update or Add Items
            foreach (var itemDto in dto.Items)
            {
                // Existing item check (Id must not be 0)
                var existingItem = po.Items.FirstOrDefault(i => i.Id == itemDto.Id && i.Id != 0);

                if (existingItem != null)
                {
                    // Update existing item fields
                    existingItem.ProductId = itemDto.ProductId;
                    existingItem.Qty = itemDto.Qty;
                    existingItem.Unit = itemDto.Unit;
                    existingItem.Rate = itemDto.Rate;
                    existingItem.DiscountPercent = itemDto.DiscountPercent;
                    existingItem.GstPercent = itemDto.GstPercent;
                    existingItem.TaxAmount = itemDto.TaxAmount;
                    existingItem.Total = itemDto.Total;
                }
                else
                {
                    // Add new item to the collection
                    po.Items.Add(new PurchaseOrderItem
                    {
                        PurchaseOrderId = po.Id, // Link to the current PO Header
                        ProductId = itemDto.ProductId, // Guid from UI
                        Qty = itemDto.Qty,
                        Unit = itemDto.Unit,
                        Rate = itemDto.Rate,
                        DiscountPercent = itemDto.DiscountPercent,
                        GstPercent = itemDto.GstPercent,
                        TaxAmount = itemDto.TaxAmount,
                        Total = itemDto.Total
                    });
                }
            }

            _repo.Update(po);

            // Unit of Work pattern ensure atomicity
            return await _uow.SaveChangesAsync(ct) > 0;
        }
    }
}