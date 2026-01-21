using Microsoft.EntityFrameworkCore;
using ScopeTrack.Application.Dtos;
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

    public async Task<RequestResult<DeliverableGetDto>> UpdateStatusAsync(
      Guid id,
      DeliverablePatchDto dto,
      CancellationToken ct
    )
    {
      DeliverableModel? deliverable = await _context.Deliverables
        .SingleOrDefaultAsync(d => d.Id == id, ct);
      if (deliverable is null)
        return RequestResult<DeliverableGetDto>.Failure("Deliverable not found");

      ContractModel? contract = await _context.Contracts
        .SingleOrDefaultAsync(c => c.Id == deliverable.ContractId, ct);
      if (contract is null)
        return RequestResult<DeliverableGetDto>.Failure("Contract not found");

      if (!Enum.TryParse(dto.NewStatus, true, out DeliverableStatus newStatus))
        return RequestResult<DeliverableGetDto>.Failure(
          "Invalid deliverable status"
        );

      try
      {
        deliverable.ChangeStatus(newStatus, contract.Status);
      }
      catch (InvalidOperationException ex)
      {
        return RequestResult<DeliverableGetDto>.Failure(ex.Message);
      }

      await _context.SaveChangesAsync(ct);

      return RequestResult<DeliverableGetDto>.Success(
        DeliverableMapper.ModelToGetDto(deliverable)
      );
    }

    public async Task<RequestResult<DeliverableGetDto>> GetByIdAsync(
      Guid id,
      CancellationToken ct
    )
    {
      DeliverableGetDto? deliverable = await _context.Deliverables
        .AsNoTracking()
        .Where(d => d.Id == id)
        .OrderBy(d => d.Status)
        .Select(d => DeliverableMapper.ModelToGetDto(d))
        .SingleOrDefaultAsync(ct);
      if (deliverable is null)
        return RequestResult<DeliverableGetDto>.Failure("Deliverable not found");

      return RequestResult<DeliverableGetDto>.Success(deliverable);
    }
  }
}