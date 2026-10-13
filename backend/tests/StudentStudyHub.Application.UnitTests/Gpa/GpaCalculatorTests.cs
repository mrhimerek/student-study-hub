using StudentStudyHub.Application.Gpa;
using Xunit;

namespace StudentStudyHub.Application.UnitTests.Gpa;

public class GpaCalculatorTests
{
    [Fact]
    public void Calculate_WithNoCourses_ReturnsZero()
    {
        var result = GpaCalculator.Calculate([]);

        Assert.Equal(0m, result.WeightedGpa);
        Assert.Equal(0m, result.UnweightedGpa);
        Assert.Equal(0m, result.TotalCredits);
    }

    [Fact]
    public void Calculate_AllRegularCourses_WeightedEqualsUnweighted()
    {
        var courses = new List<GpaCalculator.CourseGrade>
        {
            new(CreditHours: 1.0m, GradeValue: "A", IsHonorsOrAp: false),
            new(CreditHours: 1.0m, GradeValue: "B", IsHonorsOrAp: false),
        };

        var result = GpaCalculator.Calculate(courses);

        Assert.Equal(3.5m, result.UnweightedGpa);
        Assert.Equal(3.5m, result.WeightedGpa);
        Assert.Equal(2.0m, result.TotalCredits);
    }

    [Fact]
    public void Calculate_HonorsCourse_AddsWeightedBonusOnlyToWeightedGpa()
    {
        var courses = new List<GpaCalculator.CourseGrade>
        {
            new(CreditHours: 1.0m, GradeValue: "A", IsHonorsOrAp: true), // 4.0 unweighted, 5.0 weighted
        };

        var result = GpaCalculator.Calculate(courses);

        Assert.Equal(4.0m, result.UnweightedGpa);
        Assert.Equal(5.0m, result.WeightedGpa);
    }

    [Fact]
    public void Calculate_MixedCreditHours_WeightsByCredit()
    {
        var courses = new List<GpaCalculator.CourseGrade>
        {
            new(CreditHours: 3.0m, GradeValue: "A", IsHonorsOrAp: false), // 12 pts
            new(CreditHours: 1.0m, GradeValue: "C", IsHonorsOrAp: false), // 2 pts
        };

        var result = GpaCalculator.Calculate(courses);

        // (12 + 2) / 4 credits = 3.5
        Assert.Equal(3.5m, result.UnweightedGpa);
        Assert.Equal(4.0m, result.TotalCredits);
    }

    [Fact]
    public void Calculate_UnrecognizedGrade_IsSkipped()
    {
        var courses = new List<GpaCalculator.CourseGrade>
        {
            new(CreditHours: 1.0m, GradeValue: "A", IsHonorsOrAp: false),
            new(CreditHours: 1.0m, GradeValue: "Pass", IsHonorsOrAp: false), // not in grade map
        };

        var result = GpaCalculator.Calculate(courses);

        // Only the "A" course (1 credit) should count.
        Assert.Equal(1.0m, result.TotalCredits);
        Assert.Equal(4.0m, result.UnweightedGpa);
    }
}
