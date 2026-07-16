using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec010;

public sealed class SubjectDetailsPageContributorTests
{
    [Fact]
    public void Stu_03_contribution_freezes_complete_activity_and_selection_data()
    {
        var contract = RepositoryFiles.Read(
            "specs/010-offerings-groups-resources/contracts/routes/STU-03.md");

        RepositoryFiles.ContainsAll(
            contract,
            "spec010-stu03/1.0",
            "GET /api/offerings/{offeringId}",
            "GET /api/groups/{groupId}",
            "Catalogue.ReadAvailable",
            "Lecture",
            "Tutorial",
            "Laboratory",
            "Section",
            "room code",
            "location",
            "day",
            "local start/end time",
            "staff assignment",
            "GROUP_FULL",
            "GROUP_UNPUBLISHED",
            "GROUP_CLOSED",
            "GROUP_CANCELLED",
            "REGISTRATION_PAUSED",
            "GROUP_CHANGED",
            "rowversion",
            "SPEC-011");
    }

    [Fact]
    public void Stu_03_contribution_does_not_claim_page_or_eligibility_ownership()
    {
        var contract = RepositoryFiles.Read(
            "specs/010-offerings-groups-resources/contracts/routes/STU-03.md");

        Assert.Contains("Canonical page owner:** SPEC-011", contract, StringComparison.Ordinal);
        Assert.Contains("does not own or edit", contract, StringComparison.Ordinal);
        Assert.Contains(
            "does not make SPEC-011 eligibility decisions",
            contract,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "Canonical page owner:** SPEC-010",
            contract,
            StringComparison.Ordinal);
    }
}
