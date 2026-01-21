using ScopeTrack.Application.Dtos;
using ScopeTrack.Domain.Entities;

namespace ScopeTrack.Application.Mappers
{
  public static class DeliverableMapper
  {
    public static DeliverableModel PostDtoToModel(Guid contractId, DeliverablePostDto dto)
      => new(
        contractId,
        dto.Title,
        dto.Description,
        dto.DueDate
      );

    public static DeliverableGetDto ModelToGetDto(DeliverableModel model)
      => new(
        model.Id,
        model.ContractId,
        model.Title,
        model.Description,
        model.Status.ToString(),
        model.DueDate,
        model.CreatedAt,
        model.UpdatedAt
      );
  }
}
