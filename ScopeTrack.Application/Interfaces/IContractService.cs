using ScopeTrack.Application.Dtos;

namespace ScopeTrack.Application.Interfaces
{
  public interface IContractService
  {
    Task<RequestResult<ContractGetDto>> UpdateStatusAsync(Guid id, ContractPatchDto dto, CancellationToken ct);
    Task<RequestResult<DeliverableGetDto>> AddDeliverableAsync(Guid id, DeliverablePostDto dto, CancellationToken ct);
    Task<RequestResult<ContractGetDto>> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<ContractGetDto>> GetAllAsync(CancellationToken ct);
  }
}
