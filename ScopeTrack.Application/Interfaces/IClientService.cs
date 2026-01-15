using ScopeTrack.Application.DTOs;
using ScopeTrack.Domain.Enums;

namespace ScopeTrack.Application.Interfaces
{
  public interface IClientService
  {
    Task<ClientGetDTO> CreateAsync(ClientPostDTO dto, CancellationToken ct);

    Task<ClientGetDTO?> UpdateAsync(
      Guid id,
      ClientPutDTO dto,
      CancellationToken ct
    );

    Task<ClientGetDTO?> ToggleStatusAsync(Guid id, CancellationToken ct);

    Task<ClientGetDTO?> AddContractAsync(
      Guid id,
      ContractPostDTO dto,
      CancellationToken ct
    );

    Task<IEnumerable<ClientGetDTO>> ReadAllAsync(CancellationToken ct);

    Task<ClientGetDTO?> ReadByIDAsync(Guid id, CancellationToken ct);

  }
}
