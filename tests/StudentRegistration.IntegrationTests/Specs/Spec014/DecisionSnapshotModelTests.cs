using System.Text.Json;
using StudentRegistration.Registration.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec014;

public sealed class DecisionSnapshotModelTests
{
    [Fact]
    public void Snapshot_preserves_exact_policy_profile_plan_group_and_result_evidence()
    {
        var groupId = Guid.NewGuid();
        var groupVersions = new Dictionary<Guid, string>
        {
            [groupId] = "group-rv-before-9"
        };

        var snapshot = new DecisionSnapshot(
            "policy-2026.1",
            "academic-context-rv-7",
            "plan-rv-5",
            groupVersions,
            "ACCEPTED");
        groupVersions[groupId] = "changed-after-capture";

        Assert.Equal(typeof(RegistrationPlan).Assembly, typeof(DecisionSnapshot).Assembly);
        Assert.Equal(
            "StudentRegistration.Registration.Domain",
            typeof(DecisionSnapshot).Namespace);
        Assert.Equal("policy-2026.1", snapshot.PolicyVersion);
        Assert.Equal("academic-context-rv-7", snapshot.AcademicContextVersion);
        Assert.Equal("plan-rv-5", snapshot.PlanVersion);
        Assert.Equal("group-rv-before-9", snapshot.GroupVersions[groupId]);
        Assert.Equal("ACCEPTED", snapshot.ResultCode);

        var mutableView = Assert.IsAssignableFrom<IDictionary<Guid, string>>(
            snapshot.GroupVersions);
        Assert.Throws<NotSupportedException>(() =>
            mutableView.Add(Guid.NewGuid(), "group-rv-10"));
    }

    [Fact]
    public void Snapshot_rejects_missing_or_invalid_version_evidence()
    {
        Assert.Throws<ArgumentException>(() => Create(policyVersion: " "));
        Assert.Throws<ArgumentException>(() => Create(academicContextVersion: " "));
        Assert.Throws<ArgumentException>(() => Create(planVersion: " "));
        Assert.Throws<ArgumentException>(() => Create(resultCode: " "));
        Assert.Throws<ArgumentNullException>(() => new DecisionSnapshot(
            "policy-2026.1",
            "academic-context-rv-7",
            "plan-rv-5",
            null!,
            "ACCEPTED"));
        Assert.Throws<ArgumentException>(() => Create(
            groupVersions: new Dictionary<Guid, string>
            {
                [Guid.Empty] = "group-rv-9"
            }));
        Assert.Throws<ArgumentException>(() => Create(
            groupVersions: new Dictionary<Guid, string>
            {
                [Guid.NewGuid()] = " "
            }));
    }

    [Fact]
    public void Snapshot_json_contract_round_trips_without_losing_evidence()
    {
        var groupId = Guid.NewGuid();
        var snapshot = Create(groupVersions: new Dictionary<Guid, string>
        {
            [groupId] = "group-rv-before-9"
        });

        var json = JsonSerializer.Serialize(snapshot, JsonSerializerOptions.Web);
        using var document = JsonDocument.Parse(json);
        Assert.Equal(
            [
                "academicContextVersion", "groupVersions", "planVersion",
                "policyVersion", "resultCode"
            ],
            document.RootElement.EnumerateObject()
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal));

        var roundTrip = JsonSerializer.Deserialize<DecisionSnapshot>(
            json,
            JsonSerializerOptions.Web);
        Assert.NotNull(roundTrip);
        Assert.Equal(snapshot.PolicyVersion, roundTrip.PolicyVersion);
        Assert.Equal(snapshot.AcademicContextVersion, roundTrip.AcademicContextVersion);
        Assert.Equal(snapshot.PlanVersion, roundTrip.PlanVersion);
        Assert.Equal(snapshot.GroupVersions, roundTrip.GroupVersions);
        Assert.Equal(snapshot.ResultCode, roundTrip.ResultCode);
    }

    private static DecisionSnapshot Create(
        string policyVersion = "policy-2026.1",
        string academicContextVersion = "academic-context-rv-7",
        string planVersion = "plan-rv-5",
        IReadOnlyDictionary<Guid, string>? groupVersions = null,
        string resultCode = "ACCEPTED") =>
        new(
            policyVersion,
            academicContextVersion,
            planVersion,
            groupVersions ?? new Dictionary<Guid, string>
            {
                [Guid.NewGuid()] = "group-rv-before-9"
            },
            resultCode);
}
