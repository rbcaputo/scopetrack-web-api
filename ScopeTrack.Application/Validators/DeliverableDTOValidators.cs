using FluentValidation;
using ScopeTrack.Application.DTOs;

namespace ScopeTrack.Application.Validators
{
  public sealed class DeliverablePostDTOValidator : AbstractValidator<DeliverablePostDTO>
  {
    public DeliverablePostDTOValidator()
    {
      RuleFor(d => d.Title)
        .NotEmpty()
        .WithMessage("Deliverable title is required")
        .MaximumLength(200)
        .WithMessage("Deliverable title must not exceed 200 characters");

      RuleFor(d => d.Description)
        .MaximumLength(1000)
        .WithMessage("Deliverable description must not exceed 1000 characters")
        .When(d => !string.IsNullOrEmpty(d.Description));
    }
  }

  public sealed class DeliverablePatchDTOValidator : AbstractValidator<DeliverablePatchDTO>
  {
    public DeliverablePatchDTOValidator()
    {
      RuleFor(d => d.NewStatus)
        .NotEmpty()
        .WithMessage("Deliverable status is requirted")
        .Must(status => status == "Planned" || status == "InProgress" || status == "Completed")
        .WithMessage("Deliverable status must be 'Planned', 'InProgress', or 'Completed'");
    }
  }
}
