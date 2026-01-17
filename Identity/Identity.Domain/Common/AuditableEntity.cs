namespace Identity.Domain.Common;

public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; protected set; }

    public void SetModified()
    {
        ModifiedAt = DateTime.UtcNow;
    }
}
