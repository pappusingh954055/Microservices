using MediatR;
using Inventory.Application.Common.Interfaces;

namespace Inventory.Application.Subcategories.Commands.UpdateSubcategory;

public sealed class UpdateSubcategoryCommandHandler
    : IRequestHandler<UpdateSubcategoryCommand>
{
    private readonly ISubcategoryRepository _repository;

    public UpdateSubcategoryCommandHandler(ISubcategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(
        UpdateSubcategoryCommand request,
        CancellationToken cancellationToken)
    {
        var subcategory = await _repository.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException("Subcategory not found");

        subcategory.Update(
            request.SubcategoryCode,
            request.SubcategoryName,
            request.DefaultGst,
            request.Description,
            request.IsActive
        );

        await _repository.UpdateAsync(subcategory);
    }
}
