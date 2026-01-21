using System.Text.Json.Serialization;

namespace ScopeTrack.Application.Dtos
{
  public sealed record ClientPostDto
  {
    public string Name { get; }
    public string Email { get; }

    [JsonConstructor]
    public ClientPostDto(string name, string email)
    {
      Name = name;
      Email = email;
    }
  }

  public sealed record ClientPutDto
  {
    public string Name { get; }
    public string Email { get; }

    [JsonConstructor]
    public ClientPutDto(string name, string email)
    {
      Name = name;
      Email = email;
    }
  }

  public sealed record ClientGetDto(
    Guid Id,
    string Name,
    string Email,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<ContractGetDto> Contracts
  );
}
