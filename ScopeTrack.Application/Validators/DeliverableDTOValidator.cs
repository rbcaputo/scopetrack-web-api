using FluentValidation;
using ScopeTrack.Application.Dtos;

namespace ScopeTrack.Application.Validators
{
  public sealed class DeliverablePostDtoValidator : AbstractValidator<DeliverablePostDto>
  {
    public DeliverablePostDtoValidator()
    {
      RuleFor(d => d.Title)
        .NotEmpty()
        .WithMessage("Deliverable title is required")
        .MinimumLength(10)
        .WithMessage("Deliverable title must have at least 10 characters")
        .MaximumLength(200)
        .WithMessage("Deliverable title must not exceed 200 characters");

      RuleFor(d => d.Description)
        .MaximumLength(500)
        .WithMessage("Deliverable description must not exceed 500 characters")
        .When(d => !string.IsNullOrEmpty(d.Description));
    }
  }

  public sealed class DeliverablePatchDtoValidator : AbstractValidator<DeliverablePatchDto>
  {
    public DeliverablePatchDtoValidator()
    {
      RuleFor(d => d.NewStatus)
        .NotEmpty()
        .WithMessage("Deliverable new status is required")
        .Must(status => status == "Pending"
                     || status == "InProgress"
                     || status == "Completed"
                     || status == "Cancelled"
        )
        .WithMessage(
          "Deliverable status must be 'Pending', 'InProgress', 'Completed', or 'Cancelled'"
        );
    }
  }
}
