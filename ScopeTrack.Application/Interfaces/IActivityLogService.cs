using ScopeTrack.Application.DTOs;
using ScopeTrack.Domain.Entities;
using ScopeTrack.Domain.Enums;

namespace ScopeTrack.Application.Interfaces
{
  public interface IActivityLogService
  {
    Task StageAsync(ActivityLogModel model, CancellationToken ct);
    Task<IReadOnlyList<ActivityLogGetDTO>> GetByEntityAsync(
      ActivityEntityType entityType,
      Guid entityId,
      CancellationToken ct
    );
  }
}
