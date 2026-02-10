using Company.Application.DTOs;
using MediatR;

namespace Company.Application.Company.Commands.Update
{
    public record UpdateCompanyCommand(int Id, UpsertCompanyRequest Request) : IRequest<int>;
}