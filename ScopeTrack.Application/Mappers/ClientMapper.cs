using ScopeTrack.Application.DTOs;
using ScopeTrack.Domain.Entities;

namespace ScopeTrack.Application.Mappers
{
  public static class ClientMapper
  {
    public static ClientModel PostDTOToModel(ClientPostDTO dto)
      => new(dto.Name, dto.Email);

    public static ClientModel PutDTOToModel(ClientPutDTO dto)
      => new(dto.Name, dto.Email);

    public static ClientGetDTO ModelToGetDTO(ClientModel model)
    {
      IReadOnlyList<ContractGetDTO> contracts = [];
      if (model.Contracts.Count > 0)
        contracts = [.. model.Contracts
          .Select(ContractMapper.ModelToGetDTO)
        ];

      return new(
        model.ID,
        model.Name,
        model.Email,
        model.Status.ToString(),
        model.CreatedAt,
        model.UpdatedAt,
        contracts
      );
    }
  }
}
