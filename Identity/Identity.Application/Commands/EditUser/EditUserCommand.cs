using Identity.Application.Common;
using MediatR;

namespace Identity.Application.Commands.EditUser;

public record EditUserCommand(
    Guid Id,
    string UserName,
    string Email,
    string? Password,
    bool IsActive,
    List<int> RoleIds
) : IRequest<Result<Guid>>;
