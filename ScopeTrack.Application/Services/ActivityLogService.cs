using Microsoft.EntityFrameworkCore;
using ScopeTrack.Application.DTOs;
using ScopeTrack.Application.Interfaces;
using ScopeTrack.Application.Mappers;
using ScopeTrack.Domain.Entities;
using ScopeTrack.Domain.Enums;
using ScopeTrack.Infrastructure.Data;

namespace ScopeTrack.Application.Services
{
  public sealed class ActivityLogService(
    ScopeTrackDbContext context
  ) : IActivityLogService
  {
    private readonly ScopeTrackDbContext _context = context;

    public async Task StageAsync(
      ActivityLogModel model,
      CancellationToken ct
    ) => await _context.ActivityLogs.AddAsync(model, ct);

    public async Task<IReadOnlyList<ActivityLogGetDTO>> GetByEntityAsync(
      ActivityEntityType entityType,
      Guid entityID,
      CancellationToken ct
    ) => await _context.ActivityLogs
      .AsNoTracking()
      .Where(a => a.EntityType == entityType && a.EntityID == entityID)
      .OrderBy(a => a.OccurredAt)
      .Select(a => ActivityLogMapper.ModelToGetDTO(a))
      .ToListAsync(ct);
  }
}
