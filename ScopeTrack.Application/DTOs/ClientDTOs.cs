namespace ScopeTrack.Application.DTOs
{
  public sealed record ClientPostDTO(
    string Name,
    string Email
  );

  public sealed record ClientPutDTO(
    Guid ID,
    string Name,
    string Email
  );

  public sealed record ClientGetDTO(
    Guid ID,
    string Name,
    string Email,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<ContractGetDTO> Contracts
  );
}
