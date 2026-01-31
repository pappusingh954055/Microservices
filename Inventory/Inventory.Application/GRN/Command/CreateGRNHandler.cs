using Inventory.Application.Common.Interfaces;
using Inventory.Application.GRN.Command;
using Inventory.Domain.Entities;
using MediatR;

public class CreateGRNHandler : IRequestHandler<CreateGRNCommand, string>
{
    private readonly IGRNRepository _repo;
    public CreateGRNHandler(IGRNRepository repo) => _repo = repo;

    // Task<bool> ko badal kar Task<string> kiya
    public async Task<string> Handle(CreateGRNCommand request, CancellationToken ct)
    {
        var dto = request.Data;

        var header = new GRNHeader
        {
            // Yahan "AUTO-GEN" bhej rahe hain taaki Repo ise generate kare, 
            // ya phir yahan seedha generate karwa lein
            GRNNumber = "AUTO-GEN",
            PurchaseOrderId = dto.POHeaderId,
            SupplierId = dto.SupplierId,
            ReceivedDate = dto.ReceivedDate,
            TotalAmount = dto.TotalAmount,
            Remarks = dto.Remarks,
            CreatedBy = dto.CreatedBy,
            Status = "Received", // SQL Null Constraint Fix
            UpdatedOn = DateTime.Now
        };

        var details = dto.Items.Select(i => new GRNDetail
        {
            ProductId = i.ProductId,
            OrderedQty = i.OrderedQty,
            ReceivedQty = i.ReceivedQty,
            RejectedQty = i.RejectedQty,
            AcceptedQty = i.AcceptedQty,
            UnitRate = i.UnitRate,
            UpdatedOn = DateTime.Now
        }).ToList();

        // Ab ye return boolean nahi, balki generated GRN string hoga
        return await _repo.SaveGRNWithStockUpdate(header, details);
    }
}