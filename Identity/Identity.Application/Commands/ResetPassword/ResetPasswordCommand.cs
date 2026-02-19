using Identity.Application.Common;
using MediatR;

namespace Identity.Application.Commands.ResetPassword;

public record ResetPasswordCommand(string ResetToken, string NewPassword) : IRequest<Result<bool>>;
