using FirstAPIProject.Application.Modules.Whitelist.DTOs;
using FluentValidation;

namespace FirstAPIProject.Application.Modules.Whitelist.Validators
{
    public class CreateWhitelistRequestValidator : AbstractValidator<CreateWhitelistRequest>
    {
        public CreateWhitelistRequestValidator()
        {
            RuleFor(x => x.Pattern)
                .NotEmpty().WithMessage("Pattern is required.")
                .MaximumLength(255).WithMessage("Pattern cannot exceed 255 characters.")
                .Matches(@"^[a-zA-Z0-9_\.\@\*\-]+$").WithMessage("Pattern contains invalid characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
        }
    }

    public class UpdateWhitelistRequestValidator : AbstractValidator<UpdateWhitelistRequest>
    {
        public UpdateWhitelistRequestValidator()
        {
            RuleFor(x => x.Pattern)
                .NotEmpty().WithMessage("Pattern is required.")
                .MaximumLength(255).WithMessage("Pattern cannot exceed 255 characters.")
                .Matches(@"^[a-zA-Z0-9_\.\@\*\-]+$").WithMessage("Pattern contains invalid characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
        }
    }
}
