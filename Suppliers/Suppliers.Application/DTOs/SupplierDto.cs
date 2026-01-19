public record SupplierDto(
    int Id,
    string Name,
    string Phone,
    string? GstIn,
    string? Address,
    bool? IsActive,
    string? CreatedBy);

public record CreateSupplierDto(
    string Name,
    string Phone,
    string? GstIn,
    string? Address,
    string? CreatedBy);