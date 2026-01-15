namespace Inventory.Domain.Entities
{
    public class InventoryTransaction
    {
        public Guid Id { get; private set; }

        public Guid ProductId { get; private set; }
        public decimal Quantity { get; private set; }

        public string TransactionType { get; private set; } = null!;
        public Guid ReferenceId { get; private set; }

        public DateTime CreatedOn { get; private set; }

        protected InventoryTransaction() { }

        public InventoryTransaction(
            Guid productId,
            decimal quantity,
            string transactionType,
            Guid referenceId)
        {
            Id = Guid.NewGuid();
            ProductId = productId;
            Quantity = quantity;
            TransactionType = transactionType;
            ReferenceId = referenceId;
            CreatedOn = DateTime.UtcNow;
        }
    }
}
