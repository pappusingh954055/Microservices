using FluentValidation;

namespace Identity.Application.Commands.RegisterUser;

public class RegisterUserCommandValidator
    : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Dto.UserName)
            .NotEmpty()
            .MinimumLength(3);

        RuleFor(x => x.Dto.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Dto.Password)
            .NotEmpty()
            .MinimumLength(6);
    }
}
