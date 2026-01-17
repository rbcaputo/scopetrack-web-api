using ScopeTrack.Application.DTOs;

namespace ScopeTrack.Application.Interfaces
{
  public interface IClientService
  {
    Task<RequestResult<ClientGetDTO>> CreateAsync(ClientPostDTO dto, CancellationToken ct);
    Task<RequestResult<ClientGetDTO>> UpdateAsync(Guid id, ClientPutDTO dto, CancellationToken ct);
    Task<RequestResult<ClientGetDTO>> ToggleStatusAsync(Guid id, CancellationToken ct);
    Task<RequestResult<ContractGetDTO>> AddContractAsync(Guid id, ContractPostDTO dto, CancellationToken ct);
    Task<RequestResult<ClientGetDTO>> GetByIDAsync(Guid id, CancellationToken ct);
    Task<RequestResult<IReadOnlyList<ClientGetDTO>>> GetAllAsync(CancellationToken ct);
  }
}
