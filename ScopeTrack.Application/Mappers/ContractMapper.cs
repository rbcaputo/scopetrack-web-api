using ScopeTrack.Application.DTOs;
using ScopeTrack.Domain.Entities;
using ScopeTrack.Domain.Enums;

namespace ScopeTrack.Application.Mappers
{
  public static class ContractMapper
  {
    public static ContractModel PostDTOToModel(ContractPostDTO dto)
    {
      if (!Enum.TryParse(dto.Type, out ContractType type))
        throw new ArgumentException(
          $"Invalid contract type: {dto.Type}",
          dto.Type
        );

      return new(
        dto.ClientID,
        dto.Title,
        dto.Description,
        type
      );
    }

    public static ContractGetDTO ModelToGetDTO(ContractModel model)
    {
      IReadOnlyList<DeliverableGetDTO> deliverables = [];
      if (model.Deliverables.Count > 0)
        deliverables = [.. model.Deliverables
          .Select(DeliverableMapper.ModelToGetDTO)
        ];

      return new(
        model.ID,
        model.ClientID,
        model.Title,
        model.Description,
        model.Type.ToString(),
        model.Status.ToString(),
        model.CreatedAt,
        model.UpdatedAt,
        deliverables
      );
    }
  }
}
