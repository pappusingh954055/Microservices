using Identity.Application.Common;
using MediatR;

namespace Identity.Application.Commands.Logout;

public record LogoutCommand(
    Guid UserId,
    string RefreshToken
) : IRequest<Result<bool>>;
