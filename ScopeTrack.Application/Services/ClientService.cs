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
    IContractService contractService,
    IActivityLogService activityLogService
  ) : IClientService
  {
    private readonly ScopeTrackDbContext _context = context;
    private readonly IContractService _contractService =
      contractService;
    private readonly IActivityLogService _activityLogService =
      activityLogService;

    public async Task<ClientGetDTO> CreateAsync(ClientPostDTO dto, CancellationToken ct)
    {
      ClientModel client = new(dto.Name, dto.ContactEmail);

      await _context.Clients.AddAsync(client, ct);
      await _context.SaveChangesAsync(ct);

      ActivityLogModel activityLog = new(
        ActivityEntityType.Client,
        client.ID,
        "Client created"
      );

      await _activityLogService.CreateAsync(activityLog, ct);

      return ClientMapper.ModelToGetDTO(client);
    }

    public async Task<ClientGetDTO?> UpdateAsync(
      Guid id,
      ClientPutDTO dto,
      CancellationToken ct
    )
    {
      ClientModel? client = await _context.Clients
        .FirstOrDefaultAsync(c => c.ID == id, ct);
      if (client is null)
        return null;

      client.UpdateDetails(dto.Name, dto.ContactEmail);
      await _context.SaveChangesAsync(ct);

      return ClientMapper.ModelToGetDTO(client);
    }

    public async Task<ClientGetDTO?> ToggleStatusAsync(Guid id, CancellationToken ct)
    {
      ClientModel? client = await _context.Clients
        .FirstOrDefaultAsync(c => c.ID == id, ct);
      if (client is null)
        return null;

      client.ToggleStatus();

      ActivityLogModel? activityLog = await _context.ActivityLogs
        .FirstOrDefaultAsync(a => a.EntityID == client.ID, ct);
      activityLog?.AppendDescription($"Client status updated");

      await _context.SaveChangesAsync(ct);

      return ClientMapper.ModelToGetDTO(client);
    }

    public async Task<ClientGetDTO?> AddContractAsync(
      Guid id,
      ContractPostDTO dto,
      CancellationToken ct
    )
    {
      ClientModel? client = await _context.Clients
        .FirstOrDefaultAsync(c => c.ID == id, ct);
      if (client is null)
        return null;

      await _contractService.CreateAsync(id, dto, ct);

      ContractType type = Enum.TryParse(dto.Type, out ContractType contractType)
        ? contractType
        : ContractType.FixedPrice;
      client.AddContract(dto.Title, dto.Description, type);
      await _context.SaveChangesAsync(ct);

      return ClientMapper.ModelToGetDTO(client);
    }

    public async Task<IEnumerable<ClientGetDTO>> ReadAllAsync(CancellationToken ct)
    {
      IEnumerable<ClientModel> clients = await _context.Clients
        .ToListAsync(ct);

      return clients.Any()
        ? clients.Select(c => ClientMapper.ModelToGetDTO(c))
        : [];
    }

    public async Task<ClientGetDTO?> ReadByIDAsync(Guid id, CancellationToken ct)
    {
      ClientModel? client = await _context.Clients
        .FirstOrDefaultAsync(c => c.ID == id, ct);

      return client is not null
        ? ClientMapper.ModelToGetDTO(client)
        : null;
    }
  }
}
