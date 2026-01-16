using Microsoft.EntityFrameworkCore;
using ScopeTrack.Application.DTOs;
using ScopeTrack.Application.Interfaces;
using ScopeTrack.Application.Mappers;
using ScopeTrack.Domain.Entities;
using ScopeTrack.Domain.Enums;
using ScopeTrack.Infrastructure.Data;

namespace ScopeTrack.Application.Services
{
  public sealed class DeliverableService(
    ScopeTrackDbContext context,
    IActivityLogService activityLogService
  ) : IDeliverableService
  {
    private readonly ScopeTrackDbContext _context = context;
    private readonly IActivityLogService _activityLogService =
      activityLogService;

    public async Task StageAsync(
      DeliverableModel model,
      CancellationToken ct
    ) => await _context.AddAsync(model, ct);

    public async Task<DeliverableGetDTO?> UpdateStatusAsync(
      Guid id,
      DeliverablePatchDTO dto,
      CancellationToken ct
    )
    {
      DeliverableModel? deliverable = await _context.Deliverables
        .SingleOrDefaultAsync(d => d.ID == id, ct);
      if (deliverable is null)
        return null;

      ContractModel? contract = await _context.Contracts
        .SingleOrDefaultAsync(c => c.ID == deliverable.ContractID, ct);
      if (contract is null)
        return null;

      if (!Enum.TryParse(dto.NewStatus, out DeliverableStatus newStatus))
        throw new ArgumentOutOfRangeException(
          nameof(dto),
          "Invalid deliverable status"
        );

      deliverable.ChangeStatus(newStatus, contract.Status);

      ActivityLogModel activityLog = new(
        ActivityEntityType.Deliverable,
        deliverable.ID,
        ActivityType.StatusChanged,
        "Deliverable status changed"
      );

      await _activityLogService.StageAsync(activityLog, ct);
      await _context.SaveChangesAsync(ct);

      return DeliverableMapper.ModelToGetDTO(deliverable);
    }

    public async Task<DeliverableGetDTO?> GetByIDAsync(Guid id, CancellationToken ct)
      => await _context.Deliverables
          .AsNoTracking()
          .Where(d => d.ID == id)
          .OrderBy(d => d.Status)
          .Select(d => DeliverableMapper.ModelToGetDTO(d))
          .SingleOrDefaultAsync(ct);
  }
}