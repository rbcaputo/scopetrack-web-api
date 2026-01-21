using ScopeTrack.Domain.Enums;

namespace ScopeTrack.Domain.Entities
{
  public sealed class ClientModel
  {
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public ClientStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<ContractModel> _contracts = [];
    public IReadOnlyList<ContractModel> Contracts
      => _contracts.AsReadOnly();

    private ClientModel() // EF Core
    {
      Name = string.Empty;
      Email = string.Empty;
    }

    public ClientModel(string name, string email)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
      ArgumentException.ThrowIfNullOrWhiteSpace(
        email,
        nameof(email)
      );

      Id = Guid.NewGuid();
      Name = name;
      Email = email;
      Status = ClientStatus.Active;
      CreatedAt = DateTime.UtcNow;
      UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string email)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
      ArgumentException.ThrowIfNullOrWhiteSpace(
        email,
        nameof(email)
      );

      Name = name;
      Email = email;
      UpdatedAt = DateTime.UtcNow;
    }

    public void ToggleStatus()
    {
      Status = Status == ClientStatus.Active
        ? ClientStatus.Inactive
        : ClientStatus.Active;

      UpdatedAt = DateTime.UtcNow;
    }

    public void AddContract(ContractModel contract)
    {
      if (Status == ClientStatus.Inactive)
        throw new InvalidOperationException(
          "Cannot add contracts to an inactive client"
        );

      _contracts.Add(contract);
      UpdatedAt = DateTime.UtcNow;
    }
  }
}
