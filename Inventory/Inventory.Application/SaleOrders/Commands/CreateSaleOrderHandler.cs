using Inventory.Application.Common.Interfaces;
using Inventory.Application.SaleOrders.Commands;
using Inventory.Application.Clients;
using Inventory.Application.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Inventory.Domain.Entities.SO;

public class CreateSaleOrderHandler : IRequestHandler<CreateSaleOrderCommand, object>
{
    private readonly ISaleOrderRepository _repo;
    private readonly IInventoryDbContext _context;
    private readonly ICustomerClient _customerClient;
    private readonly ICompanyClient _companyClient;
    private readonly IEmailService _emailService;
    private readonly IWhatsAppService _whatsAppService;

    public CreateSaleOrderHandler(
        ISaleOrderRepository repo, 
        IInventoryDbContext context, 
        ICustomerClient customerClient,
        ICompanyClient companyClient,
        IEmailService emailService,
        IWhatsAppService whatsAppService)
    {
        _repo = repo;
        _context = context;
        _customerClient = customerClient;
        _companyClient = companyClient;
        _emailService = emailService;
        _whatsAppService = whatsAppService;
    }

    public async Task<object> Handle(CreateSaleOrderCommand request, CancellationToken cancellationToken)
    {
        var dto = request.OrderDto;

        // 1. SONumber Generate Karein
        string lastNo = await _repo.GetLastSONumberAsync();
        int nextId = lastNo == null ? 1 : int.Parse(lastNo.Split('-').Last()) + 1;
        string generatedSONo = $"SO-{DateTime.Now.Year}-{nextId:D4}";

        // 2. SaleOrder Object Mapping
        var saleOrder = new SaleOrder
        {
            SONumber = generatedSONo,
            CustomerId = dto.CustomerId,
            SODate = dto.SoDate,
            ExpectedDeliveryDate = dto.ExpectedDeliveryDate,
            SubTotal = dto.SubTotal,
            TotalTax = dto.TotalTax,
            GrandTotal = dto.GrandTotal,
            Remarks = dto.Remarks,
            Status = dto.Status,
            CreatedBy = dto.CreatedBy,
            Items = dto.Items.Select(i => new SaleOrderItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Qty = i.Qty,
                Unit = i.Unit,
                Rate = i.Rate,
                DiscountPercent = i.DiscountPercent,
                GSTPercent = i.GstPercent,
                TaxAmount = i.TaxAmount,
                Total = i.Total
            }).ToList()
        };

        object result;

        // 3. Conditional Logic: Confirm & Reduce Stock vs Save as Draft
        if (dto.Status == "Confirmed")
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            result = await strategy.ExecuteAsync(async () =>
            {
                await _repo.BeginTransactionAsync();
                try
                {
                    var savedId = await _repo.SaveAsync(saleOrder);

                    foreach (var item in saleOrder.Items)
                    {
                        decimal availableStock = await _repo.GetAvailableStockAsync(item.ProductId);
                        if (availableStock < item.Qty)
                        {
                            throw new Exception($"Insufficient stock for {item.ProductName}. Available: {availableStock}");
                        }
                        await _repo.UpdateProductStockAsync(item.ProductId, -item.Qty);
                    }

                    await _repo.CommitTransactionAsync();

                    // Ledger Sync (Fire and forget or async)
                    try
                    {
                        await _customerClient.RecordSaleAsync(
                            saleOrder.CustomerId,
                            saleOrder.GrandTotal,
                            saleOrder.SONumber,
                            $"Sale Invoice generated: {saleOrder.SONumber}",
                            saleOrder.CreatedBy ?? "System"
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ledger sync failed: {ex.Message}");
                    }

                    return new { Id = savedId, SONumber = generatedSONo };
                }
                catch (Exception)
                {
                    await _repo.RollbackTransactionAsync();
                    throw;
                }
            });
        }
        else
        {
            var savedId = await _repo.SaveAsync(saleOrder);
            result = new { Id = savedId, SONumber = generatedSONo };
        }

        // 4. Notifications (Email & WhatsApp)
        if (result != null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var company = await _companyClient.GetCompanyProfileAsync();
                    var customer = await _customerClient.GetCustomerByIdAsync(saleOrder.CustomerId);

                    if (company != null && customer != null)
                    {
                        // 1. Email
                        if (!string.IsNullOrEmpty(customer.Email))
                        {
                            await _emailService.SendSoEmailAsync(company, customer.Email, generatedSONo, saleOrder.GrandTotal);
                        }

                        // 2. WhatsApp
                        if (!string.IsNullOrEmpty(customer.Phone))
                        {
                            string msg = $"Order Confirmed! 🚀\nFrom: {company.Name}\nOrder No: {generatedSONo}\nAmount: {saleOrder.GrandTotal}\nThank you for shopping with us!";
                            await _whatsAppService.SendMessageAsync(customer.Phone, msg);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CreateSaleOrderHandler] Notification task failed: {ex.Message}");
                }
            }, cancellationToken);
        }

        return result;
    }
}