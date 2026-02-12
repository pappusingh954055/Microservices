using AutoMapper;
using Inventory.Application.Common.Interfaces;
using Inventory.Application.Features.PurchaseOrders.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore; // Added for Include logic

namespace Inventory.Application.Features.PurchaseOrders.Handlers
{
    public class GetDateRangePurchaseOrdersQueryHandler : IRequestHandler<GetDateRangePurchaseOrdersQuery, PagedResponse<PurchaseOrderDto>>
    {
        private readonly IPurchaseOrderRepository _repo;
        private readonly IInventoryDbContext _context; // Context added to fetch GRN summaries

        public GetDateRangePurchaseOrdersQueryHandler(
            IPurchaseOrderRepository repo, 
            IInventoryDbContext context)
        {
            _repo = repo;
            _context = context;
        }

        public async Task<PagedResponse<PurchaseOrderDto>> Handle(GetDateRangePurchaseOrdersQuery query, CancellationToken ct)
        {
            // 1. PO Data fetch ho raha hai
            var result = await _repo.GetDateRangePagedOrdersAsync(query.Request);

            // 2. Mapping with GRN Summary Logic
            var dtos = result.Data.Select(x => new PurchaseOrderDto
            {
                Id = x.Id,
                PoNumber = x.PoNumber,
                SupplierName = x.SupplierName,
                PoDate = x.PoDate,
                TotalTax = x.TotalTax,
                GrandTotal = x.GrandTotal,
                SubTotal = x.SubTotal,
                ExpectedDeliveryDate = x.ExpectedDeliveryDate,
                CreatedBy = x.CreatedBy,
                CreatedDate = x.CreatedDate ?? DateTime.MinValue,
                UpdatedDate = x.UpdatedDate,
                Remarks = x.Remarks,
                Status = (x.GrnHeaders != null && x.GrnHeaders.Any())
                         ? (x.Items.All(i => i.ReceivedQty >= i.Qty) ? "Received" : "Partially Received")
                         : x.Status,

                // Items mapping with extra fields
                Items = x.Items.Select(item => {
                    // Summarizing from GRN Details for this specific PO and Product
                    var grnSummary = _context.GRNDetails
                        .Where(gd => gd.ProductId == item.ProductId && gd.GRNHeader.PurchaseOrderId == x.Id)
                        .Select(gd => new { gd.RejectedQty, gd.AcceptedQty })
                        .ToList();

                    return new PurchaseOrderItemDto
                    {
                        Id = item.Id,
                        Qty = item.Qty, // Ordered Qty
                        Unit = item.Unit,
                        Rate = item.Rate,
                        Total = item.Total,
                        TaxAmount = item.TaxAmount,
                        DiscountPercent = item.DiscountPercent,
                        GstPercent = item.GstPercent,
                        ProductName = item.Product != null ? item.Product.Name : "N/A",

                        // Cumulative fields calculation
                        ReceivedQty = item.ReceivedQty,

                        // NAYA: Accepted aur Rejected ka sum GRN details se
                        AcceptedQty = grnSummary.Sum(s => s.AcceptedQty),
                        RejectedQty = grnSummary.Sum(s => s.RejectedQty),

                        // NAYA: Pending Logic (Ordered - Received)
                        PendingQty = item.Qty - (item.ReceivedQty)
                    };
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