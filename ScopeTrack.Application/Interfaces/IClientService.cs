using ScopeTrack.Application.DTOs;

namespace ScopeTrack.Application.Interfaces
{
  public interface IClientService
  {
    Task<Result<ClientGetDTO>> CreateAsync(ClientPostDTO dto, CancellationToken ct);
    Task<Result<ClientGetDTO>> UpdateAsync(Guid id, ClientPutDTO dto, CancellationToken ct);
    Task<Result<ClientGetDTO>> ToggleStatusAsync(Guid id, CancellationToken ct);
    Task<Result<ContractGetDTO>> AddContractAsync(Guid id, ContractPostDTO dto, CancellationToken ct);
    Task<Result<ClientGetDTO>> GetByIDAsync(Guid id, CancellationToken ct);
    Task<Result<IReadOnlyList<ClientGetDTO>>> GetAllAsync(CancellationToken ct);
  }
}
