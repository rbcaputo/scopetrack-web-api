using ScopeTrack.Domain.Enums;

namespace ScopeTrack.Domain.Entities
{
  public sealed class DeliverableModel
  {
    public Guid ID { get; private set; }
    public Guid ContractID { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public DeliverableStatus Status { get; private set; }
    public DateTime? DueDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private DeliverableModel() // EF Core
    {
      Title = string.Empty;
      Description = string.Empty;
    }

    public DeliverableModel(
      Guid contractID,
      string title,
      string? description,
      DateTime? dueDate
    )
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(contractID.ToString(), nameof(contractID));
      ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));

      ID = Guid.NewGuid();
      ContractID = contractID;
      Title = title;
      Description = description ?? string.Empty;
      Status = DeliverableStatus.Planned;
      DueDate = dueDate;
      CreatedAt = DateTime.UtcNow;
      UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeStatus(DeliverableStatus newStatus, ContractStatus contractStatus)
    {
      if (contractStatus != ContractStatus.Active)
        throw new InvalidOperationException("Cannot change deliverable status when contract is not active");
      if (!IsValidTransition(Status, newStatus))
        throw new InvalidOperationException($"Invalid status transition from {Status} to {newStatus}");

      Status = newStatus;
      UpdatedAt = DateTime.UtcNow;
    }

    private static bool IsValidTransition(
      DeliverableStatus current,
      DeliverableStatus newStatus
    ) => (current, newStatus) switch
    {
      (DeliverableStatus.Planned, DeliverableStatus.InProgress) => true,
      (DeliverableStatus.InProgress, DeliverableStatus.Completed) => true,
      (DeliverableStatus.InProgress, DeliverableStatus.Planned) => true,
      _ => false
    };
  }
}
