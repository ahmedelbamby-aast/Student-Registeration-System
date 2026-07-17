using StudentRegistration.Registration.Domain;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec015;

public sealed class DecisionSnapshotModelTests
{
    [Fact]
    public void Spec015_consumes_the_immutable_spec014_value_without_an_ef_entity()
    {
        Assert.True(RepositoryFiles.Exists(
            "src/StudentRegistration.Registration/Domain/DecisionSnapshot.cs"));
        Assert.Equal(
            "StudentRegistration.Registration.Domain",
            typeof(DecisionSnapshot).Namespace);
        Assert.Single(
            typeof(DecisionSnapshot).Assembly.GetTypes(),
            type => type.Name == nameof(DecisionSnapshot));

        var groupId = Guid.NewGuid();
        var source = new Dictionary<Guid, string> { [groupId] = "group-v1" };
        var snapshot = new DecisionSnapshot(
            "DEMO-POC-2026.1",
            "academic-v1",
            "plan-v1",
            source,
            "ACCEPTED");
        source[groupId] = "changed";

        Assert.Equal("group-v1", snapshot.GroupVersions[groupId]);
        using var context = ModelContext.Create("Spec015DecisionSnapshotModelOnly");
        Assert.Null(context.Model.FindEntityType(typeof(DecisionSnapshot)));
    }
}
