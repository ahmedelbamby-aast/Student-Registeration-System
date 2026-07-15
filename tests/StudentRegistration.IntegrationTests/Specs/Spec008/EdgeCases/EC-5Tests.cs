using System.Reflection;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec008.EdgeCases;

public sealed class EC_5Tests
{
    private static readonly DateTime OpensAtUtc =
        new(2026, 8, 1, 8, 0, 0, DateTimeKind.Utc);

    private static readonly DateTime ClosesAtUtc =
        new(2026, 8, 10, 18, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Request_received_at_the_half_open_cutoff_never_reaches_consumer_commit()
    {
        var boundary = new InFlightRegistrationDouble(OpensAtUtc, ClosesAtUtc);
        var commits = 0;

        var result = await boundary.TryCommitAsync(
            serverReceivedAtUtc: ClosesAtUtc,
            observedVersion: boundary.Version,
            commit: () =>
            {
                Interlocked.Increment(ref commits);
                return Task.CompletedTask;
            });

        Assert.Equal(RegistrationBoundaryOutcome.WindowClosed, result);
        Assert.Equal(0, commits);
    }

    [Fact]
    public async Task Emergency_closure_after_receipt_blocks_every_uncommitted_request()
    {
        var boundary = new InFlightRegistrationDouble(OpensAtUtc, ClosesAtUtc);
        var receivedAtUtc = ClosesAtUtc.AddTicks(-1);
        var observedVersion = boundary.Version;
        var commits = 0;

        boundary.EmergencyClose();
        var result = await boundary.TryCommitAsync(
            receivedAtUtc,
            observedVersion,
            () =>
            {
                Interlocked.Increment(ref commits);
                return Task.CompletedTask;
            });

        Assert.Equal(RegistrationBoundaryOutcome.WindowChanged, result);
        Assert.Equal(0, commits);
        Assert.Equal(1, boundary.Version);
    }

    [Fact]
    public void Frozen_in_flight_contract_uses_server_receipt_time_and_revalidation()
    {
        var requirements = RepositoryFiles.Read(
            "specs/008-academic-term-student-profile/requirements.md");
        var contract = RepositoryFiles.Read(
            "specs/008-academic-term-student-profile/contracts/api.md");

        RepositoryFiles.ContainsAll(
            requirements,
            "EC-5: A scheduled window closes while a request is in flight",
            "server-received timestamp governs the scheduled cutoff",
            "emergency",
            "closure/version change blocks every uncommitted request",
            "Registration commands MUST re-resolve time, term, window, student",
            "state, and holds");
        RepositoryFiles.ContainsAll(
            contract,
            "OpensAtUtc <= serverNowUtc < ClosesAtUtc",
            "a request received at",
            "`ClosesAtUtc` is closed",
            "Published -> EmergencyClosed",
            "Cancellation before commit rolls back audit and every mutation");
    }

    [Fact]
    public void Production_cutoff_requires_authoritative_context_and_registration_revalidation_services()
    {
        var academics = Assembly.Load("StudentRegistration.Academics");

        Assert.True(
            academics.GetType(
                "StudentRegistration.Academics.Application.AcademicContextResolver")
                is not null,
            "AcademicContextResolver must capture authoritative server time before EC-5 can execute production cutoff behavior.");
        Assert.True(
            academics.GetType(
                "StudentRegistration.Academics.Application.StudentAcademicProfileService")
                is not null,
            "StudentAcademicProfileService must revalidate the window/version before invoking a downstream consumer commit.");
    }

    private enum RegistrationBoundaryOutcome
    {
        Committed,
        WindowClosed,
        WindowChanged
    }

    private sealed class InFlightRegistrationDouble(
        DateTime opensAtUtc,
        DateTime closesAtUtc)
    {
        private bool _emergencyClosed;

        public int Version { get; private set; }

        public void EmergencyClose()
        {
            _emergencyClosed = true;
            Version++;
        }

        public async Task<RegistrationBoundaryOutcome> TryCommitAsync(
            DateTime serverReceivedAtUtc,
            int observedVersion,
            Func<Task> commit)
        {
            if (serverReceivedAtUtc.Kind is not DateTimeKind.Utc)
            {
                throw new ArgumentException(
                    "The server-received timestamp must be UTC.",
                    nameof(serverReceivedAtUtc));
            }

            if (serverReceivedAtUtc < opensAtUtc || serverReceivedAtUtc >= closesAtUtc)
            {
                return RegistrationBoundaryOutcome.WindowClosed;
            }

            if (_emergencyClosed || observedVersion != Version)
            {
                return RegistrationBoundaryOutcome.WindowChanged;
            }

            await commit();
            return RegistrationBoundaryOutcome.Committed;
        }
    }
}
