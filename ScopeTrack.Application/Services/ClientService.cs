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

    public async Task<Result<ClientGetDTO>> CreateAsync(
      ClientPostDTO dto,
      CancellationToken ct
    )
    {
      bool exists = await _context.Clients
        .AnyAsync(c => c.Name == dto.Name || c.Email == dto.Email, ct);
      if (exists)
        return Result<ClientGetDTO>.Failure("Client already exists");

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

      return Result<ClientGetDTO>.Success(
        ClientMapper.ModelToGetDTO(client)
      );
    }

    public async Task<Result<ClientGetDTO>> UpdateAsync(
      Guid id,
      ClientPutDTO dto,
      CancellationToken ct
    )
    {
      ClientModel? client = await _context.Clients
        .SingleOrDefaultAsync(c => c.ID == id, ct);
      if (client is null)
        return Result<ClientGetDTO>.Failure("Client not found");

      client.UpdateDetails(dto.Name, dto.Email);
      await _context.SaveChangesAsync(ct);

      return Result<ClientGetDTO>.Success(
        ClientMapper.ModelToGetDTO(client)
      );
    }

    public async Task<Result<ClientGetDTO>> ToggleStatusAsync(
      Guid id,
      CancellationToken ct
    )
    {
      ClientModel? client = await _context.Clients
        .SingleOrDefaultAsync(c => c.ID == id, ct);
      if (client is null)
        return Result<ClientGetDTO>.Failure("Client not found");

      client.ToggleStatus();

      ActivityLogModel activityLog = new(
        ActivityEntityType.Client,
        client.ID,
        ActivityType.StatusChanged,
        "Client status changed"
      );

      await _activityLogService.StageAsync(activityLog, ct);
      await _context.SaveChangesAsync(ct);

      return Result<ClientGetDTO>.Success(
        ClientMapper.ModelToGetDTO(client)
      );
    }

    public async Task<Result<ContractGetDTO>> AddContractAsync(
      Guid id,
      ContractPostDTO dto,
      CancellationToken ct
    )
    {
      ClientModel? client = await _context.Clients
        .SingleOrDefaultAsync(c => c.ID == id, ct);
      if (client is null)
        return Result<ContractGetDTO>.Failure("Client not found");

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

      return Result<ContractGetDTO>.Success(
        ContractMapper.ModelToGetDTO(contract)
      );
    }

    public async Task<Result<ClientGetDTO>> GetByIDAsync(
      Guid id,
      CancellationToken ct
    )
    {
      ClientGetDTO? client = await _context.Clients
        .Include(c => c.Contracts)
        .AsNoTracking()
        .Where(c => c.ID == id)
        .Select(c => ClientMapper.ModelToGetDTO(c))
        .SingleOrDefaultAsync(ct);
      if (client is null)
        return Result<ClientGetDTO>.Failure("Client not found");

      return Result<ClientGetDTO>.Success(client);
    }

    public async Task<Result<IReadOnlyList<ClientGetDTO>>> GetAllAsync(CancellationToken ct)
    {
      IReadOnlyList<ClientGetDTO> clients = await _context.Clients
        .Include(c => c.Contracts)
        .AsNoTracking()
        .OrderBy(c => c.Status)
        .Select(c => ClientMapper.ModelToGetDTO(c))
        .ToListAsync(ct);

      return Result<IReadOnlyList<ClientGetDTO>>.Success(clients);
    }
  }
}
