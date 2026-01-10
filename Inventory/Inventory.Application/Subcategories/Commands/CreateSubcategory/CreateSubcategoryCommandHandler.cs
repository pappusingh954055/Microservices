using MediatR;
using Inventory.Application.Common.Interfaces;
using Inventory.Domain.Entities;

namespace Inventory.Application.Subcategories.Commands.CreateSubcategory;

public sealed class CreateSubcategoryCommandHandler
    : IRequestHandler<CreateSubcategoryCommand, Guid>
{
    private readonly ISubcategoryRepository _repository;

    public CreateSubcategoryCommandHandler(ISubcategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(
        CreateSubcategoryCommand request,
        CancellationToken cancellationToken)
    {
        var subcategory = new Subcategory(
            request.CategoryId,
            request.SubcategoryCode,
            request.SubcategoryName,
            request.DefaultGst,
            request.Description
        );

        await _repository.AddAsync(subcategory);

        return subcategory.Id;
    }
}
