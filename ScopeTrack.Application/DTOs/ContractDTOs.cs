namespace ScopeTrack.Application.DTOs
{
  public sealed record ContractPostDTO(
    Guid ClientID,
    string Title,
    string? Description,
    string Type
  );

  public sealed record ContractGetDTO(
    Guid ID,
    Guid ClientID,
    string Title,
    string Description,
    string Type,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<DeliverableGetDTO> Deliverables
  );
}
