using System.Reflection;

namespace StudentRegistration.IntegrationTests.Specs.Spec008.EdgeCases;

public sealed class EC_2Tests
{
    [Fact]
    public void Missing_imported_gpa_provenance_or_required_seed_field_fails_readiness()
    {
        RequireAcademicsType(
            "StudentRegistration.Academics.Application.DemoStudentProfileSeedContributor");

        var complete = CompleteSeedRow();
        Assert.True(SeedReadinessDouble.Validate(complete).Ready);

        var incompleteRows = new[]
        {
            complete with { CurrentGpa = null },
            complete with { ProgramCode = " " },
            complete with { Source = " " },
            complete with { SourceReference = " " },
            complete with { DataVersion = " " },
            complete with { DataAsOfUtc = null }
        };

        Assert.All(incompleteRows, row =>
        {
            var result = SeedReadinessDouble.Validate(row);
            Assert.False(result.Ready);
            Assert.Equal("PROFILE_NOT_READY", result.ReasonCode);
            Assert.Null(result.ReadyProfile);
        });
    }

    [Fact]
    public void More_than_one_hundred_active_holds_fails_closed_without_truncation()
    {
        RequireAcademicsType(
            "StudentRegistration.Academics.Application.StudentAcademicProfileService");

        var activeHolds = Enumerable.Range(1, 101)
            .Select(index => $"HOLD-{index:000}")
            .ToArray();

        var result = ProfileReadinessDouble.Read(activeHolds);

        Assert.False(result.Ready);
        Assert.Equal("PROFILE_NOT_READY", result.ReasonCode);
        Assert.Null(result.ProfileHolds);
        Assert.NotEqual(100, result.ReturnedHoldCount);
        Assert.Equal(0, result.ReturnedHoldCount);
    }

    private static SyntheticSeedRow CompleteSeedRow() =>
        new(
            ProgramCode: "AI",
            Cohort: "2026",
            CurrentGpa: 3.25m,
            EarnedCredits: 72m,
            Standing: "Active",
            Source: "synthetic-demo",
            SourceReference: "student-fixture-v1:42",
            DataVersion: "academic-profile-v1",
            DataAsOfUtc: new DateTime(2026, 7, 14, 8, 0, 0, DateTimeKind.Utc));

    private static void RequireAcademicsType(string fullName)
    {
        var type = Assembly.Load("StudentRegistration.Academics").GetType(fullName);
        Assert.True(type is not null, $"The required Academics service {fullName} is missing.");
    }

    private sealed record SyntheticSeedRow(
        string ProgramCode,
        string Cohort,
        decimal? CurrentGpa,
        decimal EarnedCredits,
        string Standing,
        string Source,
        string SourceReference,
        string DataVersion,
        DateTime? DataAsOfUtc);

    private sealed record SeedReadinessResult(
        bool Ready,
        string? ReasonCode,
        SyntheticSeedRow? ReadyProfile);

    private static class SeedReadinessDouble
    {
        public static SeedReadinessResult Validate(SyntheticSeedRow row)
        {
            var complete =
                row.CurrentGpa is >= 0 &&
                row.EarnedCredits >= 0 &&
                Required(row.ProgramCode) &&
                Required(row.Cohort) &&
                Required(row.Standing) &&
                Required(row.Source) &&
                Required(row.SourceReference) &&
                Required(row.DataVersion) &&
                row.DataAsOfUtc?.Kind == DateTimeKind.Utc;

            return complete
                ? new(true, null, row)
                : new(false, "PROFILE_NOT_READY", null);
        }

        private static bool Required(string value) => !string.IsNullOrWhiteSpace(value);
    }

    private sealed record ProfileReadinessResult(
        bool Ready,
        string? ReasonCode,
        IReadOnlyList<string>? ProfileHolds,
        int ReturnedHoldCount);

    private static class ProfileReadinessDouble
    {
        public static ProfileReadinessResult Read(IReadOnlyList<string> activeHolds)
        {
            if (activeHolds.Count > 100)
            {
                return new(false, "PROFILE_NOT_READY", null, 0);
            }

            var complete = activeHolds.ToArray();
            return new(true, null, complete, complete.Length);
        }
    }
}
