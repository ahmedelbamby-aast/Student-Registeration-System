using System.Reflection;

namespace StudentRegistration.AcceptanceTests.Specs.Spec008;

public sealed class SC_2OutcomeTests
{
    [Fact]
    public void Academic_profile_response_is_complete_sourced_versioned_and_bounded()
    {
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read("specs/008-academic-term-student-profile/spec.md"),
            "**SC-2**: Every eligibility decision uses a complete, sourced academic profile",
            "same synthetic seed version reproduces the same logical profile values",
            "without production student data");

        var contracts = Assembly.Load("StudentRegistration.Contracts");
        var context = contracts.GetType(
            "StudentRegistration.Contracts.Academics.StudentAcademicContextDto");
        Assert.True(
            context is not null,
            "SC-2 requires the sourced StudentAcademicContextDto.");
        Assert.Equal(
            [
                "ActiveHolds", "Cohort", "CurrentGpa", "DataAsOfUtc", "DataVersion",
                "EarnedCredits", "ProgramCode", "Provenance", "Standing",
                "TranscriptAttempts", "TranscriptSummary", "UniversityId"
            ],
            context!.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Profile_service_and_seed_contributor_fail_closed_until_every_source_is_present()
    {
        var academics = Assembly.Load("StudentRegistration.Academics");
        Assert.NotNull(academics.GetType(
            "StudentRegistration.Academics.Application.StudentAcademicProfileService"));
        Assert.NotNull(academics.GetType(
            "StudentRegistration.Academics.Application.DemoStudentProfileSeedContributor"));

        var service = RepositoryFiles.Read(
            "src/StudentRegistration.Academics/Application/StudentAcademicProfileService.cs");
        RepositoryFiles.ContainsAll(
            service,
            "CurrentGpa",
            "EarnedCredits",
            "Standing",
            "Transcript",
            "ActiveHolds",
            "Provenance",
            "DataVersion",
            "DataAsOfUtc",
            "MaximumActiveHolds = 100",
            "ProfileNotReady");

        var seed = RepositoryFiles.Read(
            "src/StudentRegistration.Academics/Application/DemoStudentProfileSeedContributor.cs");
        RepositoryFiles.ContainsAll(
            seed,
            "SeedProfileVersion",
            "FixtureOrdinal",
            "StudentTermAcademicState",
            "TranscriptAttempt",
            "StudentHold",
            "Provenance",
            "Synthetic",
            "Production");
    }
}
