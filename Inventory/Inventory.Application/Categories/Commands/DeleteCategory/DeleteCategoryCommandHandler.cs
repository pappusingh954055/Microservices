using MediatR;

namespace Inventory.Application.Categories.Commands.DeleteCategory;

public sealed class DeleteCategoryCommandHandler
    : IRequestHandler<DeleteCategoryCommand>
{
    private readonly ICategoryRepository _repository;

    public DeleteCategoryCommandHandler(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(
        DeleteCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await _repository.GetByIdAsync(request.Id);
        if (category is null)
            throw new KeyNotFoundException("Category not found");

        await _repository.DeleteAsync(category);
    }
}
