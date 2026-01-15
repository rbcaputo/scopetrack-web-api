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

    public async Task<DeliverableGetDTO?> CreateAsync(
      Guid contractID,
      DeliverablePostDTO dto,
      CancellationToken ct
    )
    {
      ContractModel? contract = await _context.Contracts
        .FirstOrDefaultAsync(c => c.ID == contractID, ct);
      if (contract is null)
        return null;

      DeliverableModel deliverable = DeliverableMapper.PostDTOToModel(dto);
      await _context.Deliverables.AddAsync(deliverable, ct);
      await _context.SaveChangesAsync(ct);

      ActivityLogModel activityLog = new(
        ActivityEntityType.Deliverable,
        deliverable.ID,
        "Deliverable created"
      );

      await _activityLogService.CreateAsync(activityLog, ct);

      return DeliverableMapper.ModelToGetDTO(deliverable);
    }

    public async Task<DeliverableGetDTO?> UpdateStatusAsync(
      Guid id,
      DeliverableStatus newStatus,
      CancellationToken ct
    )
    {
      DeliverableModel? deliverable = await _context.Deliverables
        .FirstOrDefaultAsync(d => d.ID == id, ct);
      if (deliverable is null)
        return null;
      ContractModel? contract = await _context.Contracts
        .FirstOrDefaultAsync(c => c.ID == deliverable.ContractID, ct);
      if (contract is null)
        return null;

      deliverable.ChangeStatus(newStatus, contract.Status);

      ActivityLogModel? activityLog = await _context.ActivityLogs
        .FirstOrDefaultAsync(a => a.EntityID == deliverable.ID, ct);
      activityLog?.AppendDescription("Deliverable status updated");

      await _context.SaveChangesAsync(ct);

      return DeliverableMapper.ModelToGetDTO(deliverable);
    }

    public async Task<DeliverableGetDTO?> ReadByIDAsync(Guid id, CancellationToken ct)
    {
      DeliverableModel? deliverable = await _context.Deliverables
        .FirstOrDefaultAsync(d => d.ID == id, ct);

      return deliverable is not null
        ? DeliverableMapper.ModelToGetDTO(deliverable)
        : null;
    }
  }
}