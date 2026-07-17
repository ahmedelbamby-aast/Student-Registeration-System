using StudentRegistration.Scheduling.Application.Ports;

namespace StudentRegistration.StaffAdministration.Application;

public static class ScheduleImpactQuery
{
    public static IReadOnlyList<Guid> From(StaffAvailabilityPortResult result) =>
        result.ImpactAlertIds;
}
