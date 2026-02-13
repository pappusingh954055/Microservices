using Inventory.Application.Common.Interfaces;
using Inventory.Application.PurchaseOrders.Queries.GetNextPoNumber;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class CreatePurchaseOrderCommandHandler : IRequestHandler<CreatePurchaseOrderCommand, bool>
{
    private readonly IInventoryDbContext _context;
    private readonly IPurchaseOrderRepository _repo;
    private readonly IMediator _mediator;

    public CreatePurchaseOrderCommandHandler(IInventoryDbContext context, IPurchaseOrderRepository repo, IMediator mediator)
    {
        _context = context;
        _repo = repo;
        _mediator = mediator;
    }

    public async Task<bool> Handle(CreatePurchaseOrderCommand request, CancellationToken ct)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                var dto = request.PoData;

                // Calling your existing PO generation logic
                // Note: If retry happens, GetNextPoNumberQuery might be called again
                string generatedPoNumber = await _mediator.Send(new GetNextPoNumberQuery(), ct);

                var po = new PurchaseOrder
                {
                    PoNumber = generatedPoNumber,
                    SupplierId = dto.SupplierId,
                    SupplierName = dto.SupplierName,
                    PriceListId = dto.PriceListId,
                    PoDate = dto.PoDate,
                    TotalTax = dto.TotalTax,
                    SubTotal = dto.SubTotal,
                    GrandTotal = dto.GrandTotal,
                    CreatedBy = dto.CreatedBy,
                    Remarks = dto.Remarks,
                    ExpectedDeliveryDate = dto.ExpectedDeliveryDate,
                    Items = dto.Items.Select(i => new PurchaseOrderItem
                    {
                        ProductId = i.ProductId,
                        Qty = i.Qty,
                        Unit = i.Unit,
                        Rate = i.Rate,
                        DiscountPercent = i.DiscountPercent,
                        GstPercent = i.GstPercent,
                        TaxAmount = i.TaxAmount,
                        Total = i.Total
                    }).ToList()
                };

                await _repo.AddAsync(po, ct);
                await _context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return true;
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        });
    }
}