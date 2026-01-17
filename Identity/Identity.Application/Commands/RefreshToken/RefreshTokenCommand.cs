using Identity.Application.Common;
using Identity.Application.DTOs;
using MediatR;

public record RefreshTokenCommand(string RefreshToken)
    : IRequest<Result<AuthResponse>>;
