using StudentRegistration.Scheduling.Application.Ports;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.ApplicationTests.Staff;

public sealed class SchedulingAvailabilityPortTests
{
    [Fact]
    public void Port_contract_requires_parent_version_complete_ranges_and_atomic_result()
    {
        var replace = typeof(IStaffAvailabilityPort).GetMethod(nameof(IStaffAvailabilityPort.ReplaceOwnAsync));
        Assert.NotNull(replace);
        Assert.Equal(typeof(ReplaceOwnStaffAvailability), replace.GetParameters()[0].ParameterType);

        var fields = typeof(ReplaceOwnStaffAvailability).GetProperties().Select(property => property.Name).ToArray();
        Assert.Contains(nameof(ReplaceOwnStaffAvailability.ExpectedStaffTermVersion), fields);
        Assert.Contains(nameof(ReplaceOwnStaffAvailability.Ranges), fields);
        Assert.Contains(nameof(ReplaceOwnStaffAvailability.StaffId), fields);
        Assert.Contains(nameof(ReplaceOwnStaffAvailability.TermId), fields);

        var resultFields = typeof(StaffAvailabilityPortResult).GetProperties().Select(property => property.Name).ToArray();
        Assert.Contains(nameof(StaffAvailabilityPortResult.ServerTimeUtc), resultFields);
        Assert.Contains(nameof(StaffAvailabilityPortResult.DeadlineUtc), resultFields);
        Assert.Contains(nameof(StaffAvailabilityPortResult.ImpactAlertIds), resultFields);
        Assert.Contains(nameof(StaffAvailabilityPortResult.Availability), resultFields);
        Assert.Equal(
            [AvailabilityKind.Available, AvailabilityKind.Unavailable],
            Enum.GetValues<AvailabilityKind>());
    }
}
