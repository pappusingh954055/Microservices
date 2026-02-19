using Identity.Application.Common;
using MediatR;

namespace Identity.Application.Commands.ChangePassword;

public record ChangePasswordCommand(
    string Email,
    string OldPassword,
    string NewPassword
) : IRequest<Result<bool>>;
