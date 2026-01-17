using Microsoft.EntityFrameworkCore;
using ScopeTrack.Application.DTOs;
using ScopeTrack.Application.Interfaces;
using ScopeTrack.Application.Mappers;
using ScopeTrack.Domain.Entities;
using ScopeTrack.Domain.Enums;
using ScopeTrack.Infrastructure.Data;

namespace ScopeTrack.Application.Services
{
  public sealed class DeliverableService(ScopeTrackDbContext context) : IDeliverableService
  {
    private readonly ScopeTrackDbContext _context = context;

    public async Task<RequestResult<DeliverableGetDTO>> UpdateStatusAsync(
      Guid id,
      DeliverablePatchDTO dto,
      CancellationToken ct
    )
    {
      DeliverableModel? deliverable = await _context.Deliverables
        .SingleOrDefaultAsync(d => d.ID == id, ct);
      if (deliverable is null)
        return RequestResult<DeliverableGetDTO>.Failure("Deliverable not found");

      ContractModel? contract = await _context.Contracts
        .SingleOrDefaultAsync(c => c.ID == deliverable.ContractID, ct);
      if (contract is null)
        return RequestResult<DeliverableGetDTO>.Failure("Contract not found");

      if (!Enum.TryParse(dto.NewStatus, out DeliverableStatus newStatus))
        throw new ArgumentOutOfRangeException(
          nameof(dto),
          "Invalid deliverable status"
        );

      deliverable.ChangeStatus(newStatus, contract.Status);

      await _context.SaveChangesAsync(ct);

      return RequestResult<DeliverableGetDTO>.Success(
        DeliverableMapper.ModelToGetDTO(deliverable)
      );
    }

    public async Task<RequestResult<DeliverableGetDTO>> GetByIDAsync(
      Guid id,
      CancellationToken ct
    )
    {
      DeliverableGetDTO? deliverable = await _context.Deliverables
        .AsNoTracking()
        .Where(d => d.ID == id)
        .OrderBy(d => d.Status)
        .Select(d => DeliverableMapper.ModelToGetDTO(d))
        .SingleOrDefaultAsync(ct);
      if (deliverable is null)
        return RequestResult<DeliverableGetDTO>.Failure("Deliverable not found");

      return RequestResult<DeliverableGetDTO>.Success(deliverable);
    }
  }
}