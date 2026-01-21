using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ScopeTrack.Application.Dtos;
using ScopeTrack.Application.Interfaces;
using ScopeTrack.Application.Mappers;
using ScopeTrack.Domain.Entities;
using ScopeTrack.Infrastructure.Data;

namespace ScopeTrack.Application.Services
{
  public sealed class ClientService(ScopeTrackDbContext context) : IClientService
  {
    private readonly ScopeTrackDbContext _context = context;

    public async Task<RequestResult<ClientGetDto>> CreateAsync(
      ClientPostDto dto,
      CancellationToken ct
    )
    {
      bool exists = await _context.Clients
        .AnyAsync(c => c.Email == dto.Email, ct);
      if (exists)
        return RequestResult<ClientGetDto>.Failure("Client already exists");

      ClientModel client = ClientMapper.PostDtoToModel(dto);
      await _context.Clients.AddAsync(client, ct);

      try
      {
        await _context.SaveChangesAsync(ct);
      }
      catch (DbUpdateException ex)
        when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601)
      {
        return RequestResult<ClientGetDto>.Failure("Client already exists");
      }

      return RequestResult<ClientGetDto>.Success(
        ClientMapper.ModelToGetDto(client)
      );
    }

    public async Task<RequestResult<ClientGetDto>> UpdateAsync(
      Guid id,
      ClientPutDto dto,
      CancellationToken ct
    )
    {
      ClientModel? client = await _context.Clients
        .SingleOrDefaultAsync(c => c.Id == id, ct);
      if (client is null)
        return RequestResult<ClientGetDto>.Failure("Client not found");

      client.UpdateDetails(dto.Name, dto.Email);
      await _context.SaveChangesAsync(ct);

      return RequestResult<ClientGetDto>.Success(
        ClientMapper.ModelToGetDto(client)
      );
    }

    public async Task<RequestResult<ClientGetDto>> ToggleStatusAsync(
      Guid id,
      CancellationToken ct
    )
    {
      ClientModel? client = await _context.Clients
        .SingleOrDefaultAsync(c => c.Id == id, ct);
      if (client is null)
        return RequestResult<ClientGetDto>.Failure("Client not found");

      client.ToggleStatus();

      await _context.SaveChangesAsync(ct);

      return RequestResult<ClientGetDto>.Success(
        ClientMapper.ModelToGetDto(client)
      );
    }

    public async Task<RequestResult<ContractGetDto>> AddContractAsync(
      Guid id,
      ContractPostDto dto,
      CancellationToken ct
    )
    {
      ClientModel? client = await _context.Clients
        .Include(c => c.Contracts)
        .SingleOrDefaultAsync(c => c.Id == id, ct);
      if (client is null)
        return RequestResult<ContractGetDto>.Failure("Client not found");

      ContractModel contract = ContractMapper.PostDtoToModel(client.Id, dto);
      client.AddContract(contract);

      await _context.Contracts.AddAsync(contract, ct);
      await _context.SaveChangesAsync(ct);

      return RequestResult<ContractGetDto>.Success(
        ContractMapper.ModelToGetDto(contract)
      );
    }

    public async Task<RequestResult<ClientGetDto>> GetByIdAsync(
      Guid id,
      CancellationToken ct
    )
    {
      ClientGetDto? client = await _context.Clients
        .Include(c => c.Contracts)
        .AsNoTracking()
        .Where(c => c.Id == id)
        .Select(c => ClientMapper.ModelToGetDto(c))
        .SingleOrDefaultAsync(ct);
      if (client is null)
        return RequestResult<ClientGetDto>.Failure("Client not found");

      return RequestResult<ClientGetDto>.Success(client);
    }

    public async Task<IReadOnlyList<ClientGetDto>> GetAllAsync(CancellationToken ct)
      => await _context.Clients
          .Include(c => c.Contracts)
          .AsNoTracking()
          .OrderBy(c => c.Status)
          .Select(c => ClientMapper.ModelToGetDto(c))
          .ToListAsync(ct);
  }
}
