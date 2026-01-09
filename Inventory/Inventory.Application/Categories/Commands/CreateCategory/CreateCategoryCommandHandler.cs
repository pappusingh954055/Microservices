using MediatR;

using Inventory.Domain.Entities;

namespace Inventory.Application.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandHandler
    : IRequestHandler<CreateCategoryCommand, Guid>
{
    private readonly ICategoryRepository _repository;

    public CreateCategoryCommandHandler(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = new Category(
            request.CategoryName,
            request.CategoryCode,
            request.DefaultGst,
            request.Description,
            request.IsActive
        );

        await _repository.AddAsync(category);

        return category.Id;
    }
}
