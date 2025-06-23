using FluentValidation;

namespace GuestRoomAllocation.Application.Features.Guests.Commands.CreateGuest;

public class CreateGuestCommandValidator : AbstractValidator<CreateGuestCommand>
{
    public CreateGuestCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("First name is required and must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Last name is required and must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(150)
            .WithMessage("A valid email address is required and must not exceed 150 characters.");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .MaximumLength(20)
            .WithMessage("Phone number is required and must not exceed 20 characters.");

        RuleFor(x => x.JobPosition)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.JobPosition))
            .WithMessage("Job position must not exceed 100 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes must not exceed 500 characters.");
    }
}
