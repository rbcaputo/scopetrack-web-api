using ScopeTrack.Application.DTOs;
using ScopeTrack.Domain.Entities;

namespace ScopeTrack.Application.Mappers
{
  public static class ActivityLogMapper
  {
    public static ActivityLogGetDTO ModelToGetDTO(ActivityLogModel model)
      => new(
        model.EntityType.ToString(),
        model.ActivityType.ToString(),
        model.Description,
        model.Timestamp
      );
  }
}
