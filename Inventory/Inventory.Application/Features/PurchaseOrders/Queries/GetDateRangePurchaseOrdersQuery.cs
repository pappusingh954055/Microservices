namespace Inventory.Application.Features.PurchaseOrders.Queries
{
    using Inventory.Application.PurchaseOrders.DTOs;
    using MediatR;

    public class GetDateRangePurchaseOrdersQuery : IRequest<PagedResponse<PurchaseOrderDto>>
    {
        public GetPurchaseOrdersRequest Request { get; set; } // Ye line honi chahiye
        public GetDateRangePurchaseOrdersQuery(GetPurchaseOrdersRequest request) => Request = request;
    }
}
