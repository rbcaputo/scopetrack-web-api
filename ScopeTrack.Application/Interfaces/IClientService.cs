using ScopeTrack.Application.Dtos;

namespace ScopeTrack.Application.Interfaces
{
  public interface IClientService
  {
    Task<RequestResult<ClientGetDto>> CreateAsync(ClientPostDto dto, CancellationToken ct);
    Task<RequestResult<ClientGetDto>> UpdateAsync(Guid id, ClientPutDto dto, CancellationToken ct);
    Task<RequestResult<ClientGetDto>> ToggleStatusAsync(Guid id, CancellationToken ct);
    Task<RequestResult<ContractGetDto>> AddContractAsync(Guid id, ContractPostDto dto, CancellationToken ct);
    Task<RequestResult<ClientGetDto>> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<ClientGetDto>> GetAllAsync(CancellationToken ct);
  }
}
