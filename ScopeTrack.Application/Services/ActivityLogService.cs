using Microsoft.EntityFrameworkCore;
using ScopeTrack.Application.Dtos;
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
      Guid entityId,
      CancellationToken ct
    )
    {
      return entityType switch
      {
        ActivityEntityType.Client
          => await _context.Clients.AnyAsync(c => c.Id == entityId, ct),
        ActivityEntityType.Contract
          => await _context.Contracts.AnyAsync(c => c.Id == entityId, ct),
        ActivityEntityType.Deliverable
          => await _context.Contracts.AnyAsync(d => d.Id == entityId, ct),
        _ => false
      };
    }

    public async Task<IReadOnlyList<ActivityLogGetDto>> GetByEntityAsync(
      ActivityEntityType entityType,
      Guid entityId,
      CancellationToken ct
    ) => await _context.ActivityLogs
          .AsNoTracking()
          .Where(a => a.EntityType == entityType && a.EntityId == entityId)
          .OrderBy(a => a.Timestamp)
          .Select(a => ActivityLogMapper.ModelToGetDto(a))
          .ToListAsync(ct);
  }
}
