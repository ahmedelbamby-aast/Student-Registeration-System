using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec001;

public sealed class CharterJourneyTests
{
    [Fact]
    public void Fr_4_student_journey_reaches_an_atomic_registration_receipt()
    {
        var evidence = Normalize(RepositoryFiles.Read(
            "docs/release-evidence/SPEC-001-charter-traceability.md"));
        RepositoryFiles.ContainsAll(
            evidence,
            "FR-4",
            "**Status:** PASS",
            "AUTH-02",
            "STU-02",
            "STU-04",
            "STU-05",
            "STU-06",
            "one commit boundary",
            "zero overbooking",
            "zero partial atomic submissions");

        var routeSources = new Dictionary<string, string>
        {
            ["src/StudentRegistration.Client/Pages/StudentLoginPage.razor"] =
                "@page \"/student/login\"",
            ["src/StudentRegistration.Client/Pages/SubjectDiscoveryPage.razor"] =
                "@page \"/student/subjects\"",
            ["src/StudentRegistration.Client/Pages/ScheduleBuilderPage.razor"] =
                "@page \"/student/schedule\"",
            ["src/StudentRegistration.Client/Pages/RegistrationReviewPage.razor"] =
                "@page \"/student/review\"",
            ["src/StudentRegistration.Client/Pages/RegistrationResultPage.razor"] =
                "@page \"/student/registration/result/{Id:guid}\""
        };
        Assert.All(
            routeSources,
            route => Assert.Contains(
                route.Value,
                RepositoryFiles.Read(route.Key),
                StringComparison.Ordinal));

        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read(
                "src/StudentRegistration.Client/Pages/RegistrationReviewPage.razor"),
            "Confirm registration",
            "Navigation.NavigateTo($\"/student/registration/result/{submissionId:D}\")");
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read(
                "src/StudentRegistration.Infrastructure.SqlServer/Registration/SqlSeatAllocator.cs"),
            "share one commit boundary",
            "CreateSavepointAsync",
            "RollbackToSavepointAsync",
            "Enrollment");

        using var load = JsonDocument.Parse(RepositoryFiles.Read(
            "docs/release-evidence/SPEC-018-load-results.json"));
        foreach (var profileName in new[] { "target", "spike", "collision" })
        {
            var invariants = load.RootElement.GetProperty(profileName)
                .GetProperty("invariants");
            Assert.Equal(0, invariants.GetProperty("overbookedGroups").GetInt32());
            Assert.Equal(
                0,
                invariants.GetProperty("partialScheduleCommits").GetInt32());
            Assert.Equal(0, invariants.GetProperty("totalViolations").GetInt32());
        }
    }

    [Fact]
    public void Fr_6_mvp_scope_and_non_goals_match_the_project_plan()
    {
        var evidence = Normalize(RepositoryFiles.Read(
            "docs/release-evidence/SPEC-001-charter-traceability.md"));

        RepositoryFiles.ContainsAll(
            evidence,
            "FR-6",
            "MVP scope parity",
            "OS-1 through OS-4",
            "docs/PROJECT_PLAN.md");
    }

    [Fact]
    public void Fr_7_delivery_admission_requires_approved_fr_and_ac_traceability()
    {
        var evidence = Normalize(RepositoryFiles.Read(
            "docs/release-evidence/SPEC-001-charter-traceability.md"));

        RepositoryFiles.ContainsAll(
            evidence,
            "FR-7",
            "SPEC-NNN/FR-N",
            "SPEC-NNN/AC-N",
            "Rejected when either link is missing or unapproved");
    }

    private static string Normalize(string value) => string.Join(
        " ",
        value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
