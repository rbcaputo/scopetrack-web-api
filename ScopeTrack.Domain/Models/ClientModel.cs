using ScopeTrack.Domain.Enums;

namespace ScopeTrack.Domain.Entities
{
  public sealed class ClientModel
  {
    public Guid ID { get; private set; }
    public string Name { get; private set; }
    public string ContactEmail { get; private set; }
    public ClientStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<ContractModel> _contracts = [];
    public IReadOnlyList<ContractModel> Contracts => _contracts.AsReadOnly();

    private ClientModel() { } // EF Core

    public ClientModel(string name, string contactEmail)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
      ArgumentException.ThrowIfNullOrWhiteSpace(
        contactEmail,
        nameof(contactEmail)
      );

      ID = Guid.NewGuid();
      Name = name;
      ContactEmail = contactEmail;
      Status = ClientStatus.Active;
      CreatedAt = DateTime.UtcNow;
      UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string contactEmail)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
      ArgumentException.ThrowIfNullOrWhiteSpace(
        contactEmail,
        nameof(contactEmail)
      );

      Name = name;
      ContactEmail = contactEmail;
      UpdatedAt = DateTime.UtcNow;
    }

    public void ToggleStatus()
    {
      Status = Status == ClientStatus.Active
        ? ClientStatus.Inactive
        : ClientStatus.Active;

      UpdatedAt = DateTime.UtcNow;
    }

    public ContractModel AddContract(
      string title,
      string? description,
      ContractType type
    )
    {
      if (Status == ClientStatus.Inactive)
        throw new InvalidOperationException(
          "Cannot add contracts to an inactive client"
        );

      ContractModel contract = new(ID, title, description, type);
      _contracts.Add(contract);
      UpdatedAt = DateTime.UtcNow;

      return contract;
    }
  }
}
