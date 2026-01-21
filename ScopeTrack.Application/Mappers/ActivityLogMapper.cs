using ScopeTrack.Application.Dtos;
using ScopeTrack.Domain.Entities;

namespace ScopeTrack.Application.Mappers
{
  public static class ActivityLogMapper
  {
    public static ActivityLogGetDto ModelToGetDto(ActivityLogModel model)
      => new(
        model.EntityType.ToString(),
        model.ActivityType.ToString(),
        model.Description,
        model.Timestamp
      );
  }
}
