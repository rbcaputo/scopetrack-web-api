using ScopeTrack.Domain.Enums;

namespace ScopeTrack.Domain.Entities
{
  public sealed class ContractModel
  {
    public Guid ID { get; private set; }
    public Guid ClientID { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public ContractType Type { get; private set; }
    public ContractStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<DeliverableModel> _deliverables = [];
    public IReadOnlyList<DeliverableModel> Deliverables
      => _deliverables;

    private ContractModel() // EF Core
    {
      Title = string.Empty;
      Description = string.Empty;
    }

    public ContractModel(
      Guid clientID,
      string title,
      string? description,
      ContractType type
    )
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(
        clientID.ToString(),
        nameof(clientID)
      );
      ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));

      ID = Guid.NewGuid();
      ClientID = clientID;
      Title = title;
      Description = description ?? string.Empty;
      Type = type;
      Status = ContractStatus.Draft;
      CreatedAt = DateTime.UtcNow;
      UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
      if (Status == ContractStatus.Active)
        throw new InvalidOperationException(
          "Contract is already active"
        );
      if (Status == ContractStatus.Completed)
        throw new InvalidOperationException(
          "Cannot activate a completed contract"
        );
      if (Status == ContractStatus.Archived)
        throw new InvalidOperationException(
          "Cannot activate an archived contract"
        );
      if (Deliverables.Count == 0)
        throw new InvalidOperationException(
          "Cannot activate contract without at least one deliverable"
        );

      Status = ContractStatus.Active;
      UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
      if (Status == ContractStatus.Completed)
        throw new InvalidOperationException(
          "Contract is already completed"
        );
      if (Status == ContractStatus.Archived)
        throw new InvalidOperationException(
          "Cannot complete an archived contract"
        );

      Status = ContractStatus.Completed;
      UpdatedAt = DateTime.UtcNow;
    }

    public void Archive()
    {
      if (Status == ContractStatus.Archived)
        throw new InvalidOperationException(
          "Contract is already archived"
        );

      Status = ContractStatus.Archived;
      UpdatedAt = DateTime.UtcNow;
    }

    public void AddDeliverable(DeliverableModel deliverable)
    {
      if (Status == ContractStatus.Archived)
        throw new InvalidOperationException(
          "Cannot add deliverables to an archived contract"
        );

      _deliverables.Add(deliverable);
      UpdatedAt = DateTime.UtcNow;
    }
  }
}
