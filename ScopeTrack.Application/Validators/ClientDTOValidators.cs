using FluentValidation;
using ScopeTrack.Application.DTOs;

namespace ScopeTrack.Application.Validators
{
  public sealed class ClientPostDTOValidator : AbstractValidator<ClientPostDTO>
  {
    public ClientPostDTOValidator()
    {
      RuleFor(c => c.Name)
        .NotEmpty()
        .WithMessage("Client name is required")
        .MaximumLength(200)
        .WithMessage("Client name must not exceed 200 characters.");

      RuleFor(c => c.ContactEmail)
        .NotEmpty()
        .WithMessage("Client contact email is required")
        .EmailAddress()
        .WithMessage("Client contact email must be a valid email address")
        .MaximumLength(200)
        .WithMessage("Client contact email must not exceed 200 characters");
    }
  }

  public sealed class ClientPutDTOValidator : AbstractValidator<ClientPutDTO>
  {
    public ClientPutDTOValidator()
    {
      RuleFor(c => c.Name)
        .NotEmpty()
        .WithMessage("Client name is required")
        .MaximumLength(200)
        .WithMessage("Client name must not exceed 200 characters.");

      RuleFor(c => c.ContactEmail)
        .NotEmpty()
        .WithMessage("Client contact email is required")
        .EmailAddress()
        .WithMessage("Client contact email must be a valid email address")
        .MaximumLength(200)
        .WithMessage("Client contact email must not exceed 200 characters");
    }
  }
}
