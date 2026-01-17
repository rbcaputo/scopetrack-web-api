using FluentValidation;
using ScopeTrack.Application.DTOs;

namespace ScopeTrack.Application.Validators
{
  public sealed class ContractPostDTOValidadator : AbstractValidator<ContractPostDTO>
  {
    public ContractPostDTOValidadator()
    {
      RuleFor(c => c.ClientID)
        .NotEmpty()
        .WithMessage("Client ID is required");

      RuleFor(c => c.Title)
        .NotEmpty()
        .WithMessage("Contract title is required")
        .MinimumLength(10)
        .WithMessage("Contract title must have at least 10 characters")
        .MaximumLength(200)
        .WithMessage("Contract title must not exceed 200 characters");

      RuleFor(c => c.Description)
        .MaximumLength(1000)
        .WithMessage("Contract description must not exceed 1000 characters")
        .When(c => !string.IsNullOrEmpty(c.Description));

      RuleFor(c => c.Type)
        .NotEmpty()
        .WithMessage("Contract type is required")
        .Must(type => type == "FixedPrice" || type == "TimeBased")
        .WithMessage("Contract type must be either 'FixedPrice' or 'TimeBased'");
    }
  }

  public sealed class ContractPatchDTOValidator : AbstractValidator<ContractPatchDTO>
  {
    public ContractPatchDTOValidator()
    {
      RuleFor(c => c.NewStatus)
        .NotEmpty()
        .WithMessage("Contract new status is required")
        .Must(status => status == "Active"
                     || status == "Completed"
                     || status == "Archived"
        )
        .WithMessage(
          "Contract new status must be 'Active', 'Completed', or 'Archived'"
        );
    }
  }
}
