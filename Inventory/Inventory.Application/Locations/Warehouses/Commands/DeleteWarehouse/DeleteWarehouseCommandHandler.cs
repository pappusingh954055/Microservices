using Inventory.Application.Common.Interfaces;
using MediatR;

namespace Inventory.Application.Locations.Warehouses.Commands.DeleteWarehouse;

public sealed class DeleteWarehouseCommandHandler : IRequestHandler<DeleteWarehouseCommand, Unit>
{
    private readonly IWarehouseRepository _repository;
    private readonly IInventoryDbContext _context;

    public DeleteWarehouseCommandHandler(IWarehouseRepository repository, IInventoryDbContext context)
    {
        _repository = repository;
        _context = context;
    }

    public async Task<Unit> Handle(DeleteWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await _repository.GetByIdAsync(request.Id);

        if (warehouse == null)
        {
            throw new Exception("Warehouse not found");
        }

        await _repository.DeleteAsync(warehouse);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
