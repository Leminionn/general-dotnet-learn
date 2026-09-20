using FirstAPIProject.Application.Modules.Announcement.DTOs;
using FluentValidation;

namespace FirstAPIProject.Application.Modules.Announcement.Validators
{
    public class CreateAnnouncementRequestValidator : AbstractValidator<CreateAnnouncementRequest>
    {
        public CreateAnnouncementRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(255).WithMessage("Title cannot exceed 255 characters.");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Content is required.");
        }
    }

    public class UpdateAnnouncementRequestValidator : AbstractValidator<UpdateAnnouncementRequest>
    {
        public UpdateAnnouncementRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(255).WithMessage("Title cannot exceed 255 characters.");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Content is required.");
        }
    }
}
