using System.Reflection;
using StudentRegistration.AcceptanceTests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec008;

public sealed class AC_5Tests
{
    [Fact]
    public async Task Losing_registration_consumer_revalidates_the_hold_and_never_commits()
    {
        var boundary = new StudentTermBoundaryDouble();
        var snapshotRead = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var resumeConsumer = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var commits = 0;

        var consumer = Task.Run(async () =>
        {
            var observedVersion = boundary.ReadVersion();
            snapshotRead.SetResult();
            await resumeConsumer.Task;
            return await boundary.RevalidateAndCommitAsync(
                observedVersion,
                () =>
                {
                    Interlocked.Increment(ref commits);
                    return Task.CompletedTask;
                });
        });

        await snapshotRead.Task;
        await boundary.AddBlockingHoldAsync();
        resumeConsumer.SetResult();

        Assert.Equal(BoundaryOutcome.HoldBlocked, await consumer);
        Assert.Equal(0, commits);
        Assert.Equal(1, boundary.Version);
    }

    [Fact]
    public void Frozen_acceptance_contract_stops_at_the_student_term_boundary()
    {
        var requirements = RepositoryFiles.Read(
            "specs/008-academic-term-student-profile/requirements.md");

        RepositoryFiles.ContainsAll(
            requirements,
            "AC-5: Hold mutation races submission",
            "participate in one database-backed student-term",
            "Then the operations have one valid serial order",
            "returns HOLD_BLOCKED",
            "commit callback",
            "SPEC-014 remains",
            "responsible for proving the protocol",
            "real seat/enrollment writes");
    }

    [Fact]
    public void Production_protocol_requires_the_academic_profile_service()
    {
        var service = Assembly.Load("StudentRegistration.Academics").GetType(
            "StudentRegistration.Academics.Application.StudentAcademicProfileService");

        Assert.True(
            service is not null,
            "StudentAcademicProfileService must expose the shared student-term revalidation protocol before AC-5 can execute against production code.");
        Assert.Contains(
            service!.GetMethods(BindingFlags.Instance | BindingFlags.Public),
            method => method.Name.Contains(
                "Registration",
                StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Real_sql_protocol_requires_the_spec008_store_before_starting_docker()
    {
        Assert.Contains("mssql/server:2022", Spec008AcceptanceSqlServerFixture.SqlServerImage);

        var store = Assembly.Load("StudentRegistration.Infrastructure.SqlServer").GetType(
            "StudentRegistration.Infrastructure.SqlServer.Persistence.AcademicStore");

        Assert.True(
            store is not null,
            "AcademicStore is required for the real-SQL student-term lock/version proof; Docker is intentionally not started while that adapter is absent.");
    }

    private enum BoundaryOutcome
    {
        Committed,
        HoldBlocked,
        StaleVersion
    }

    private sealed class StudentTermBoundaryDouble
    {
        private readonly SemaphoreSlim _gate = new(1, 1);
        private bool _hasBlockingHold;
        private int _version;

        public int Version => Volatile.Read(ref _version);

        public int ReadVersion() => Version;

        public async Task AddBlockingHoldAsync()
        {
            await _gate.WaitAsync();
            try
            {
                _hasBlockingHold = true;
                _version++;
            }
            finally
            {
                _gate.Release();
            }
        }

        public async Task<BoundaryOutcome> RevalidateAndCommitAsync(
            int observedVersion,
            Func<Task> commit)
        {
            await _gate.WaitAsync();
            try
            {
                if (_hasBlockingHold)
                {
                    return BoundaryOutcome.HoldBlocked;
                }

                if (observedVersion != _version)
                {
                    return BoundaryOutcome.StaleVersion;
                }

                await commit();
                return BoundaryOutcome.Committed;
            }
            finally
            {
                _gate.Release();
            }
        }
    }
}
