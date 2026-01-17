using ScopeTrack.Application.DTOs;
using ScopeTrack.Domain.Entities;
using ScopeTrack.Domain.Enums;

namespace ScopeTrack.Application.Interfaces
{
  public interface IActivityLogService
  {
    Task StageAsync(ActivityLogModel model, CancellationToken ct);
    Task<Result<IReadOnlyList<ActivityLogGetDTO>>> GetByEntityIDAsync(Guid entityId, CancellationToken ct);
  }
}
