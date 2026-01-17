using ScopeTrack.Application.DTOs;
using ScopeTrack.Domain.Entities;

namespace ScopeTrack.Application.Interfaces
{
  public interface IContractService
  {
    Task StageAsync(ContractModel contract, CancellationToken ct);
    Task<RequestResult<ContractGetDTO>> UpdateStatusAsync(Guid id, ContractPatchDTO dto, CancellationToken ct);
    Task<RequestResult<DeliverableGetDTO>> AddDeliverableAsync(Guid id, DeliverablePostDTO dto, CancellationToken ct);
    Task<RequestResult<ContractGetDTO>> GetByIDAsync(Guid id, CancellationToken ct);
  }
}
