using FluentValidation;

namespace StudentStudyHub.Application.Users;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.GradeLevel).InclusiveBetween(9, 12)
            .WithMessage("Grade level must be between 9 and 12.");
        RuleFor(x => x.SchoolName).MaximumLength(200);
    }
}
