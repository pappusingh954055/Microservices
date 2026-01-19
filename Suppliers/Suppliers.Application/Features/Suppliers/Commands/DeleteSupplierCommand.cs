using MediatR;

public record DeleteSupplierCommand(int Id) : IRequest<bool>;