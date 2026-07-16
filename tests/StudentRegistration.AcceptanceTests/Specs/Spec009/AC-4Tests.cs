using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Domain;

namespace StudentRegistration.AcceptanceTests.Specs.Spec009;

public sealed class AC_4Tests
{
    [Fact]
    public void Authorized_publish_supersedes_immutable_versions_and_unauthorized_publish_changes_nothing()
    {
        var service = new PolicyAdministrationService();
        var draft = PolicyAdministrationService.CreateDemoPolicySet(Guid.NewGuid());
        Assert.True(service.Validate(draft).IsValid);

        Assert.Equal(PolicyPublishOutcome.Unauthorized, service.Publish(draft, false));
        Assert.Equal(PolicySetState.Validated, draft.PolicySet.State);
        Assert.Equal(PolicyPublishOutcome.Published, service.Publish(draft, true));

        var prior = new CatalogueVersion(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "AI-DS",
            "2026.1",
            "SRC-DATA-SCIENCE",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 16, 0, 0, 0, DateTimeKind.Utc),
            "admin-1",
            CatalogueVersionState.Published);
        var successor = new CatalogueVersion(
            Guid.NewGuid(),
            Guid.NewGuid(),
            prior.Id,
            "AI-DS",
            "2026.2",
            "SRC-DATA-SCIENCE",
            new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc),
            "admin-1",
            CatalogueVersionState.Published);
        prior.MarkSuperseded();

        Assert.Equal(CatalogueVersionState.Superseded, prior.State);
        Assert.Equal(prior.Id, successor.SupersedesId);
        Assert.Equal("2026.1", prior.VersionCode);
    }
}
