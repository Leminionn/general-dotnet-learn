using FirstAPIProject.Application.Modules.User.DTOs;
using FluentValidation;

namespace FirstAPIProject.Application.Modules.User.Validators
{
    public class AdminUpdateUserRequestValidator : AbstractValidator<AdminUpdateUserRequest>
    {
        public AdminUpdateUserRequestValidator()
        {
            RuleFor(x => x.UserName)
                .MaximumLength(50).WithMessage("UserName cannot exceed 50 characters.")
                .Matches(@"^[a-zA-Z0-9_\.\s\-]+$").When(x => !string.IsNullOrEmpty(x.UserName))
                .WithMessage("UserName contains invalid characters.");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20).WithMessage("PhoneNumber cannot exceed 20 characters.")
                .Matches(@"^\+?[0-9\s\-]+$").When(x => !string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("PhoneNumber is not a valid phone format.");

            RuleFor(x => x.Role)
                .IsInEnum().WithMessage("Invalid user role specified.");
        }
    }
}
