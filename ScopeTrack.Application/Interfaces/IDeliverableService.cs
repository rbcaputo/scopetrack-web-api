using ScopeTrack.Application.DTOs;
using ScopeTrack.Domain.Enums;

namespace ScopeTrack.Application.Interfaces
{
  public interface IDeliverableService
  {
    Task<DeliverableGetDTO?> CreateAsync(
      Guid contractID,
      DeliverablePostDTO dto,
      CancellationToken ct
    );

    Task<DeliverableGetDTO?> UpdateStatusAsync(
      Guid id,
      DeliverableStatus newStatus,
      CancellationToken ct
    );

    Task<DeliverableGetDTO?> ReadByIDAsync(Guid id, CancellationToken ct);
  }
}
