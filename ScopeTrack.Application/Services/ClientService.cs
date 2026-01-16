using Microsoft.EntityFrameworkCore;
using ScopeTrack.Application.DTOs;
using ScopeTrack.Application.Interfaces;
using ScopeTrack.Application.Mappers;
using ScopeTrack.Domain.Entities;
using ScopeTrack.Domain.Enums;
using ScopeTrack.Infrastructure.Data;

namespace ScopeTrack.Application.Services
{
  public sealed class ClientService(
    ScopeTrackDbContext context,
    IActivityLogService activityLogService,
    IContractService contractService
  ) : IClientService
  {
    private readonly ScopeTrackDbContext _context = context;
    private readonly IActivityLogService _activityLogService =
      activityLogService;
    private readonly IContractService _contractService =
      contractService;

    public async Task<ClientGetDTO> CreateAsync(ClientPostDTO dto, CancellationToken ct)
    {
      ClientModel client = ClientMapper.PostDTOToModel(dto);

      await _context.Clients.AddAsync(client, ct);

      ActivityLogModel activityLog = new(
        ActivityEntityType.Client,
        client.ID,
        ActivityType.Created,
        "Client created"
      );

      await _activityLogService.StageAsync(activityLog, ct);
      await _context.SaveChangesAsync(ct);

      return ClientMapper.ModelToGetDTO(client);
    }

    public async Task<ClientGetDTO?> UpdateAsync(
      ClientPutDTO dto,
      CancellationToken ct
    )
    {
      ClientModel? client = await _context.Clients
        .SingleOrDefaultAsync(c => c.ID == dto.ID, ct);
      if (client is null)
        return null;

      client.UpdateDetails(dto.Name, dto.ContactEmail);
      await _context.SaveChangesAsync(ct);

      return ClientMapper.ModelToGetDTO(client);
    }

    public async Task<ClientGetDTO?> ToggleStatusAsync(Guid id, CancellationToken ct)
    {
      ClientModel? client = await _context.Clients
        .SingleOrDefaultAsync(c => c.ID == id, ct);
      if (client is null)
        return null;

      client.ToggleStatus();

      ActivityLogModel activityLog = new(
        ActivityEntityType.Client,
        client.ID,
        ActivityType.StatusChanged,
        "Client status changed"
      );

      await _activityLogService.StageAsync(activityLog, ct);
      await _context.SaveChangesAsync(ct);

      return ClientMapper.ModelToGetDTO(client);
    }

    public async Task<ContractGetDTO?> AddContractAsync(
      Guid id,
      ContractPostDTO dto,
      CancellationToken ct
    )
    {
      ClientModel? client = await _context.Clients
        .SingleOrDefaultAsync(c => c.ID == id, ct);
      if (client is null)
        return null;

      ContractModel contract = ContractMapper.PostDTOToModel(dto);
      client.AddContract(contract);

      ActivityLogModel activityLog = new(
        ActivityEntityType.Contract,
        contract.ID,
        ActivityType.Created,
        "Contract created"
      );

      await _contractService.StageAsync(contract, ct);
      await _activityLogService.StageAsync(activityLog, ct);
      await _context.SaveChangesAsync(ct);

      return ContractMapper.ModelToGetDTO(contract);
    }

    public async Task<ClientGetDTO?> GetByIDAsync(Guid id, CancellationToken ct)
      => await _context.Clients
          .Include(c => c.Contracts)
          .AsNoTracking()
          .Where(c => c.ID == id)
          .Select(c => ClientMapper.ModelToGetDTO(c))
          .SingleOrDefaultAsync(ct) ?? null;

    public async Task<IReadOnlyList<ClientGetDTO>> GetAllAsync(CancellationToken ct)
      => await _context.Clients
          .Include(c => c.Contracts)
          .AsNoTracking()
          .OrderBy(c => c.Status)
          .Select(c => ClientMapper.ModelToGetDTO(c))
          .ToListAsync(ct);
  }
}
