using MediatR;

public record CreateSupplierCommand(CreateSupplierDto SupplierData) : IRequest<int>;