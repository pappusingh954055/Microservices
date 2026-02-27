using Inventory.Application.Clients;
using Inventory.Application.Common.Interfaces;
using Inventory.Application.GRN.Command;
using Inventory.Application.Services;
using Inventory.Domain.Entities;
using MediatR;
using System;
using System.Threading.Tasks;

public class CreateGRNHandler : IRequestHandler<CreateGRNCommand, string>
{
    private readonly IGRNRepository _repo;
    private readonly IPurchaseOrderRepository _poRepo;
    private readonly IEmailService _emailService;
    private readonly IWhatsAppService _whatsAppService;
    private readonly ICompanyClient _companyClient;
    private readonly ISupplierClient _supplierClient;

    public CreateGRNHandler(
        IGRNRepository repo,
        IPurchaseOrderRepository poRepo,
        IEmailService emailService,
        IWhatsAppService whatsAppService,
        ICompanyClient companyClient,
        ISupplierClient supplierClient)
    {
        _repo = repo;
        _poRepo = poRepo;
        _emailService = emailService;
        _whatsAppService = whatsAppService;
        _companyClient = companyClient;
        _supplierClient = supplierClient;
    }

    public async Task<string> Handle(CreateGRNCommand request, CancellationToken ct)
    {
        var dto = request.Data;

        var header = new GRNHeader
        {
            GRNNumber = "AUTO-GEN",
            PurchaseOrderId = dto.POHeaderId,
            SupplierId = dto.SupplierId,
            ReceivedDate = dto.ReceivedDate,
            GatePassNo = dto.GatePassNo,
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
            DiscountPercent = i.DiscountPercent,
            GstPercent = i.GstPercent,
            TaxAmount = i.TaxAmount,
            Total = i.TotalAmount,
            WarehouseId = i.WarehouseId,
            RackId = i.RackId,
            UpdatedOn = DateTime.Now
        }).ToList();

        var grnNumber = await _repo.SaveGRNWithStockUpdate(header, details);

        if (!string.IsNullOrEmpty(grnNumber))
        {
            // Background Task for notifications
            _ = Task.Run(async () =>
            {
                try
                {
                    var company = await _companyClient.GetCompanyProfileAsync();
                    var supplier = await _supplierClient.GetSupplierByIdAsync(dto.SupplierId);
                    var po = await _poRepo.GetByIdAsync(dto.POHeaderId);
                    string poNumber = po?.PoNumber ?? "N/A";

                    if (company != null && supplier != null)
                    {
                        // 1. Email
                        if (!string.IsNullOrEmpty(supplier.Email))
                        {
                            await _emailService.SendGrnEmailAsync(company, supplier.Email, grnNumber, poNumber, dto.TotalAmount);
                        }

                        // 2. WhatsApp
                        if (!string.IsNullOrEmpty(supplier.Phone))
                        {
                            string msg = $"Goods Received: {grnNumber}\nRef PO: {poNumber}\nSource: {company.Name}\nStatus: Received & Accepted.\nThank you!";
                            await _whatsAppService.SendMessageAsync(supplier.Phone, msg);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CreateGRNHandler] Notification failed: {ex.Message}");
                }
            }, ct);
        }

        return grnNumber;
    }
}