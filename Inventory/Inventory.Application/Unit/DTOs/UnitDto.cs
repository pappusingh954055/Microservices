public record UnitDto(int Id, string Name, string Description, bool IsActive);
public record CreateUnitDto(string Name, string Description);