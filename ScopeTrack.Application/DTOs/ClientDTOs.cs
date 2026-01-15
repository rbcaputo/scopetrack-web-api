namespace ScopeTrack.Application.DTOs
{
  public sealed record ClientPostDTO(
    string Name,
    string ContactEmail
  );

  public sealed record ClientPutDTO(
    string Name,
    string ContactEmail
  );

  public sealed record ClientGetDTO(
    Guid ID,
    string Name,
    string ContactEmail,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<ContractGetDTO> Contracts
  );
}
