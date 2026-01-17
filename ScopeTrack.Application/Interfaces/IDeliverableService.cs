using ScopeTrack.Application.DTOs;
using ScopeTrack.Domain.Entities;

namespace ScopeTrack.Application.Interfaces
{
  public interface IDeliverableService
  {
    Task StageAsync(DeliverableModel model, CancellationToken ct);
    Task<Result<DeliverableGetDTO>> UpdateStatusAsync(Guid id, DeliverablePatchDTO dto, CancellationToken ct);
    Task<Result<DeliverableGetDTO>> GetByIDAsync(Guid id, CancellationToken ct);
  }
}
