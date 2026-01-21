using ScopeTrack.Application.Dtos;

namespace ScopeTrack.Application.Interfaces
{
  public interface IDeliverableService
  {
    Task<RequestResult<DeliverableGetDto>> UpdateStatusAsync(Guid id, DeliverablePatchDto dto, CancellationToken ct);
    Task<RequestResult<DeliverableGetDto>> GetByIdAsync(Guid id, CancellationToken ct);
  }
}
