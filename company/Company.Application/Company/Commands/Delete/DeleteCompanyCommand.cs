using MediatR;

namespace Company.Application.Company.Commands.Delete
{
    public record DeleteCompanyCommand(int Id) : IRequest<bool>;
}