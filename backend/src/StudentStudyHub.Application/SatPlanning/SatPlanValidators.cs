using FluentValidation;

namespace StudentStudyHub.Application.SatPlanning;

public class CreateSatPlanRequestValidator : AbstractValidator<CreateSatPlanRequest>
{
    public CreateSatPlanRequestValidator()
    {
        RuleFor(x => x.GoalScore).InclusiveBetween(400, 1600);
    }
}

public class UpdateSatPlanRequestValidator : AbstractValidator<UpdateSatPlanRequest>
{
    public UpdateSatPlanRequestValidator()
    {
        RuleFor(x => x.GoalScore).InclusiveBetween(400, 1600);
    }
}

public class LogSatPracticeScoreRequestValidator : AbstractValidator<LogSatPracticeScoreRequest>
{
    public LogSatPracticeScoreRequestValidator()
    {
        RuleFor(x => x.ReadingWritingScore).InclusiveBetween(200, 800);
        RuleFor(x => x.MathScore).InclusiveBetween(200, 800);
    }
}
