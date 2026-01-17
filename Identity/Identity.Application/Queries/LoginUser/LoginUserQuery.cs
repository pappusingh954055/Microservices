using Identity.Application.Common;
using Identity.Application.DTOs;
using MediatR;

namespace Identity.Application.Queries.LoginUser;

public record LoginUserQuery(LoginDto Dto)
    : IRequest<Result<AuthResponse>>;
