using FluentValidation;

namespace StudentStudyHub.Application.StudyGroups;

public class CreateStudyGroupRequestValidator : AbstractValidator<CreateStudyGroupRequest>
{
    public CreateStudyGroupRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Subject).MaximumLength(100);
        RuleFor(x => x.SchoolName).NotEmpty().MaximumLength(200);
    }
}

public class UpdateStudyGroupRequestValidator : AbstractValidator<UpdateStudyGroupRequest>
{
    public UpdateStudyGroupRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Subject).MaximumLength(100);
    }
}

public class JoinStudyGroupRequestValidator : AbstractValidator<JoinStudyGroupRequest>
{
    public JoinStudyGroupRequestValidator()
    {
        RuleFor(x => x.InviteCode).NotEmpty().MaximumLength(8);
    }
}
