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
    public IReadOnlyList<DeliverableModel> Deliverables => _deliverables;

    private ContractModel() { } // EF Core

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
      Status = ContractStatus.Active;
      UpdatedAt = DateTime.UtcNow;
    }

    public void Archive()
    {
      Status = ContractStatus.Archived;
      UpdatedAt = DateTime.UtcNow;
    }

    public DeliverableModel AddDeliverable(
      string title,
      string description,
      DateTime? dueDate
    )
    {
      if (Status == ContractStatus.Archived)
        throw new InvalidOperationException(
          "Cannot add deliverables to an archived contract"
        );

      DeliverableModel deliverable = new(ID, title, description, dueDate);
      _deliverables.Add(deliverable);
      UpdatedAt = DateTime.UtcNow;

      return deliverable;
    }
  }
}
