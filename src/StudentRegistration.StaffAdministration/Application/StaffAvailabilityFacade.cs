using StudentRegistration.Scheduling.Application.Ports;

namespace StudentRegistration.StaffAdministration.Application;

public sealed class StaffAvailabilityFacade(IStaffAvailabilityPort port)
{
    public Task<StaffAvailabilityPortResult> GetOwnAsync(
        Guid authenticatedStaffId,
        Guid termId,
        CancellationToken cancellationToken = default) =>
        port.GetOwnAsync(authenticatedStaffId, termId, cancellationToken);

    public Task<StaffAvailabilityPortResult> ReplaceOwnAsync(
        ReplaceOwnStaffAvailability command,
        CancellationToken cancellationToken = default) =>
        port.ReplaceOwnAsync(command, cancellationToken);
}
