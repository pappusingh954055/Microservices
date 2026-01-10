using MediatR;
using Inventory.Application.Common.Interfaces;

namespace Inventory.Application.Subcategories.Commands.DeleteSubcategory;

public sealed class DeleteSubcategoryCommandHandler
    : IRequestHandler<DeleteSubcategoryCommand>
{
    private readonly ISubcategoryRepository _repository;

    private readonly IInventoryDbContext _context;

    public DeleteSubcategoryCommandHandler(ISubcategoryRepository repository, IInventoryDbContext context)
    {
        _repository = repository;
        _context = context;
    }

    public async Task Handle(
        DeleteSubcategoryCommand request,
        CancellationToken cancellationToken)
    {
        var subcategory = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException("Subcategory not found");

        await _repository.DeleteAsync(subcategory);

        await _context.SaveChangesAsync();
    }
}
