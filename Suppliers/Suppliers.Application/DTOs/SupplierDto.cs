public record SupplierDto(
    int id,
    string name,
    string phone,
    string? gstIn,
    string? address,
    string? email,
    bool? isActive,
    string? createdBy,
    Guid? defaultpricelistId
);

public record CreateSupplierDto(
    string name,
    string phone,
    string? gstIn,
    string? address,
    string? email,
    string? createdBy,
    Guid? defaultpricelistId,
    bool isActive
);