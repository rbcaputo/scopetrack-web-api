namespace ScopeTrack.Application.Dtos
{
  public sealed record ActivityLogGetDto(
    string EntityType,
    string ActivityType,
    string Description,
    DateTime OccurredAt
  );
}
