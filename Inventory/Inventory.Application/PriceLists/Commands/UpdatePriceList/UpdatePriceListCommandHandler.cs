using Inventory.Application.Common.Interfaces;
using Inventory.Application.PriceLists.Commands.UpdatePriceList;
using Inventory.Domain.PriceLists;
using MediatR;
using Microsoft.EntityFrameworkCore; // AnyAsync ke liye zaroori hai

public class UpdatePriceListCommandHandler : IRequestHandler<UpdatePriceListCommand, bool>
{
    private readonly IPriceListRepository _repository;
    private readonly IInventoryDbContext _context; // DB context validation ke liye

    public UpdatePriceListCommandHandler(IPriceListRepository repository, IInventoryDbContext context)
    {
        _repository = repository;
        _context = context;
    }

    public async Task<bool> Handle(UpdatePriceListCommand request, CancellationToken cancellationToken)
    {
        // 1. Existing entity fetch karein with items
        var entity = await _repository.GetByIdWithItemsAsync(request.id, cancellationToken);
        if (entity == null) return false;

        // 2. Inventory Validation Rule Check
        // Agar ye list Active hai (IsActive = true), toh hi duplicacy check karenge
        if (request.isActive)
        {
            foreach (var item in request.priceListItems)
            {
                // Check: Kya ye Product kisi aur ACTIVE list mein same PriceType ke saath hai?
                var isAlreadyActiveElsewhere = await _context.PriceListItems
                    .AnyAsync(pi => pi.ProductId == item.productId &&
                                    pi.PriceList.PriceType == request.priceType &&
                                    pi.PriceList.IsActive == true &&
                                    pi.PriceListId != request.id, cancellationToken); // Khud ko chhod kar

                if (isAlreadyActiveElsewhere)
                {
                    // Agar duplicate milta hai, toh error message throw karenge
                    throw new Exception($"Product ID {item.productId} is already assigned to another active '{request.priceType}' price list. You must deactivate the other list first.");
                }
            }
        }

        // 3. Header Update karein (Mapping UI fields)
        entity.Name = request.name;
        entity.Code = request.code;
        entity.PriceType = request.priceType;
        entity.ValidFrom = request.validFrom;
        entity.ValidTo = request.validTo;
        entity.IsActive = request.isActive;
        entity.Remarks = request.remarks;

        // 4. Child Items Update (Clear and Re-add Strategy)
        entity.PriceListItems.Clear();

        foreach (var item in request.priceListItems)
        {
            entity.PriceListItems.Add(new PriceListItem
            {
                PriceListId = entity.Id,
                ProductId = item.productId,
                Rate = item.rate,
                DiscountPercent = item.discountPercent,
                MinQty = item.minQty,
                MaxQty = item.maxQty,
                Unit = item.unit
            });
        }

        // 5. Atomic Update Execution
        await _repository.UpdatePriceListAsync(entity, cancellationToken);
        return true;
    }
}