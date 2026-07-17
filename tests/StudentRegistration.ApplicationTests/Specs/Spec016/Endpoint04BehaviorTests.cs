using StudentRegistration.Scheduling.Application.Ports;
using StudentRegistration.StaffAdministration.Application;

namespace StudentRegistration.ApplicationTests.Specs.Spec016;

public sealed class Endpoint04BehaviorTests
{
    [Fact]
    public async Task Facade_reads_only_the_authenticated_staff_aggregate_through_Scheduling()
    {
        var port = new CapturingAvailabilityPort();
        var facade = new StaffAvailabilityFacade(port);
        var staffId = Guid.NewGuid();
        var termId = Guid.NewGuid();

        await facade.GetOwnAsync(staffId, termId);

        Assert.Equal(staffId, port.ReadStaffId);
        Assert.Equal(termId, port.ReadTermId);
    }

    private sealed class CapturingAvailabilityPort : IStaffAvailabilityPort
    {
        public Guid ReadStaffId { get; private set; }
        public Guid ReadTermId { get; private set; }

        public Task<StaffAvailabilityPortResult> GetOwnAsync(Guid staffId, Guid termId, CancellationToken cancellationToken = default)
        {
            ReadStaffId = staffId;
            ReadTermId = termId;
            return Task.FromResult(StaffAvailabilityPortResult.NotFound(DateTime.UtcNow));
        }

        public Task<StaffAvailabilityPortResult> ReplaceOwnAsync(ReplaceOwnStaffAvailability command, CancellationToken cancellationToken = default) =>
            Task.FromResult(StaffAvailabilityPortResult.NotFound(DateTime.UtcNow));
    }
}
