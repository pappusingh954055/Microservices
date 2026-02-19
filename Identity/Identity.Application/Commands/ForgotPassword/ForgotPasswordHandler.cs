using Identity.Application.Common;
using Identity.Application.Interfaces;
using MediatR;
using System.Security.Cryptography;

namespace Identity.Application.Commands.ForgotPassword;

public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, Result<string>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ForgotPasswordHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
            return Result<string>.Failure("User not found");

        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        string token = Convert.ToBase64String(tokenBytes);
        var expiry = DateTime.UtcNow.AddMinutes(15);

        user.SetResetToken(token, expiry);

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // In production, send token via email
        // For development, return the token so it can be used for testing
        return Result<string>.Success(token);
    }
}
