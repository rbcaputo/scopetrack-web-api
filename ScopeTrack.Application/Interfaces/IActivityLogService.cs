using ScopeTrack.Application.Dtos;
using ScopeTrack.Domain.Enums;

namespace ScopeTrack.Application.Interfaces
{
  public interface IActivityLogService
  {
    Task<bool> EntityExistsAsync(ActivityEntityType entityType, Guid entityId, CancellationToken ct);
    Task<IReadOnlyList<ActivityLogGetDto>> GetByEntityAsync(ActivityEntityType entityType, Guid entityId, CancellationToken ct);
  }
}
