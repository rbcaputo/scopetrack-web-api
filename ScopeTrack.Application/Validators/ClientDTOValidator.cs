using FluentValidation;
using ScopeTrack.Application.Dtos;

namespace ScopeTrack.Application.Validators
{
  public sealed class ClientPostDtoValidator : AbstractValidator<ClientPostDto>
  {
    public ClientPostDtoValidator()
    {
      RuleFor(c => c.Name)
        .NotEmpty()
        .WithMessage("Client name is required")
        .MinimumLength(3)
        .WithMessage("Client name must have at least 3 characters")
        .MaximumLength(100)
        .WithMessage("Client name must not exceed 100 characters.");

      RuleFor(c => c.Email)
        .NotEmpty()
        .WithMessage("Client email is required")
        .EmailAddress()
        .WithMessage("Client email must be a valid email address")
        .MinimumLength(5)
        .WithMessage("Client email must have at least 5 characters")
        .MaximumLength(100)
        .WithMessage("Client email must not exceed 100 characters");
    }
  }

  public sealed class ClientPutDtoValidator : AbstractValidator<ClientPutDto>
  {
    public ClientPutDtoValidator()
    {
      RuleFor(c => c.Name)
        .NotEmpty()
        .WithMessage("Client name is required")
        .MinimumLength(3)
        .WithMessage("Client name must have at least 3 characters")
        .MaximumLength(100)
        .WithMessage("Client name must not exceed 100 characters.");

      RuleFor(c => c.Email)
        .NotEmpty()
        .WithMessage("Client email is required")
        .EmailAddress()
        .WithMessage("Client email must be a valid email address")
        .MinimumLength(5)
        .WithMessage("Client email must have at least 5 characters")
        .MaximumLength(100)
        .WithMessage("Client email must not exceed 100 characters");
    }
  }
}
