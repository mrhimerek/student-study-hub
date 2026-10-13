using FluentValidation;

namespace StudentStudyHub.Application.Courses;

public class CreateCourseRequestValidator : AbstractValidator<CreateCourseRequest>
{
    public CreateCourseRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.CreditHours).GreaterThan(0).LessThanOrEqualTo(10);
        RuleFor(x => x.GradeValue).NotEmpty().MaximumLength(5);
        RuleFor(x => x.Term).NotEmpty().MaximumLength(50);
    }
}

public class UpdateCourseRequestValidator : AbstractValidator<UpdateCourseRequest>
{
    public UpdateCourseRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.CreditHours).GreaterThan(0).LessThanOrEqualTo(10);
        RuleFor(x => x.GradeValue).NotEmpty().MaximumLength(5);
        RuleFor(x => x.Term).NotEmpty().MaximumLength(50);
    }
}
