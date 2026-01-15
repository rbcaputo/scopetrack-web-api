namespace ScopeTrack.Application.DTOs
{
  public sealed record DeliverablePostDTO(
    Guid contractID,
    string Title,
    string Description,
    DateTime? DueDate
  );

  public sealed record DeliverablePatchDTO(
    string Status
  );

  public sealed record DeliverableGetDTO(
    Guid ID,
    Guid ContractID,
    string Title,
    string Description,
    string Status,
    DateTime? DueDate,
    DateTime CreatedAt,
    DateTime UpdatedAt
  );
}
