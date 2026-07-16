using StudentRegistration.Client.Features.Scheduling;
using StudentRegistration.Registration.Domain;
using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec012;

public sealed class RegistrationReviewPageContributorTests
{
    [Fact]
    public void Stu_05_contribution_freezes_blocked_review_conflict_actions_and_snapshot()
    {
        var contract = RepositoryFiles.Read(
            "specs/012-schedule-builder-conflicts/contracts/routes/STU-05.md");

        RepositoryFiles.ContainsAll(
            contract,
            "spec012-stu05/1.0",
            "GET /api/student/terms/{termId}/registration-plan",
            "POST /api/student/terms/{termId}/registration-plan/validate",
            "MEETING_OVERLAP",
            "exact overlap start and end",
            "change-group",
            "remove-group",
            "red X",
            "Conflict",
            "disabled",
            "STU-04 /student/schedule",
            "evaluated UTC time",
            "academic context",
            "catalogue",
            "selected offering",
            "selected group",
            "STALE_VERSION",
            "GROUP_CHANGED",
            "GROUP_FULL",
            "GROUP_UNAVAILABLE",
            "same canonical meeting collection",
            "aria-describedby");
    }

    [Fact]
    public void Stu_05_contribution_does_not_claim_page_submission_or_override_ownership()
    {
        var contract = RepositoryFiles.Read(
            "specs/012-schedule-builder-conflicts/contracts/routes/STU-05.md");

        Assert.Contains("Canonical page owner:** SPEC-014", contract, StringComparison.Ordinal);
        Assert.Contains("does not own or edit that page", contract, StringComparison.Ordinal);
        Assert.Contains("SPEC-014 alone owns submission", contract, StringComparison.Ordinal);
        Assert.Contains("never reserves a seat", contract, StringComparison.Ordinal);
        Assert.Contains("there is no override", contract, StringComparison.Ordinal);
        Assert.DoesNotContain("Canonical page owner:** SPEC-012", contract, StringComparison.Ordinal);
    }

    [Fact]
    public void Stu_05_contribution_maps_blocking_actions_and_preserves_snapshot_semantics()
    {
        var firstGroupId = "group-ds221-g01";
        var secondGroupId = "group-ai301-g02";
        var conflict = new ServerScheduleConflictPresentation(
            "MEETING_OVERLAP",
            new(
                firstGroupId,
                "G01",
                "DS221",
                "Data Science",
                new TimeOnly(10, 0),
                new TimeOnly(11, 30)),
            new(
                secondGroupId,
                "G02",
                "AI301",
                "Machine Learning",
                new TimeOnly(11, 0),
                new TimeOnly(12, 0)),
            DayOfWeek.Monday,
            new TimeOnly(11, 0),
            new TimeOnly(11, 30),
            "DS221 G01 overlaps AI301 G02 on Monday from 11:00 to 11:30.",
            [
                new("change-group", firstGroupId, "Change DS221 group", "/student/subjects/ds221"),
                new("remove-group", firstGroupId, "Remove DS221 group", "/student/schedule"),
                new("change-group", secondGroupId, "Change AI301 group", "/student/subjects/ai301"),
                new("remove-group", secondGroupId, "Remove AI301 group", "/student/schedule")
            ]);

        var state = ConflictStateMapper.Map([conflict], []);

        Assert.True(state.ReviewBlocked);
        Assert.Equal("Conflict", state.StatusText);
        Assert.Equal([conflict.Message], state.BlockingReasons);
        var mapped = Assert.Single(state.Conflicts);
        Assert.Same(conflict, mapped.Source);
        Assert.Equal(4, mapped.Panel.ResolutionLinks.Count);
        foreach (var groupId in new[] { firstGroupId, secondGroupId })
        {
            Assert.Contains(
                conflict.Actions,
                action => action.TargetGroupId == groupId && action.Action == "change-group");
            Assert.Contains(
                conflict.Actions,
                action => action.TargetGroupId == groupId && action.Action == "remove-group");
        }

        var offeringId = Guid.Parse("12000000-0000-0000-0000-000000000006");
        var selectedGroupId =
            Guid.Parse("12000000-0000-0000-0000-000000000007");
        var offeringVersions = new Dictionary<Guid, string>
        {
            [offeringId] = "offering/7"
        };
        var groupVersions = new Dictionary<Guid, string>
        {
            [selectedGroupId] = "group/12"
        };
        var evaluatedAtUtc =
            new DateTime(2026, 7, 16, 9, 0, 0, DateTimeKind.Utc);
        var snapshot = new ValidationSnapshot(
            evaluatedAtUtc,
            "academic/4",
            "DEMO-POC-2026.1",
            "catalogue/3",
            offeringVersions,
            groupVersions);
        offeringVersions.Clear();
        groupVersions.Clear();

        Assert.Equal(evaluatedAtUtc, snapshot.EvaluatedAtUtc);
        Assert.Equal("academic/4", snapshot.AcademicContextVersion);
        Assert.Equal("DEMO-POC-2026.1", snapshot.PolicyVersion);
        Assert.Equal("catalogue/3", snapshot.CatalogueVersion);
        Assert.Equal("offering/7", snapshot.OfferingVersions[offeringId]);
        Assert.Equal("group/12", snapshot.GroupVersions[selectedGroupId]);
    }
}
