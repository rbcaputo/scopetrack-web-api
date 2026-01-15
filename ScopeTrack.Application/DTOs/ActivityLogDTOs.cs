namespace ScopeTrack.Application.DTOs
{
  public sealed record ActivityLogGetDTO(
    string ActivityType,
    string Description,
    DateTime OccurredAt
  );
}
