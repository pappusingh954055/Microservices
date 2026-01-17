
using Identity.Application.Commands.RegisterUser;
using Identity.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;

public class RegisterUserHandler
    : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IUnitOfWork _uow;

    public RegisterUserHandler(
        IUserRepository users,
        IRoleRepository roles,
        IPasswordHasher<User> passwordHasher,
        IUnitOfWork uow)
    {
        _users = users;
        _roles = roles;
        _passwordHasher = passwordHasher;
        _uow = uow;
    }

    public async Task<Guid> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        if (await _users.ExistsByEmailAsync(dto.Email))
            throw new InvalidOperationException("Email already exists");

        var role = await _roles.GetByNameAsync(dto.RoleName)
            ?? throw new InvalidOperationException("Invalid role");

        var user = new User(dto.UserName, dto.Email);

        // ✅ HASH PASSWORD HERE
        var hash = _passwordHasher.HashPassword(user, dto.Password);
        user.SetPasswordHash(hash);

        user.AssignRole(role.Id);

        await _users.AddAsync(user);
        await _uow.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
