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
    IDeliverableService deliverableService,
    IActivityLogService activityLogService
  ) : IContractService
  {
    private readonly ScopeTrackDbContext _context = context;
    private readonly IDeliverableService _deliverableService =
      deliverableService;
    private readonly IActivityLogService _activityLogService =
      activityLogService;

    public async Task<ContractGetDTO?> CreateAsync(
      Guid clientID,
      ContractPostDTO dto,
      CancellationToken ct
    )
    {
      ClientModel? client = await _context.Clients
        .FirstOrDefaultAsync(c => c.ID == clientID, ct);
      if (client is null)
        return null;

      ContractType type = Enum.TryParse(
        dto.Type,
        out ContractType contractType
      )
        ? contractType
        : throw new ArgumentException($"Invalid contract type: {dto.Type}");

      ContractModel contract = ContractMapper.PostDTOToModel(dto);

      await _context.Contracts.AddAsync(contract, ct);
      await _context.SaveChangesAsync(ct);

      ActivityLogModel activityLog = new(
        ActivityEntityType.Contract,
        contract.ID,
        "Contract created"
      );

      await _activityLogService.CreateAsync(activityLog, ct);

      return ContractMapper.ModelToGetDTO(contract);
    }

    public async Task<ContractGetDTO?> UpdateStatusAsync(
      Guid id,
      ContractStatus newStatus,
      CancellationToken ct
    )
    {
      ContractModel? contract = await _context.Contracts
        .FirstOrDefaultAsync(c => c.ID == id, ct);
      if (contract is null)
        return null;

      switch (newStatus)
      {
        case ContractStatus.Draft:
          return null;
        case ContractStatus.Active:
          contract.Activate();
          break;
        case ContractStatus.Archived:
          contract.Archive();
          break;
        default:
          throw new ArgumentException($"Invalid status: {newStatus}");
      }

      ActivityLogModel? activityLog = await _context.ActivityLogs
        .FirstOrDefaultAsync(a => a.EntityID == contract.ID, ct);
      activityLog?.AppendDescription("Contract status updated");

      await _context.SaveChangesAsync(ct);

      return ContractMapper.ModelToGetDTO(contract);
    }

    public async Task<ContractGetDTO?> AddDeliverableAsync(
      Guid id,
      DeliverablePostDTO dto,
      CancellationToken ct
    )
    {
      ContractModel? contract = await _context.Contracts
        .FirstOrDefaultAsync(c => c.ID == id, ct);
      if (contract is null)
        return null;

      await _deliverableService.CreateAsync(id, dto, ct);

      contract.AddDeliverable(dto.Title, dto.Description, dto.DueDate);
      await _context.SaveChangesAsync(ct);

      return ContractMapper.ModelToGetDTO(contract);
    }

    public async Task<ContractGetDTO?> ReadByIDAsync(Guid id, CancellationToken ct)
    {
      ContractModel? contract = await _context.Contracts
        .FirstOrDefaultAsync(c => c.ID == id, ct);

      return contract is not null
        ? ContractMapper.ModelToGetDTO(contract)
        : null;
    }
  }
}
