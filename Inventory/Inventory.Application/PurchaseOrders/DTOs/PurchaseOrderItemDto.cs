public class PurchaseOrderItemDto
{
    public int Id { get; set; }
    public int PurchaseOrderId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Qty { get; set; }
    public string Unit { get; set; }
    public decimal Rate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal GstPercent { get; set; }

    // Manual Mapping from Entity to DTO
    public static PurchaseOrderItemDto FromEntity(dynamic entity)
    {
        return new PurchaseOrderItemDto
        {
            Id = entity.Id,
            PurchaseOrderId = entity.PurchaseOrderId,
            ProductId = entity.ProductId,
            Qty = entity.Qty,
            Unit = entity.Unit,
            Rate = entity.Rate,
            TaxAmount = entity.TaxAmount,
            Total = entity.Total,
            DiscountPercent = entity.DiscountPercent,
            GstPercent = entity.GstPercent
        };
    }
}