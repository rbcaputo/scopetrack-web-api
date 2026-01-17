using ScopeTrack.Application.DTOs;

namespace ScopeTrack.Application.Interfaces
{
  public interface IDeliverableService
  {
    Task<RequestResult<DeliverableGetDTO>> UpdateStatusAsync(Guid id, DeliverablePatchDTO dto, CancellationToken ct);
    Task<RequestResult<DeliverableGetDTO>> GetByIDAsync(Guid id, CancellationToken ct);
  }
}
