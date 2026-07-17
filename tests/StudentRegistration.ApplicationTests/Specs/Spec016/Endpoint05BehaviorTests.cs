using StudentRegistration.Scheduling.Application.Ports;
using StudentRegistration.Scheduling.Domain;
using StudentRegistration.StaffAdministration.Application;

namespace StudentRegistration.ApplicationTests.Specs.Spec016;

public sealed class Endpoint05BehaviorTests
{
    [Fact]
    public async Task Facade_passes_complete_replacement_and_expected_parent_version_once()
    {
        var port = new CapturingAvailabilityPort();
        var facade = new StaffAvailabilityFacade(port);
        var command = new ReplaceOwnStaffAvailability(
            Guid.NewGuid(),
            Guid.NewGuid(),
            [1, 2, 3],
            [new(Guid.NewGuid(), DayOfWeek.Monday, new(9, 0), new(12, 0), AvailabilityKind.Available)],
            "staff declaration",
            "correlation-1");

        await facade.ReplaceOwnAsync(command);

        Assert.Same(command, port.Command);
        Assert.Single(port.Command!.Ranges);
        Assert.Equal([1, 2, 3], port.Command.ExpectedStaffTermVersion);
    }

    private sealed class CapturingAvailabilityPort : IStaffAvailabilityPort
    {
        public ReplaceOwnStaffAvailability? Command { get; private set; }

        public Task<StaffAvailabilityPortResult> GetOwnAsync(Guid staffId, Guid termId, CancellationToken cancellationToken = default) =>
            Task.FromResult(StaffAvailabilityPortResult.NotFound(DateTime.UtcNow));

        public Task<StaffAvailabilityPortResult> ReplaceOwnAsync(ReplaceOwnStaffAvailability command, CancellationToken cancellationToken = default)
        {
            Command = command;
            return Task.FromResult(StaffAvailabilityPortResult.NotFound(DateTime.UtcNow));
        }
    }
}
