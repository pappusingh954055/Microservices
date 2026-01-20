using MediatR;

public class CreateSupplierHandler : IRequestHandler<CreateSupplierCommand, int>
{
    private readonly ISupplierRepository _repository;

    public CreateSupplierHandler(ISupplierRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = new Supplier(
            request.SupplierData.name,
            request.SupplierData.phone,
            request.SupplierData.gstIn,
            request.SupplierData.address,
            request.SupplierData.createdBy,
            request.SupplierData.isActive,
            request.SupplierData.defaultpricelistId
            
        );

        await _repository.AddAsync(supplier);
        return supplier.Id;
    }
}