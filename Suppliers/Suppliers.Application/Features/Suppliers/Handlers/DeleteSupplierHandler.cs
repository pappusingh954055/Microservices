using MediatR;

public class DeleteSupplierHandler : IRequestHandler<DeleteSupplierCommand, bool>
{
    private readonly ISupplierRepository _repository;

    public DeleteSupplierHandler(ISupplierRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _repository.GetByIdAsync(request.Id);
        if (supplier == null) return false;

        // DDD: Entity method for soft delete
        supplier.Deactivate();

        await _repository.UpdateAsync(supplier);
        return true;
    }
}