using FluentValidation;

namespace StudentStudyHub.Application.StudyGroups;

public class CreateGroupNoteRequestValidator : AbstractValidator<CreateGroupNoteRequest>
{
    public CreateGroupNoteRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Content).MaximumLength(10000);
    }
}

public class UpdateGroupNoteRequestValidator : AbstractValidator<UpdateGroupNoteRequest>
{
    public UpdateGroupNoteRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Content).MaximumLength(10000);
    }
}
