namespace StudentStudyHub.Domain.Common;

/// <summary>
/// Base class for all domain entities, providing a strongly-typed identity
/// and basic audit timestamps shared across the aggregate roots.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
