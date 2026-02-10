using Company.Application.Common.Interfaces;
using MediatR;

namespace Company.Application.Company.Commands.Delete.Handler
{
    public class DeleteCompanyHandler : IRequestHandler<DeleteCompanyCommand, bool>
    {
        private readonly ICompanyRepository _repo;
        public DeleteCompanyHandler(ICompanyRepository repo) => _repo = repo;

        public async Task<bool> Handle(DeleteCompanyCommand cmd, CancellationToken ct)
        {
            // Repository method call
            return await _repo.DeleteCompanyProfileAsync(cmd.Id);
        }
    }
}