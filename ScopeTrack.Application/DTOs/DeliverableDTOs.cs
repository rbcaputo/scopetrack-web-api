using System.Text.Json.Serialization;

namespace ScopeTrack.Application.Dtos
{
  public sealed record DeliverablePostDto
  {
    public string Title { get; }
    public string Description { get; }
    public DateTime? DueDate { get; }

    [JsonConstructor]
    public DeliverablePostDto(string title, string description, DateTime? dueDate)
    {
      Title = title;
      Description = description;
      DueDate = dueDate;
    }
  }

  public sealed record DeliverablePatchDto
  {
    public string NewStatus { get; }

    [JsonConstructor]
    public DeliverablePatchDto(string newStatus)
      => NewStatus = newStatus;
  }

  public sealed record DeliverableGetDto(
    Guid Id,
    Guid ContractId,
    string Title,
    string Description,
    string Status,
    DateTime? DueDate,
    DateTime CreatedAt,
    DateTime UpdatedAt
  );
}
