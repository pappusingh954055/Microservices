using MediatR;

public class UpdateSupplierHandler : IRequestHandler<UpdateSupplierCommand, bool>
{
    private readonly ISupplierRepository _repository;

    public UpdateSupplierHandler(ISupplierRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _repository.GetByIdAsync(request.Id);

        if (supplier == null) return false;

        // DDD: Entity method call
        supplier.UpdateDetails(
            request.SupplierData.Name,
            request.SupplierData.Phone,
            request.SupplierData.GstIn,
            request.SupplierData.Address);

        await _repository.UpdateAsync(supplier);
        return true;
    }
}