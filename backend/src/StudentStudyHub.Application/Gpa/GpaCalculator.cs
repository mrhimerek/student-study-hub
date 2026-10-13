namespace StudentStudyHub.Application.Gpa;

/// <summary>
/// Pure, deterministic GPA calculation logic — kept dependency-free so it is
/// trivially unit-testable and reusable by both the GpaSummary query handler
/// and any future "what-if" GPA simulator feature.
/// </summary>
public static class GpaCalculator
{
    /// <summary>
    /// Standard 4.0-scale grade points for unweighted GPA.
    /// Extend this map if the school uses +/- grades not listed here.
    /// </summary>
    private static readonly Dictionary<string, decimal> UnweightedGradePoints = new()
    {
        ["A+"] = 4.0m,
        ["A"] = 4.0m,
        ["A-"] = 3.7m,
        ["B+"] = 3.3m,
        ["B"] = 3.0m,
        ["B-"] = 2.7m,
        ["C+"] = 2.3m,
        ["C"] = 2.0m,
        ["C-"] = 1.7m,
        ["D+"] = 1.3m,
        ["D"] = 1.0m,
        ["D-"] = 0.7m,
        ["F"] = 0.0m,
    };

    /// <summary>Extra grade points awarded per Honors/AP course on the weighted scale.</summary>
    private const decimal WeightedBonus = 1.0m;

    public record CourseGrade(decimal CreditHours, string GradeValue, bool IsHonorsOrAp);

    public record GpaSummary(decimal WeightedGpa, decimal UnweightedGpa, decimal TotalCredits);

    public static GpaSummary Calculate(IReadOnlyCollection<CourseGrade> courses)
    {
        if (courses.Count == 0)
        {
            return new GpaSummary(0m, 0m, 0m);
        }

        decimal totalCredits = 0m;
        decimal unweightedPoints = 0m;
        decimal weightedPoints = 0m;

        foreach (var course in courses)
        {
            if (!UnweightedGradePoints.TryGetValue(NormalizeGrade(course.GradeValue), out var basePoints))
            {
                continue; // skip grades that don't map to a GPA value (e.g. Pass/Fail, Incomplete)
            }

            var weightedBasePoints = course.IsHonorsOrAp ? basePoints + WeightedBonus : basePoints;

            totalCredits += course.CreditHours;
            unweightedPoints += basePoints * course.CreditHours;
            weightedPoints += weightedBasePoints * course.CreditHours;
        }

        if (totalCredits == 0m)
        {
            return new GpaSummary(0m, 0m, 0m);
        }

        return new GpaSummary(
            WeightedGpa: Math.Round(weightedPoints / totalCredits, 2),
            UnweightedGpa: Math.Round(unweightedPoints / totalCredits, 2),
            TotalCredits: totalCredits);
    }

    private static string NormalizeGrade(string grade) => grade.Trim().ToUpperInvariant();
}
