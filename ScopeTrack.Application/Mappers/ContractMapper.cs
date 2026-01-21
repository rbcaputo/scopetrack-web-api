using ScopeTrack.Application.Dtos;
using ScopeTrack.Domain.Entities;
using ScopeTrack.Domain.Enums;

namespace ScopeTrack.Application.Mappers
{
  public static class ContractMapper
  {
    public static ContractModel PostDtoToModel(Guid clientId, ContractPostDto dto)
    {
      if (!Enum.TryParse(dto.Type, out ContractType type))
        throw new ArgumentException(
          $"Invalid contract type: {dto.Type}",
          dto.Type
        );

      return new(
        clientId,
        dto.Title,
        dto.Description,
        type
      );
    }

    public static ContractGetDto ModelToGetDto(ContractModel model)
    {
      IReadOnlyList<DeliverableGetDto> deliverables = [];
      if (model.Deliverables.Count > 0)
        deliverables = [.. model.Deliverables
          .Select(DeliverableMapper.ModelToGetDto)
        ];

      return new(
        model.Id,
        model.ClientId,
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
