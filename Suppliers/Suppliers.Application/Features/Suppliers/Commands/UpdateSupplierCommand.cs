using MediatR;

public record UpdateSupplierCommand(int Id, CreateSupplierDto SupplierData) : IRequest<bool>;