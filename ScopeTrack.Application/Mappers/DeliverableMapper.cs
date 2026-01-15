using ScopeTrack.Application.DTOs;
using ScopeTrack.Domain.Entities;

namespace ScopeTrack.Application.Mappers
{
  public static class DeliverableMapper
  {
    public static DeliverableModel PostDTOToModel(DeliverablePostDTO dto)
      => new(
        dto.contractID,
        dto.Title,
        dto.Description,
        dto.DueDate
      );

    public static DeliverableGetDTO ModelToGetDTO(DeliverableModel model)
      => new(
        model.ID,
        model.ContractID,
        model.Title,
        model.Description,
        model.Status.ToString(),
        model.DueDate,
        model.CreatedAt,
        model.UpdatedAt
      );
  }
}
