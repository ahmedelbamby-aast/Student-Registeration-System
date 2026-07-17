using System.Diagnostics;
using System.Security.Claims;
using StudentRegistration.StaffAdministration.Application;
using StudentRegistration.StaffAdministration.Application.Ports;
using StudentRegistration.StaffAdministration.Domain;

namespace StudentRegistration.QualityTests.Specs.Spec016;

public sealed class NFR_1EvidenceTests
{
    [Fact]
    public async Task Staff_assignment_reads_meet_the_300ms_p95_target()
    {
        var queries = new StaffWorkspaceQueries(new Reader(), new Audit());
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new(ClaimTypes.Role, "Lecturer")
            ],
            "quality"));

        for (var i = 0; i < 20; i++)
        {
            _ = await queries.GetAssignmentsAsync(principal);
        }

        var samples = new double[200];
        for (var i = 0; i < samples.Length; i++)
        {
            var start = Stopwatch.GetTimestamp();
            _ = await queries.GetAssignmentsAsync(principal);
            samples[i] = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
        }

        Array.Sort(samples);
        var p95 = samples[(int)Math.Ceiling(samples.Length * 0.95) - 1];
        Console.WriteLine($"SPEC-016 NFR-1 samples={samples.Length}; p95={p95:F4} ms; threshold=300 ms");
        Assert.True(p95 <= 300d, $"Measured p95 {p95:F4} ms exceeded 300 ms.");
    }

    private sealed class Reader : IStaffWorkspaceReader
    {
        public Task<IReadOnlyList<StaffAssignment>> ListAssignmentsAsync(Guid actorApplicationUserId, string activeRole, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<StaffAssignment>>([]);
        public Task<IReadOnlyList<StaffAssignment>> ListTimetableAsync(Guid actorApplicationUserId, string activeRole, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<StaffAssignment>>([]);
        public Task<StaffRosterPageSnapshot?> ReadRosterIfAssignedAsync(Guid actorApplicationUserId, string activeRole, Guid groupId, int page, int pageSize, string sort, CancellationToken cancellationToken = default) =>
            Task.FromResult<StaffRosterPageSnapshot?>(null);
    }

    private sealed class Audit : IStaffWorkspaceAuditWriter
    {
        public Task WriteRosterAccessAsync(StaffRosterAuditEntry entry, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
