using ScopeTrack.Application.DTOs;

namespace ScopeTrack.Application.Interfaces
{
  public interface IClientService
  {
    Task<ClientGetDTO> CreateAsync(ClientPostDTO dto, CancellationToken ct);
    Task<ClientGetDTO?> UpdateAsync(ClientPutDTO dto, CancellationToken ct);
    Task<ClientGetDTO?> ToggleStatusAsync(Guid id, CancellationToken ct);
    Task<ContractGetDTO?> AddContractAsync(Guid id, ContractPostDTO dto, CancellationToken ct);
    Task<ClientGetDTO?> GetByIDAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<ClientGetDTO>> GetAllAsync(CancellationToken ct);
  }
}
