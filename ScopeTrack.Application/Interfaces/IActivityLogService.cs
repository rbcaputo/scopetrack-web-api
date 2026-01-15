using ScopeTrack.Application.DTOs;
using ScopeTrack.Domain.Entities;
using ScopeTrack.Domain.Enums;

namespace ScopeTrack.Application.Interfaces
{
  public interface IActivityLogService
  {
    Task<ActivityLogGetDTO> CreateAsync(ActivityLogModel activityLog, CancellationToken ct);
    
    Task<IEnumerable<ActivityLogGetDTO>> ReadByEntityAsync(
      ActivityEntityType entityType,
      Guid entityID,
      CancellationToken ct
    );
  }
}
