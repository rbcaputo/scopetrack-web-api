using ScopeTrack.Application.DTOs;
using ScopeTrack.Domain.Entities;

namespace ScopeTrack.Application.Interfaces
{
  public interface IContractService
  {
    Task StageAsync(ContractModel contract, CancellationToken ct);
    Task<ContractGetDTO?> UpdateStatusAsync(
      Guid id,
      ContractPatchDTO dto,
      CancellationToken ct
    );
    Task<ContractGetDTO?> GetByIDAsync(Guid id, CancellationToken ct);
  }
}
