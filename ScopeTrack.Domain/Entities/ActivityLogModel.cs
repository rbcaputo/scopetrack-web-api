using ScopeTrack.Domain.Enums;

namespace ScopeTrack.Domain.Entities
{
  public sealed class ActivityLogModel
  {
    public Guid ID { get; private set; }
    public ActivityEntityType EntityType { get; private set; }
    public Guid EntityID { get; private set; }
    public ActivityType ActivityType { get; private set; }
    public string Description { get; private set; }
    public DateTime Timestamp { get; private set; }

    private ActivityLogModel() { Description = string.Empty; } // EF Core

    public ActivityLogModel(
      ActivityEntityType entityType,
      Guid entityID,
      ActivityType activityType,
      string description
    )
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(entityID.ToString(), nameof(entityID));
      ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));

      ID = Guid.NewGuid();
      EntityType = entityType;
      EntityID = entityID;
      ActivityType = activityType;
      Description = description;
      Timestamp = DateTime.UtcNow;
    }
  }
}
