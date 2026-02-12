using Inventory.Application.Common.Interfaces;
using Inventory.Application.GRN.Command;
using Inventory.Domain.Entities;
using MediatR;

public class CreateGRNHandler : IRequestHandler<CreateGRNCommand, string>
{
    private readonly IGRNRepository _repo;
    public CreateGRNHandler(IGRNRepository repo) => _repo = repo;

    public async Task<string> Handle(CreateGRNCommand request, CancellationToken ct)
    {
        var dto = request.Data;

        var header = new GRNHeader
        {
            GRNNumber = "AUTO-GEN",
            PurchaseOrderId = dto.POHeaderId,
            SupplierId = dto.SupplierId,
            ReceivedDate = dto.ReceivedDate,
            TotalAmount = dto.TotalAmount,
            Remarks = dto.Remarks,
            CreatedBy = dto.CreatedBy,
            Status = "Received",
            UpdatedOn = DateTime.Now
        };

        var details = dto.Items.Select(i => new GRNDetail
        {
            ProductId = i.ProductId,
            OrderedQty = i.OrderedQty,
            PendingQty = i.PendingQty,
            ReceivedQty = i.ReceivedQty,
            RejectedQty = i.RejectedQty,
            AcceptedQty = i.AcceptedQty,
            UnitRate = i.UnitRate,
            UpdatedOn = DateTime.Now
        }).ToList();

        return await _repo.SaveGRNWithStockUpdate(header, details);
    }
}