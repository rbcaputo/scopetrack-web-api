using ScopeTrack.Application.DTOs;
using ScopeTrack.Domain.Enums;

namespace ScopeTrack.Application.Interfaces
{
  public interface IContractService
  {
    Task<ContractGetDTO?> CreateAsync(
      Guid clientID,
      ContractPostDTO dto,
      CancellationToken ct
    );

    Task<ContractGetDTO?> UpdateStatusAsync(
      Guid id,
      ContractStatus newStatus,
      CancellationToken ct
    );

    Task<ContractGetDTO?> AddDeliverableAsync(
      Guid id,
      DeliverablePostDTO dto,
      CancellationToken ct
    );

    Task<ContractGetDTO?> ReadByIDAsync(Guid id, CancellationToken ct);
  }
}
