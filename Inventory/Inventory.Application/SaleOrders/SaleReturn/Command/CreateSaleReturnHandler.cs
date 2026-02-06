using Inventory.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Inventory.Application.SaleOrders.SaleReturn.Command
{
    public class CreateSaleReturnHandler : IRequestHandler<CreateSaleReturnCommand, bool>
    {
        private readonly ISaleReturnRepository _repo;
        public CreateSaleReturnHandler(ISaleReturnRepository repo) => _repo = repo;

        public async Task<bool> Handle(CreateSaleReturnCommand request, CancellationToken ct)
        {
            var dto = request.Dto;

            // --- 1. VALIDATION LOGIC START ---
            // Isse dashboard par -4 aana band ho jayega kyunki bache huye se zyada return block ho jayega
            foreach (var item in dto.Items)
            {
                // Repository method call karke remaining returnable quantity mangwayi
                var remainingQty = await _repo.GetRemainingReturnableQtyAsync(dto.SaleOrderId, item.ProductId);

                if (item.ReturnQty > remainingQty)
                {
                    // Agar remaining quantity se zyada return karne ki koshish ki toh exception dega
                    throw new Exception($"Cannot return {item.ReturnQty} units for Product ID {item.ProductId}. Maximum allowed return is {remainingQty}.");
                }
            }
            // --- VALIDATION LOGIC END ---

            var header = new SaleReturnHeader
            {
                CustomerId = dto.CustomerId,
                SaleOrderId = dto.SaleOrderId,
                ReturnDate = dto.ReturnDate,
                Remarks = dto.Remarks,
                ReturnNumber = "SR-" + DateTime.Now.ToString("yyyyMMddHHmm"),
                Status = "Confirmed",
                CreatedOn = DateTime.Now,
                // Dashboard consistency ke liye sum total amount
                TotalAmount = dto.Items.Sum(x => x.TotalAmount),
                ReturnItems = dto.Items.Select(i => new SaleReturnItem
                {
                    ProductId = i.ProductId,
                    ReturnQty = i.ReturnQty,
                    UnitPrice = i.UnitPrice,
                    TaxPercentage = i.TaxPercentage,
                    // Financial calculation for line items
                    TaxAmount = (i.ReturnQty * i.UnitPrice) * (i.TaxPercentage / 100m),
                    TotalAmount = i.TotalAmount,
                    Reason = i.Reason,
                    ItemCondition = i.ItemCondition,
                    CreatedOn = DateTime.Now
                }).ToList()
            };

            return await _repo.CreateSaleReturnAsync(header);
        }
    }
}