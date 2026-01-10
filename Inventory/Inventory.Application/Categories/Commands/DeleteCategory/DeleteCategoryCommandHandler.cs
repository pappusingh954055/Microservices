using Inventory.Application.Common.Interfaces;
using MediatR;

namespace Inventory.Application.Categories.Commands.DeleteCategory;

internal sealed class DeleteCategoryCommandHandler
    : IRequestHandler<DeleteCategoryCommand, Guid>
{
    private readonly ICategoryRepository _repository;
    private readonly IInventoryDbContext _context;

    public DeleteCategoryCommandHandler(
        ICategoryRepository repository,
        IInventoryDbContext context)
    {
        _repository = repository;
        _context = context;
    }

    public async Task<Guid> Handle(
        DeleteCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await _repository.GetByIdAsync(request.Id);

        if (category is null)
            throw new KeyNotFoundException("Category not found");

       await _repository.DeleteAsync(category);

        await _context.SaveChangesAsync(cancellationToken);

        return request.Id;
    }
}
