using MediatR;

namespace Inventory.Application.Categories.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandHandler
    : IRequestHandler<UpdateCategoryCommand>
{
    private readonly ICategoryRepository _repository;

    public UpdateCategoryCommandHandler(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await _repository.GetByIdAsync(request.Id);
        if (category is null)
            throw new KeyNotFoundException("Category not found");

        category.Update(
            request.CategoryCode,
            request.CategoryName,
            request.DefaultGst,
            request.Description,
            request.IsActive
        );

        await _repository.UpdateAsync(category);
    }
}
