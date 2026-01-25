// Application/Commands/BulkDeletePOItemsCommand.cs
using MediatR;

public record BulkDeletePOItemsCommand(int PurchaseOrderId, List<int> ItemIds) : IRequest<bool>;