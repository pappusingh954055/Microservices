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
            request.SupplierData.Name,
            request.SupplierData.Phone,
            request.SupplierData.GstIn,
            request.SupplierData.Address,
            request.SupplierData.CreatedBy
        );

        await _repository.AddAsync(supplier);
        return supplier.Id;
    }
}