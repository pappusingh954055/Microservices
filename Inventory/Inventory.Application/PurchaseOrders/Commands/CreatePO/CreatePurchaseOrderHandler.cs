using Inventory.Application.Common.Interfaces;
using Inventory.Application.PurchaseOrders.Queries.GetNextPoNumber;
using MediatR;

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
        // Fix: Ensure IInventoryDbContext has Database property
        using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            var dto = request.PoData;

            // Calling your existing PO generation logic
            string generatedPoNumber = await _mediator.Send(new GetNextPoNumberQuery(), ct);

            var po = new PurchaseOrder
            {
                PoNumber = generatedPoNumber,
                SupplierId = dto.SupplierId,
                SupplierName= dto.SupplierName,
                PriceListId = dto.PriceListId,
                PoDate = dto.PoDate,
                TotalTax = dto.TotalTax,
                GrandTotal = dto.GrandTotal,
                CreatedBy = dto.CreatedBy,
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
    }
}