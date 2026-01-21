using System.Text.Json.Serialization;

namespace ScopeTrack.Application.Dtos
{
  public sealed record ContractPostDto
  {
    public string Title { get; }
    public string? Description { get; }
    public string Type { get; }

    [JsonConstructor]
    public ContractPostDto(string title, string description, string type)
    {
      Title = title;
      Description = description;
      Type = type;
    }
  }

  public sealed record ContractPatchDto
  {
    public string NewStatus { get; }

    [JsonConstructor]
    public ContractPatchDto(string newStatus)
      => NewStatus = newStatus;
  }

  public sealed record ContractGetDto(
    Guid Id,
    Guid ClientId,
    string Title,
    string Description,
    string Type,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<DeliverableGetDto> Deliverables
  );
}
