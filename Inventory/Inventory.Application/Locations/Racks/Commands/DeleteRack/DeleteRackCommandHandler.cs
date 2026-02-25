using Inventory.Application.Common.Interfaces;
using MediatR;

namespace Inventory.Application.Locations.Racks.Commands.DeleteRack;

public sealed class DeleteRackCommandHandler : IRequestHandler<DeleteRackCommand, MediatR.Unit>
{
    private readonly IRackRepository _repository;
    private readonly IInventoryDbContext _context;

    public DeleteRackCommandHandler(IRackRepository repository, IInventoryDbContext context)
    {
        _repository = repository;
        _context = context;
    }

    public async Task<MediatR.Unit> Handle(DeleteRackCommand request, CancellationToken cancellationToken)
    {
        var rack = await _repository.GetByIdAsync(request.Id);

        if (rack == null)
        {
            throw new Exception("Rack not found");
        }

        await _repository.DeleteAsync(rack);
        await _context.SaveChangesAsync(cancellationToken);

        return MediatR.Unit.Value;
    }
}
