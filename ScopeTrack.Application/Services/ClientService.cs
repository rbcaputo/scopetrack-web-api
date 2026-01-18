using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ScopeTrack.Application.DTOs;
using ScopeTrack.Application.Interfaces;
using ScopeTrack.Application.Mappers;
using ScopeTrack.Domain.Entities;
using ScopeTrack.Infrastructure.Data;

namespace ScopeTrack.Application.Services
{
  public sealed class ClientService(ScopeTrackDbContext context) : IClientService
  {
    private readonly ScopeTrackDbContext _context = context;

    public async Task<RequestResult<ClientGetDTO>> CreateAsync(
      ClientPostDTO dto,
      CancellationToken ct
    )
    {
      bool exists = await _context.Clients
        .AnyAsync(c => c.Email == dto.Email, ct);
      if (exists)
        return RequestResult<ClientGetDTO>.Failure("Client already exists");

      ClientModel client = ClientMapper.PostDTOToModel(dto);
      await _context.Clients.AddAsync(client, ct);

      try
      {
        await _context.SaveChangesAsync(ct);
      }
      catch (DbUpdateException ex)
        when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601)
      {
        return RequestResult<ClientGetDTO>.Failure("Client already exists");
      }

      return RequestResult<ClientGetDTO>.Success(
        ClientMapper.ModelToGetDTO(client)
      );
    }

    public async Task<RequestResult<ClientGetDTO>> UpdateAsync(
      Guid id,
      ClientPutDTO dto,
      CancellationToken ct
    )
    {
      ClientModel? client = await _context.Clients
        .SingleOrDefaultAsync(c => c.ID == id, ct);
      if (client is null)
        return RequestResult<ClientGetDTO>.Failure("Client not found");

      client.UpdateDetails(dto.Name, dto.Email);
      await _context.SaveChangesAsync(ct);

      return RequestResult<ClientGetDTO>.Success(
        ClientMapper.ModelToGetDTO(client)
      );
    }

    public async Task<RequestResult<ClientGetDTO>> ToggleStatusAsync(
      Guid id,
      CancellationToken ct
    )
    {
      ClientModel? client = await _context.Clients
        .SingleOrDefaultAsync(c => c.ID == id, ct);
      if (client is null)
        return RequestResult<ClientGetDTO>.Failure("Client not found");

      client.ToggleStatus();

      await _context.SaveChangesAsync(ct);

      return RequestResult<ClientGetDTO>.Success(
        ClientMapper.ModelToGetDTO(client)
      );
    }

    public async Task<RequestResult<ContractGetDTO>> AddContractAsync(
      Guid id,
      ContractPostDTO dto,
      CancellationToken ct
    )
    {
      ClientModel? client = await _context.Clients
        .SingleOrDefaultAsync(c => c.ID == id, ct);
      if (client is null)
        return RequestResult<ContractGetDTO>.Failure("Client not found");

      ContractModel contract = ContractMapper.PostDTOToModel(dto);
      client.AddContract(contract);

      await _context.Contracts.AddAsync(contract, ct);
      await _context.SaveChangesAsync(ct);

      return RequestResult<ContractGetDTO>.Success(
        ContractMapper.ModelToGetDTO(contract)
      );
    }

    public async Task<RequestResult<ClientGetDTO>> GetByIDAsync(
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
        return RequestResult<ClientGetDTO>.Failure("Client not found");

      return RequestResult<ClientGetDTO>.Success(client);
    }

    public async Task<IReadOnlyList<ClientGetDTO>> GetAllAsync(CancellationToken ct)
      => await _context.Clients
          .Include(c => c.Contracts)
          .AsNoTracking()
          .OrderBy(c => c.Status)
          .Select(c => ClientMapper.ModelToGetDTO(c))
          .ToListAsync(ct);
  }
}
