using AutoMapper;
using Inventory.Application.Common.Interfaces;
using Inventory.Application.Features.PurchaseOrders.Queries;
using MediatR;

namespace Inventory.Application.Features.PurchaseOrders.Handlers
{
    public class GetDateRangePurchaseOrdersQueryHandler : IRequestHandler<GetDateRangePurchaseOrdersQuery, PagedResponse<PurchaseOrderDto>>
    {
        private readonly IPurchaseOrderRepository _repo;

        public GetDateRangePurchaseOrdersQueryHandler(IPurchaseOrderRepository repo)
        {
            _repo = repo;
        }

        public async Task<PagedResponse<PurchaseOrderDto>> Handle(GetDateRangePurchaseOrdersQuery query, CancellationToken ct)
        {
            // 1. Repo se data fetch ho raha hai. 
            // Note: Ensure karein ki aapki Repository mein .Include(x => x.GrnHeaders) laga ho.
            var result = await _repo.GetDateRangePagedOrdersAsync(query.Request);

            var dtos = result.Data.Select(x => new PurchaseOrderDto
            {
                Id = x.Id,
                PoNumber = x.PoNumber,
                SupplierName = x.SupplierName,
                PoDate = x.PoDate,
                TotalTax = x.TotalTax,
                GrandTotal = x.GrandTotal,
                SubTotal = x.SubTotal,
                ExpectedDeliveryDate=x.ExpectedDeliveryDate,
                CreatedBy=x.CreatedBy,
                CreatedDate = x.CreatedDate ?? DateTime.MinValue,
                UpdatedDate = x.UpdatedDate,
                Remarks=x.Remarks,
                Status = (x.GrnHeaders != null && x.GrnHeaders.Any())
                         ? "Received"
                         : x.Status,

                // Items mapping
                Items = x.Items.Select(item => new PurchaseOrderItemDto
                {
                    Id = item.Id,
                    Qty = item.Qty,
                    Unit = item.Unit,
                    Rate = item.Rate,
                    Total = item.Total,
                    TaxAmount = item.TaxAmount,
                    DiscountPercent = item.DiscountPercent,
                    GstPercent = item.GstPercent,
                    ProductName = item.Product != null ? item.Product.Name : "N/A"
                }).ToList()
            }).ToList();

            return new PagedResponse<PurchaseOrderDto>(
                dtos,
                result.Total,
                query.Request.PageIndex,
                query.Request.PageSize
            );
        }
    }
}