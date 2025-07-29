using FluentValidation;
using LibraryManagement.Models.ViewModels;

namespace LibraryManagement.Models.Validations
{
    public class AddViewModelValidator : AbstractValidator<AddViewModel>
    {
        public AddViewModelValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.");

            RuleFor(x => x.Author)
                .NotEmpty().WithMessage("Author is requiredmmmmmmmmmmmmm.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.");

            RuleFor(x => x.AvailableCopies)
                .GreaterThan(0).WithMessage("Available copies must be at least 1.");

            // Edit mode — skip validation entirely
            // No need to define a rule at all here

            // Add mode — require image upload
            When(model => model.Id == null, () =>
            {
                RuleFor(x => x.CoverImageUrl)
                    .NotNull().WithMessage("Cover image is required when adding.");
            });
        }
    }

}

