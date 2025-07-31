using FluentValidation;
using LibraryManagement.Models.ViewModels;

namespace LibraryManagement.Models.Validations
{
    public class AddViewModelValidator : AbstractValidator<AddViewModel>
    {
        public AddViewModelValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required fluent.");

            RuleFor(x => x.Author)
                .NotEmpty().WithMessage("Author is required fluent.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required fluent.");

            RuleFor(x => x.AvailableCopies)
                .GreaterThan(0).WithMessage("Available copies must be at least 1 fluent.");

            RuleFor(x => x.CoverImageUrl)
           .NotEmpty().WithMessage("Cover Image URL is required fluent.");
        }


    }

}


