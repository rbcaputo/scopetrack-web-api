using ScopeTrack.Application.Dtos;
using ScopeTrack.Domain.Entities;

namespace ScopeTrack.Application.Mappers
{
  public static class ClientMapper
  {
    public static ClientModel PostDtoToModel(ClientPostDto dto)
      => new(dto.Name, dto.Email);

    public static ClientModel PutDtoToModel(ClientPutDto dto)
      => new(dto.Name, dto.Email);

    public static ClientGetDto ModelToGetDto(ClientModel model)
    {
      IReadOnlyList<ContractGetDto> contracts = [];
      if (model.Contracts.Count > 0)
        contracts = [.. model.Contracts
          .Select(ContractMapper.ModelToGetDto)
        ];

      return new(
        model.Id,
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
