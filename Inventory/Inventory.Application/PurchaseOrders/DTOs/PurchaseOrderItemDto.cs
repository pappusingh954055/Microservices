namespace Application.DTOs;

// Item level data (Table Rows)
public record PurchaseOrderItemDto(
    Guid productId,
    int qty,
    decimal price,
    decimal discountPercent,
    decimal gstPercent,
    decimal total
);

// Full PO data structure
public record PurchaseOrderDto(
    int supplierId,
    string poNumber,
    DateTime poDate,
    DateTime? expectedDeliveryDate,
    string referenceNumber,
    string remarks,
    List<PurchaseOrderItemDto> items
);