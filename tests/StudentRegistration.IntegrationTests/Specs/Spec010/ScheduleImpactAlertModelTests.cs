using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec010;

public sealed class ScheduleImpactAlertModelTests
{
    private static readonly DateTime DetectedAtUtc =
        new(2026, 7, 16, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Alert_preserves_affected_resources_detected_versions_reason_and_rowversion()
    {
        var id = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var availabilityId = Guid.NewGuid();
        byte[] groupVersion = [8];
        byte[] resourceVersion = [4];

        var alert = new ScheduleImpactAlert(
            id,
            groupId,
            availabilityId,
            "STAFF_UNAVAILABLE",
            groupVersion,
            resourceVersion,
            DetectedAtUtc);

        Assert.Equal(id, alert.Id);
        Assert.Equal(groupId, alert.GroupId);
        Assert.Equal(availabilityId, alert.StaffTermAvailabilityId);
        Assert.Equal("STAFF_UNAVAILABLE", alert.ReasonCode);
        Assert.Equal(groupVersion, alert.DetectedGroupVersion);
        Assert.Equal(resourceVersion, alert.DetectedResourceVersion);
        Assert.Equal(ScheduleImpactAlertState.Open, alert.State);
        Assert.Equal(DetectedAtUtc, alert.DetectedAtUtc);
        Assert.Null(alert.RevalidatedAtUtc);
        Assert.Null(alert.ResolvedAtUtc);
        Assert.Empty(alert.Version);
        AssertPrivateSetter(nameof(ScheduleImpactAlert.Version));
    }

    [Fact]
    public void Alert_requires_passing_revalidation_before_explicit_resolution()
    {
        var alert = Create();
        var revalidatedAtUtc = DetectedAtUtc.AddHours(1);

        Assert.Throws<InvalidOperationException>(() => alert.Resolve(
            "Resource corrected",
            revalidatedAtUtc));

        alert.RecordRevalidation(
            """{"valid":false,"reasons":["STAFF_UNAVAILABLE"]}""",
            passed: false,
            revalidatedAtUtc);
        Assert.Equal(ScheduleImpactAlertState.Revalidated, alert.State);
        Assert.False(alert.LastRevalidationPassed);
        Assert.Throws<InvalidOperationException>(() => alert.Resolve(
            "Resource corrected",
            revalidatedAtUtc.AddMinutes(1)));

        alert.RecordRevalidation(
            """{"valid":true,"reasons":[]}""",
            passed: true,
            revalidatedAtUtc.AddMinutes(2));
        alert.Resolve("Resource corrected", revalidatedAtUtc.AddMinutes(3));

        Assert.Equal(ScheduleImpactAlertState.Resolved, alert.State);
        Assert.True(alert.LastRevalidationPassed);
        Assert.Equal(revalidatedAtUtc.AddMinutes(3), alert.ResolvedAtUtc);
    }

    [Fact]
    public void Alert_rejects_missing_identity_reason_versions_and_non_utc_detection()
    {
        Assert.Throws<ArgumentException>(() => Create(id: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(groupId: Guid.Empty));
        Assert.Throws<ArgumentException>(
            () => Create(staffTermAvailabilityId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(reasonCode: " "));
        Assert.Throws<ArgumentException>(() => Create(groupVersion: []));
        Assert.Throws<ArgumentException>(() => Create(resourceVersion: []));
        Assert.Throws<ArgumentException>(() => Create(
            detectedAtUtc: DateTime.SpecifyKind(
                DetectedAtUtc,
                DateTimeKind.Unspecified)));
    }

    private static ScheduleImpactAlert Create(
        Guid? id = null,
        Guid? groupId = null,
        Guid? staffTermAvailabilityId = null,
        string reasonCode = "STAFF_UNAVAILABLE",
        byte[]? groupVersion = null,
        byte[]? resourceVersion = null,
        DateTime? detectedAtUtc = null) =>
        new(
            id ?? Guid.NewGuid(),
            groupId ?? Guid.NewGuid(),
            staffTermAvailabilityId ?? Guid.NewGuid(),
            reasonCode,
            groupVersion ?? [8],
            resourceVersion ?? [4],
            detectedAtUtc ?? DetectedAtUtc);

    private static void AssertPrivateSetter(string propertyName)
    {
        var property = typeof(ScheduleImpactAlert).GetProperty(propertyName);
        Assert.NotNull(property);
        Assert.False(property.SetMethod?.IsPublic ?? false);
    }
}
