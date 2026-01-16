namespace ScopeTrack.Application.DTOs
{
  public sealed record ActivityLogGetDTO(
    string EntityType,
    string ActivityType,
    string Description,
    DateTime OccurredAt
  );
}
