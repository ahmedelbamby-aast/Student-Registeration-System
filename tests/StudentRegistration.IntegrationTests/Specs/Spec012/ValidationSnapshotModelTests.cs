using StudentRegistration.Registration.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec012;

public sealed class ValidationSnapshotModelTests
{
    [Fact]
    public void Snapshot_preserves_timestamped_dependency_versions_immutably()
    {
        var offeringId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var offeringVersions = new Dictionary<Guid, string>
        {
            [offeringId] = "offering/7"
        };
        var groupVersions = new Dictionary<Guid, string>
        {
            [groupId] = "group/12"
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
        offeringVersions[offeringId] = "changed";
        groupVersions.Clear();

        Assert.Equal(evaluatedAtUtc, snapshot.EvaluatedAtUtc);
        Assert.Equal("academic/4", snapshot.AcademicContextVersion);
        Assert.Equal("DEMO-POC-2026.1", snapshot.PolicyVersion);
        Assert.Equal("catalogue/3", snapshot.CatalogueVersion);
        Assert.Equal("offering/7", snapshot.OfferingVersions[offeringId]);
        Assert.Equal("group/12", snapshot.GroupVersions[groupId]);
    }

    [Fact]
    public void Snapshot_rejects_non_utc_missing_or_invalid_dependency_versions()
    {
        Assert.Throws<ArgumentException>(() => Create(
            evaluatedAtUtc: new DateTime(2026, 7, 16, 9, 0, 0)));
        Assert.Throws<ArgumentException>(() => Create(academicVersion: " "));
        Assert.Throws<ArgumentException>(() => Create(policyVersion: " "));
        Assert.Throws<ArgumentException>(() => Create(catalogueVersion: " "));
        Assert.Throws<ArgumentException>(() => Create(
            offeringVersions: new Dictionary<Guid, string>
            {
                [Guid.Empty] = "offering/1"
            }));
        Assert.Throws<ArgumentException>(() => Create(
            groupVersions: new Dictionary<Guid, string>
            {
                [Guid.NewGuid()] = " "
            }));
    }

    private static ValidationSnapshot Create(
        DateTime? evaluatedAtUtc = null,
        string academicVersion = "academic/1",
        string policyVersion = "policy/1",
        string catalogueVersion = "catalogue/1",
        IReadOnlyDictionary<Guid, string>? offeringVersions = null,
        IReadOnlyDictionary<Guid, string>? groupVersions = null) =>
        new(
            evaluatedAtUtc ?? DateTime.UtcNow,
            academicVersion,
            policyVersion,
            catalogueVersion,
            offeringVersions ?? new Dictionary<Guid, string>(),
            groupVersions ?? new Dictionary<Guid, string>());
}
