using Microsoft.EntityFrameworkCore;
using ScopeTrack.Application.DTOs;
using ScopeTrack.Application.Interfaces;
using ScopeTrack.Application.Mappers;
using ScopeTrack.Domain.Entities;
using ScopeTrack.Domain.Enums;
using ScopeTrack.Infrastructure.Data;

namespace ScopeTrack.Application.Services
{
  public sealed class ContractService(
    ScopeTrackDbContext context,
    IActivityLogService activityLogService,
    IDeliverableService deliverableService
  ) : IContractService
  {
    private readonly ScopeTrackDbContext _context = context;
    private readonly IActivityLogService _activityLogService
      = activityLogService;
    private readonly IDeliverableService _deliverableService
      = deliverableService;

    public async Task StageAsync(
      ContractModel contract,
      CancellationToken ct
    ) => await _context.Contracts.AddAsync(contract, ct);

    public async Task<RequestResult<ContractGetDTO>> UpdateStatusAsync(
      Guid id,
      ContractPatchDTO dto,
      CancellationToken ct
    )
    {
      ContractModel? contract = await _context.Contracts
        .SingleOrDefaultAsync(c => c.ID == id, ct);
      if (contract is null)
        return RequestResult<ContractGetDTO>.Failure("Contract not found");

      if (!Enum.TryParse(dto.NewStatus, out ContractStatus newStatus))
        throw new ArgumentOutOfRangeException(
          nameof(dto),
          "Invalid contract status"
        );

      switch (newStatus)
      {
        case ContractStatus.Active:
          contract.Activate();
          break;
        case ContractStatus.Archived:
          contract.Archive();
          break;
        default:
          throw new ArgumentOutOfRangeException(
            nameof(dto),
            "Invalid contract status"
          );
      }

      ActivityLogModel activityLog = new(
        ActivityEntityType.Contract,
        contract.ID,
        ActivityType.StatusChanged,
        "Contract status changed"
      );

      await _activityLogService.StageAsync(activityLog, ct);
      await _context.SaveChangesAsync(ct);

      return RequestResult<ContractGetDTO>.Success(
        ContractMapper.ModelToGetDTO(contract)
      );
    }

    public async Task<RequestResult<DeliverableGetDTO>> AddDeliverableAsync(
      Guid id,
      DeliverablePostDTO dto,
      CancellationToken ct
    )
    {
      ContractModel? contract = await _context.Contracts
        .SingleOrDefaultAsync(c => c.ID == id, ct);
      if (contract is null)
        return RequestResult<DeliverableGetDTO>.Failure("Contract not found");

      DeliverableModel deliverable = DeliverableMapper.PostDTOToModel(dto);
      contract.AddDeliverable(deliverable);

      ActivityLogModel activityLog = new(
        ActivityEntityType.Deliverable,
        deliverable.ID,
        ActivityType.Created,
        "Deliverable created"
      );

      await _deliverableService.StageAsync(deliverable, ct);
      await _activityLogService.StageAsync(activityLog, ct);
      await _context.SaveChangesAsync(ct);

      return RequestResult<DeliverableGetDTO>.Success(
        DeliverableMapper.ModelToGetDTO(deliverable)
      );
    }

    public async Task<RequestResult<ContractGetDTO>> GetByIDAsync(
      Guid id,
      CancellationToken ct
    )
    {
      ContractGetDTO? contract = await _context.Contracts
        .Include(c => c.Deliverables)
        .AsNoTracking()
        .Where(c => c.ID == id)
        .Select(c => ContractMapper.ModelToGetDTO(c))
        .SingleOrDefaultAsync(ct);
      if (contract is null)
        return RequestResult<ContractGetDTO>.Failure("Contract not found");

      return RequestResult<ContractGetDTO>.Success(contract);
    }
  }
}
