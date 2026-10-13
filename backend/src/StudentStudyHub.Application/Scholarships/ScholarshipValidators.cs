using FluentValidation;

namespace StudentStudyHub.Application.Scholarships;

public class CreateScholarshipRequestValidator : AbstractValidator<CreateScholarshipRequest>
{
    public CreateScholarshipRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0).When(x => x.Amount.HasValue);
        RuleFor(x => x.Status).IsInEnum();
        RuleForEach(x => x.Requirements).MaximumLength(200).When(x => x.Requirements is not null);
    }
}

public class UpdateScholarshipRequestValidator : AbstractValidator<UpdateScholarshipRequest>
{
    public UpdateScholarshipRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0).When(x => x.Amount.HasValue);
        RuleFor(x => x.Status).IsInEnum();
        RuleForEach(x => x.Requirements).MaximumLength(200).When(x => x.Requirements is not null);
    }
}
