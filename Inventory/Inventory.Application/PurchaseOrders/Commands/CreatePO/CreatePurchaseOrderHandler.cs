using Inventory.Application.Clients;
using Inventory.Application.Common.Interfaces;
using Inventory.Application.PurchaseOrders.Queries.GetNextPoNumber;
using Inventory.Application.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class CreatePurchaseOrderCommandHandler : IRequestHandler<CreatePurchaseOrderCommand, bool>
{
    private readonly IInventoryDbContext _context;
    private readonly IPurchaseOrderRepository _repo;
    private readonly IMediator _mediator;
    private readonly IEmailService _emailService;
    private readonly IWhatsAppService _whatsAppService;
    private readonly ICompanyClient _companyClient;
    private readonly ISupplierClient _supplierClient;

    public CreatePurchaseOrderCommandHandler(
        IInventoryDbContext context, 
        IPurchaseOrderRepository repo, 
        IMediator mediator,
        IEmailService emailService,
        IWhatsAppService whatsAppService,
        ICompanyClient companyClient,
        ISupplierClient supplierClient)
    {
        _context = context;
        _repo = repo;
        _mediator = mediator;
        _emailService = emailService;
        _whatsAppService = whatsAppService;
        _companyClient = companyClient;
        _supplierClient = supplierClient;
    }

    public async Task<bool> Handle(CreatePurchaseOrderCommand request, CancellationToken ct)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        string? finalPoNumber = null;

        var result = await strategy.ExecuteAsync(async () =>
        {
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
                finalPoNumber = generatedPoNumber;
                return true;
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        });

        if (result && finalPoNumber != null)
        {
            // Background Task to send notifications
            _ = Task.Run(async () =>
            {
                try
                {
                    var company = await _companyClient.GetCompanyProfileAsync();
                    var supplier = await _supplierClient.GetSupplierByIdAsync(request.PoData.SupplierId);

                    if (company != null && supplier != null)
                    {
                        // 1. Email
                        if (!string.IsNullOrEmpty(supplier.Email))
                        {
                            await _emailService.SendPoEmailAsync(company, supplier.Email, finalPoNumber, request.PoData.GrandTotal);
                        }

                        // 2. WhatsApp
                        if (!string.IsNullOrEmpty(supplier.Phone))
                        {
                            string msg = $"New Purchase Order from {company.Name}:\nPO Number: {finalPoNumber}\nAmount: {request.PoData.GrandTotal}\nPlease check your email for details.";
                            await _whatsAppService.SendMessageAsync(supplier.Phone, msg);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CreatePurchaseOrderHandler] Notification task failed: {ex.Message}");
                }
            }, ct);
        }

        return result;
    }
}
