using ScopeTrack.Application.DTOs;
using ScopeTrack.Domain.Entities;
using ScopeTrack.Domain.Enums;

namespace ScopeTrack.Application.Mappers
{
  public static class ContractMapper
  {
    public static ContractModel PostDTOToModel(ContractPostDTO dto)
    {
      ContractType type = Enum.TryParse(dto.Type, out ContractType contractType)
        ? contractType
        : ContractType.FixedPrice;

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
          .Select(d => DeliverableMapper.ModelToGetDTO(d))
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
