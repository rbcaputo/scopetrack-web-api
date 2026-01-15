using ScopeTrack.Domain.Enums;

namespace ScopeTrack.Domain.Entities
{
  public sealed class ActivityLogModel
  {
    public Guid ID { get; private set; }
    public ActivityEntityType EntityType { get; private set; }
    public Guid EntityID { get; private set; }
    public string Description { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ActivityLogModel() { } // EF Core

    public ActivityLogModel(
      ActivityEntityType entityType,
      Guid entityID,
      string description
    )
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(entityID.ToString(), nameof(entityID));
      ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));

      ID = Guid.NewGuid();
      EntityType = entityType;
      EntityID = entityID;
      Description = $"[{DateTime.UtcNow}] {description}";
      CreatedAt = DateTime.UtcNow;
    }

    public string AppendDescription(string description)
      => Description += $"\n[{DateTime.UtcNow}] {description}";
  }
}
