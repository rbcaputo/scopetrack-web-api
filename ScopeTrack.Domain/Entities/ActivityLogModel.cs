using ScopeTrack.Domain.Enums;

namespace ScopeTrack.Domain.Entities
{
  public sealed class ActivityLogModel
  {
    public Guid Id { get; private set; }
    public ActivityEntityType EntityType { get; private set; }
    public Guid EntityId { get; private set; }
    public ActivityType ActivityType { get; private set; }
    public string Description { get; private set; }
    public DateTime Timestamp { get; private set; }

    private ActivityLogModel() { Description = string.Empty; } // EF Core

    public ActivityLogModel(
      ActivityEntityType entityType,
      Guid entityId,
      ActivityType activityType,
      string description
    )
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(entityId.ToString(), nameof(entityId));
      ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));

      Id = Guid.NewGuid();
      EntityType = entityType;
      EntityId = entityId;
      ActivityType = activityType;
      Description = description;
      Timestamp = DateTime.UtcNow;
    }
  }
}
