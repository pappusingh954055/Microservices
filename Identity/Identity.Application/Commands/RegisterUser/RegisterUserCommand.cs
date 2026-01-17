using Identity.Application.DTOs;
using MediatR;

namespace Identity.Application.Commands.RegisterUser;

public record RegisterUserCommand(RegisterUserDto Dto)
    : IRequest<Guid>;
