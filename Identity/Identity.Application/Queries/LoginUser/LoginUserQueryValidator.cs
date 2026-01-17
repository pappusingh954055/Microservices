using FluentValidation;

namespace Identity.Application.Queries.LoginUser;

public class LoginUserQueryValidator
    : AbstractValidator<LoginUserQuery>
{
    public LoginUserQueryValidator()
    {
        RuleFor(x => x.Dto.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Dto.Password)
            .NotEmpty();
    }
}
