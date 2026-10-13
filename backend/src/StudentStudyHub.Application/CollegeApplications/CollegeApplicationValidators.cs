using FluentValidation;

namespace StudentStudyHub.Application.CollegeApplications;

public class CreateCollegeApplicationRequestValidator : AbstractValidator<CreateCollegeApplicationRequest>
{
    public CreateCollegeApplicationRequestValidator()
    {
        RuleFor(x => x.CollegeName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ApplicationType).IsInEnum();
        RuleFor(x => x.Status).IsInEnum();
        RuleForEach(x => x.Requirements).MaximumLength(200).When(x => x.Requirements is not null);
    }
}

public class UpdateCollegeApplicationRequestValidator : AbstractValidator<UpdateCollegeApplicationRequest>
{
    public UpdateCollegeApplicationRequestValidator()
    {
        RuleFor(x => x.CollegeName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ApplicationType).IsInEnum();
        RuleFor(x => x.Status).IsInEnum();
        RuleForEach(x => x.Requirements).MaximumLength(200).When(x => x.Requirements is not null);
    }
}
