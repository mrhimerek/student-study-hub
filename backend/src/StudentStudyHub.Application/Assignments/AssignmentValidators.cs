using FluentValidation;

namespace StudentStudyHub.Application.Assignments;

public class CreateAssignmentRequestValidator : AbstractValidator<CreateAssignmentRequest>
{
    public CreateAssignmentRequestValidator()
    {
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Priority).IsInEnum();
    }
}

public class UpdateAssignmentRequestValidator : AbstractValidator<UpdateAssignmentRequest>
{
    public UpdateAssignmentRequestValidator()
    {
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Priority).IsInEnum();
        RuleFor(x => x.Status).IsInEnum();
    }
}
