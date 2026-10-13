using StudentStudyHub.Domain.Common;

namespace StudentStudyHub.Domain.Entities;

public class SatPlan : BaseEntity
{
    public Guid UserId { get; set; } // one active plan per user
    public User? User { get; set; }

    public DateOnly TargetTestDate { get; set; }
    public int GoalScore { get; set; }

    public ICollection<SatPracticeScore> PracticeScores { get; set; } = new List<SatPracticeScore>();
}
