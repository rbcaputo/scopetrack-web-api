using Microsoft.EntityFrameworkCore;
using ScopeTrack.Application.Dtos;
using ScopeTrack.Application.Interfaces;
using ScopeTrack.Application.Mappers;
using ScopeTrack.Domain.Entities;
using ScopeTrack.Domain.Enums;
using ScopeTrack.Infrastructure.Data;

namespace ScopeTrack.Application.Services
{
  public sealed class ContractService(ScopeTrackDbContext context) : IContractService
  {
    private readonly ScopeTrackDbContext _context = context;

    public async Task<RequestResult<ContractGetDto>> UpdateStatusAsync(
      Guid id,
      ContractPatchDto dto,
      CancellationToken ct
    )
    {
      ContractModel? contract = await _context.Contracts
        .Include(c => c.Deliverables)
        .SingleOrDefaultAsync(c => c.Id == id, ct);
      if (contract is null)
        return RequestResult<ContractGetDto>.Failure("Contract not found");

      if (!Enum.TryParse(dto.NewStatus, true, out ContractStatus newStatus))
        return RequestResult<ContractGetDto>.Failure(
          "Invalid contract status"
        );

      try
      {
        switch (newStatus)
        {
          case ContractStatus.Active:
            contract.Activate();
            break;
          case ContractStatus.Completed:
            contract.Complete();
            break;
          case ContractStatus.Archived:
            contract.Archive();
            break;
        }
      }
      catch (InvalidOperationException ex)
      {
        return RequestResult<ContractGetDto>.Failure(ex.Message);
      }

      await _context.SaveChangesAsync(ct);

      return RequestResult<ContractGetDto>.Success(
        ContractMapper.ModelToGetDto(contract)
      );
    }

    public async Task<RequestResult<DeliverableGetDto>> AddDeliverableAsync(
      Guid id,
      DeliverablePostDto dto,
      CancellationToken ct
    )
    {
      ContractModel? contract = await _context.Contracts
        .Include(c => c.Deliverables)
        .SingleOrDefaultAsync(c => c.Id == id, ct);
      if (contract is null)
        return RequestResult<DeliverableGetDto>.Failure("Contract not found");

      DeliverableModel deliverable = DeliverableMapper.PostDtoToModel(contract.Id, dto);
      contract.AddDeliverable(deliverable);

      await _context.Deliverables.AddAsync(deliverable, ct);
      await _context.SaveChangesAsync(ct);

      return RequestResult<DeliverableGetDto>.Success(
        DeliverableMapper.ModelToGetDto(deliverable)
      );
    }

    public async Task<RequestResult<ContractGetDto>> GetByIdAsync(
      Guid id,
      CancellationToken ct
    )
    {
      ContractGetDto? contract = await _context.Contracts
        .Include(c => c.Deliverables)
        .AsNoTracking()
        .Where(c => c.Id == id)
        .Select(c => ContractMapper.ModelToGetDto(c))
        .SingleOrDefaultAsync(ct);
      if (contract is null)
        return RequestResult<ContractGetDto>.Failure("Contract not found");

      return RequestResult<ContractGetDto>.Success(contract);
    }

    public async Task<IReadOnlyList<ContractGetDto>> GetAllAsync(CancellationToken ct)
      => await _context.Contracts
          .Include(c => c.Deliverables)
          .AsNoTracking()
          .Select(c => ContractMapper.ModelToGetDto(c))
          .ToListAsync(ct);
  }
}
