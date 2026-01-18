using ScopeTrack.Application.DTOs;

namespace ScopeTrack.Application.Interfaces
{
  public interface IContractService
  {
    Task<RequestResult<ContractGetDTO>> UpdateStatusAsync(Guid id, ContractPatchDTO dto, CancellationToken ct);
    Task<RequestResult<DeliverableGetDTO>> AddDeliverableAsync(Guid id, DeliverablePostDTO dto, CancellationToken ct);
    Task<RequestResult<ContractGetDTO>> GetByIDAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<ContractGetDTO>> GetAllAsync(CancellationToken ct);
  }
}
