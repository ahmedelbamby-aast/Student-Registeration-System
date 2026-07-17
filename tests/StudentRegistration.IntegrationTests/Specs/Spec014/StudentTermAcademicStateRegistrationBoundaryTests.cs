using Microsoft.EntityFrameworkCore;
using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Registration.Application;

namespace StudentRegistration.IntegrationTests.Specs.Spec014;

public sealed class StudentTermAcademicStateRegistrationBoundaryTests
{
    [Fact]
    public async Task Coordinator_consumes_the_spec008_boundary_and_runs_work_inside_its_callback()
    {
        var studentId = Guid.NewGuid();
        var termId = Guid.NewGuid();
        var expectedVersion = Convert.ToBase64String([1, 2, 3, 4]);
        var store = new BoundaryStore(AcademicProfileStoreOutcome.Succeeded);
        var service = new StudentAcademicProfileService(store, TimeProvider.System);
        var coordinator = new RegistrationTransactionCoordinator(service);
        var callbackCalls = 0;

        var result = await coordinator.ExecuteAsync(
            studentId,
            termId,
            expectedVersion,
            _ =>
            {
                callbackCalls++;
                return Task.CompletedTask;
            },
            CancellationToken.None);

        Assert.Equal(RegistrationBoundaryOutcome.Committed, result.Outcome);
        Assert.Equal(1, callbackCalls);
        Assert.NotNull(store.Command);
        Assert.Equal(studentId, store.Command.StudentId);
        Assert.Equal(termId, store.Command.TermId);
        Assert.Equal([1, 2, 3, 4], store.Command.ExpectedStudentTermStateRowVersion);
    }

    [Fact]
    public async Task Coordinator_does_not_run_registration_work_when_spec008_blocks_the_boundary()
    {
        var store = new BoundaryStore(AcademicProfileStoreOutcome.HoldBlocked);
        var coordinator = new RegistrationTransactionCoordinator(
            new StudentAcademicProfileService(store, TimeProvider.System));
        var callbackCalls = 0;

        var result = await coordinator.ExecuteAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Convert.ToBase64String([7]),
            _ =>
            {
                callbackCalls++;
                return Task.CompletedTask;
            },
            CancellationToken.None);

        Assert.Equal(RegistrationBoundaryOutcome.HoldBlocked, result.Outcome);
        Assert.Equal(0, callbackCalls);
    }

    [Fact]
    public void Registration_reuses_the_canonical_academic_row_and_defines_no_guard_entity_or_table()
    {
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(
                "Server=localhost;Database=Spec014ModelOnly;User Id=sa;" +
                "Password=ModelOnly_Passw0rd;TrustServerCertificate=True")
            .Options;
        using var context = new StudentRegistrationDbContext(options);

        var entityTypes = context.Model.GetEntityTypes().ToArray();
        Assert.Contains(
            entityTypes,
            entity => entity.ClrType ==
                typeof(StudentRegistration.Academics.Domain.StudentTermAcademicState));
        Assert.DoesNotContain(
            entityTypes,
            entity => entity.ClrType.Name.Contains(
                "StudentTermRegistrationGuard",
                StringComparison.Ordinal));
        Assert.DoesNotContain(
            typeof(RegistrationTransactionCoordinator).Assembly.GetTypes(),
            type => type.Name.Contains(
                "StudentTermRegistrationGuard",
                StringComparison.Ordinal));
    }

    private sealed class BoundaryStore(AcademicProfileStoreOutcome outcome)
        : IStudentAcademicProfileStore
    {
        public RegistrationBoundaryStoreCommand? Command { get; private set; }

        public Task<AcademicProfileStoreResult> ReadByApplicationUserIdAsync(
            Guid applicationUserId,
            AcademicProfileReadRequest request,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<AcademicProfileStoreResult> ReadByStudentIdAsync(
            Guid studentId,
            Guid termId,
            AcademicProfileReadRequest request,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<AcademicProfileStoreResult> CorrectAsync(
            CorrectAcademicProfileStoreCommand command,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public async Task<AcademicProfileStoreResult> ExecuteRegistrationBoundaryAsync(
            RegistrationBoundaryStoreCommand command,
            Func<CancellationToken, Task> commitCallback,
            CancellationToken cancellationToken = default)
        {
            Command = command;
            if (outcome is AcademicProfileStoreOutcome.Succeeded)
            {
                await commitCallback(cancellationToken);
            }

            return new(outcome);
        }
    }
}
