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
            var result = await _repo.GetDateRangePagedOrdersAsync(query.Request);

            var dtos = result.Data.Select(x => new PurchaseOrderDto
            {
                PoNumber = x.PoNumber,
                SupplierName = x.SupplierName,
                PoDate = x.PoDate,
                TotalTax = x.TotalTax,
                GrandTotal = x.GrandTotal,
                Status = x.Status,

                // Items mapping with ProductName from Included Product table
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
                    // Product table se Name uthaya
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
