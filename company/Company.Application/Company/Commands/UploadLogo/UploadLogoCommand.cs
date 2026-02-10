using MediatR;

namespace Company.Application.Company.Commands.UploadLogo
{
    public record UploadLogoCommand(int Id, string LogoUrl) : IRequest<bool>;
}
