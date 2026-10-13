namespace StudentStudyHub.Application.SatPlanning;

public record SatPracticeScoreDto(
    Guid Id,
    DateOnly TestDate,
    int ReadingWritingScore,
    int MathScore,
    int TotalScore,
    DateTime CreatedAt);

public record SatPlanDto(
    Guid Id,
    DateOnly TargetTestDate,
    int GoalScore,
    IReadOnlyList<SatPracticeScoreDto> PracticeScores,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateSatPlanRequest(DateOnly TargetTestDate, int GoalScore);

public record UpdateSatPlanRequest(DateOnly TargetTestDate, int GoalScore);

public record LogSatPracticeScoreRequest(DateOnly TestDate, int ReadingWritingScore, int MathScore);
