namespace ScopeTrack.Application.DTOs
{
  public sealed record DeliverablePostDTO(
    Guid ContractID,
    string Title,
    string Description,
    DateTime? DueDate
  );

  public sealed record DeliverablePatchDTO(
    string NewStatus
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
