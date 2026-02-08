using Identity.Application.DTOs;
using MediatR;

namespace Identity.Application.Commands.RegisterUser;

public record RegisterUserCommand(
    string UserName,
    string Email,
    string Password,
    List<int> RoleIds
) : IRequest<Guid>;
