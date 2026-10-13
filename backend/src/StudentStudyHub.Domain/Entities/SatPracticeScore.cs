using StudentStudyHub.Domain.Common;

namespace StudentStudyHub.Domain.Entities;

public class SatPracticeScore : BaseEntity
{
    public Guid SatPlanId { get; set; }
    public SatPlan? SatPlan { get; set; }

    public DateOnly TestDate { get; set; }
    public int ReadingWritingScore { get; set; }
    public int MathScore { get; set; }
    public int TotalScore => ReadingWritingScore + MathScore;
}
