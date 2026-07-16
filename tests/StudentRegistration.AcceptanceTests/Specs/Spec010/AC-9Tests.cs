using StudentRegistration.Scheduling.Application;

namespace StudentRegistration.AcceptanceTests.Specs.Spec010;

public sealed class AC_9Tests
{
    [Fact]
    public async Task Admin_reads_without_override_and_staff_change_requests_alert()
    {
        var store = new ResourceAvailabilityStoreFake
        {
            AffectsPublishedGroup = true
        };
        var service = new ResourceAvailabilityService(store);
        var adminView = await service.GetAdminPlanningInputAsync(
            store.OwnerStaffId,
            store.TermId);

        var forbidden = await service.ReplaceOwnAvailabilityAsync(
            Spec010Scenario.Id(99),
            store.AvailabilityId,
            [1],
            [new(1, new(8, 0), new(12, 0), "unavailable")],
            new DateTime(2026, 7, 16, 12, 0, 0, DateTimeKind.Utc),
            "admin override attempt");
        var staffUpdate = await service.ReplaceOwnAvailabilityAsync(
            store.OwnerStaffId,
            store.AvailabilityId,
            [1],
            [new(1, new(12, 0), new(15, 0), "unavailable")],
            new DateTime(2026, 7, 16, 12, 0, 0, DateTimeKind.Utc),
            "staff update");

        Assert.NotNull(adminView);
        Assert.Equal([1], adminView.RowVersion);
        Assert.Equal(ResourceAvailabilityOutcome.Forbidden, forbidden.Outcome);
        Assert.Equal(ResourceAvailabilityOutcome.Applied, staffUpdate.Outcome);
        var write = Assert.Single(store.AvailabilityWrites);
        Assert.True(write.AppendAudit);
        Assert.True(write.CreateImpactAlert);

        var serviceMethods = typeof(ResourceAvailabilityService)
            .GetMethods()
            .Where(method =>
                method.DeclaringType == typeof(ResourceAvailabilityService))
            .Select(method => method.Name)
            .ToArray();
        Assert.DoesNotContain(serviceMethods, method =>
            method.Contains("AdminOverride", StringComparison.Ordinal)
            || method.Contains("AdminReplace", StringComparison.Ordinal)
            || method.Contains(
                "AdminUpdateAvailability",
                StringComparison.Ordinal));
    }
}
