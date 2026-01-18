using ScopeTrack.Application.DTOs;
using ScopeTrack.Domain.Enums;

namespace ScopeTrack.Application.Interfaces
{
  public interface IActivityLogService
  {
    Task<bool> EntityExistsAsync(ActivityEntityType entityType, Guid entityID, CancellationToken ct);
    Task<IReadOnlyList<ActivityLogGetDTO>> GetByEntityAsync(ActivityEntityType entityType, Guid entityId, CancellationToken ct);
  }
}
