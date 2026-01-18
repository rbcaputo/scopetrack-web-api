using Microsoft.EntityFrameworkCore;
using ScopeTrack.Application.DTOs;
using ScopeTrack.Application.Interfaces;
using ScopeTrack.Application.Mappers;
using ScopeTrack.Domain.Enums;
using ScopeTrack.Infrastructure.Data;

namespace ScopeTrack.Application.Services
{
  public sealed class ActivityLogService(ScopeTrackDbContext context) : IActivityLogService
  {
    private readonly ScopeTrackDbContext _context = context;

    public async Task<bool> EntityExistsAsync(
      ActivityEntityType entityType,
      Guid entityID,
      CancellationToken ct
    )
    {
      return entityType switch
      {
        ActivityEntityType.Client
          => await _context.Clients.AnyAsync(c => c.ID == entityID, ct),
        ActivityEntityType.Contract
          => await _context.Contracts.AnyAsync(c => c.ID == entityID, ct),
        ActivityEntityType.Deliverable
          => await _context.Contracts.AnyAsync(d => d.ID == entityID, ct),
        _ => false
      };
    }

    public async Task<IReadOnlyList<ActivityLogGetDTO>> GetByEntityAsync(
      ActivityEntityType entityType,
      Guid entityID,
      CancellationToken ct
    ) => await _context.ActivityLogs
          .AsNoTracking()
          .Where(a => a.EntityType == entityType && a.EntityID == entityID)
          .OrderBy(a => a.Timestamp)
          .Select(a => ActivityLogMapper.ModelToGetDTO(a))
          .ToListAsync(ct);
  }
}
