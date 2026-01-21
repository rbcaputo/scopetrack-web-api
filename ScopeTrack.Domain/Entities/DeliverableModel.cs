using ScopeTrack.Domain.Enums;

namespace ScopeTrack.Domain.Entities
{
  public sealed class DeliverableModel
  {
    public Guid Id { get; private set; }
    public Guid ContractId { get; private set; }
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
      Guid contractId,
      string title,
      string? description,
      DateTime? dueDate
    )
    {
      if (contractId == Guid.Empty)
        throw new ArgumentException("Contract id cannot be empty", nameof(contractId));
      ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));

      Id = Guid.NewGuid();
      ContractId = contractId;
      Title = title;
      Description = description ?? string.Empty;
      Status = DeliverableStatus.Pending;
      DueDate = dueDate;
      CreatedAt = DateTime.UtcNow;
      UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeStatus(
      DeliverableStatus newStatus,
      ContractStatus contractStatus
    )
    {
      if (contractStatus != ContractStatus.Active)
        throw new InvalidOperationException(
          "Cannot change deliverable status when contract is not active"
        );
      if (Status == DeliverableStatus.Completed)
        throw new InvalidOperationException(
          "Cannot change status of a completed deliverable"
        );
      if (Status == DeliverableStatus.Cancelled)
        throw new InvalidOperationException(
          "Cannot change status of a cancelled deliverable"
        );
      if (Status == newStatus)
        throw new InvalidOperationException(
          $"Deliverable status is already {Status}"
        );
      if (!IsValidTransition(Status, newStatus))
        throw new InvalidOperationException(
          $"Invalid status transition from {Status} to {newStatus}"
        );

      Status = newStatus;
      UpdatedAt = DateTime.UtcNow;
    }

    private static bool IsValidTransition(
      DeliverableStatus current,
      DeliverableStatus newStatus
    ) => (current, newStatus) switch
    {
      (DeliverableStatus.Pending, DeliverableStatus.InProgress) => true,
      (DeliverableStatus.Pending, DeliverableStatus.Completed) => false,
      (DeliverableStatus.Pending, DeliverableStatus.Cancelled) => true,
      (DeliverableStatus.InProgress, DeliverableStatus.Pending) => false,
      (DeliverableStatus.InProgress, DeliverableStatus.Completed) => true,
      (DeliverableStatus.InProgress, DeliverableStatus.Cancelled) => true,
      _ => false
    };
  }
}
