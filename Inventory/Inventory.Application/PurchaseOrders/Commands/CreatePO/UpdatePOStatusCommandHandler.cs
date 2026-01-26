using Inventory.Application.Common.Interfaces;
using MediatR;

public class UpdatePOStatusHandler : IRequestHandler<UpdatePOStatusCommand, bool>
{
    private readonly IPurchaseOrderRepository _repository;

    public UpdatePOStatusHandler(IPurchaseOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(UpdatePOStatusCommand request, CancellationToken cancellationToken)
    {
        return await _repository.UpdatePOStatusAsync(request.Id, request.Status);
    }
}