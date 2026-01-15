namespace Inventory.Application.PurchaseOrders.Commands.Create
{
    public sealed class CreatePurchaseOrderItemDto
    {
        public Guid ProductId { get; init; }
        public decimal Quantity { get; init; }
        public decimal UnitPrice { get; init; }
        public decimal DiscountPercent { get; init; }
        public decimal GstPercent { get; init; }
    }
}
