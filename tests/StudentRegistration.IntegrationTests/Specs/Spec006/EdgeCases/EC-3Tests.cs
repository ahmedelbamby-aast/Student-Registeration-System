using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.Academics.Domain;
using StudentRegistration.Api.Composition;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Auditing;
using StudentRegistration.Infrastructure.SqlServer.Audit;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.IntegrationTests.Infrastructure;

namespace StudentRegistration.IntegrationTests.Specs.Spec006.EdgeCases;

public sealed class EC_3Tests
{
    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Cancellation_rolls_back_before_commit_and_lost_response_replays_once()
    {
        await using var sqlServer = new SqlServerContainerFixture();
        await sqlServer.StartAsync();

        var connectionString = new SqlConnectionStringBuilder(sqlServer.ConnectionString)
        {
            InitialCatalog = $"StudentRegistration_Test_Spec006_EC3_{Guid.NewGuid():N}"
        }.ConnectionString;
        await using var provider = CreateSqlProvider(connectionString);

        try
        {
            await using (var setupScope = provider.CreateAsyncScope())
            {
                var context = setupScope.ServiceProvider
                    .GetRequiredService<StudentRegistrationDbContext>();
                await context.Database.MigrateAsync();
            }

            var canceledCommand = CreateTermCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "SHA256:SPEC006-CANCELED",
                "2026-CANCELED");
            using (var cancellation = new CancellationTokenSource())
            {
                cancellation.Cancel();
                await using var canceledScope = provider.CreateAsyncScope();
                var store = canceledScope.ServiceProvider
                    .GetRequiredService<IRegistrationWindowStore>();

                await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                    store.CreateOrReplayTermAsync(
                        canceledCommand,
                        cancellation.Token));
            }

            await AssertPersistedCountsAsync(
                provider,
                terms: 0,
                windows: 0,
                audits: 0);

            var requestId = Guid.NewGuid();
            var committedTermId = Guid.NewGuid();
            var committed = CreateTermCommand(
                committedTermId,
                Guid.NewGuid(),
                requestId,
                "SHA256:SPEC006-COMMITTED",
                "2026-COMMITTED");

            await using (var lostResponseScope = provider.CreateAsyncScope())
            {
                var store = lostResponseScope.ServiceProvider
                    .GetRequiredService<IRegistrationWindowStore>();

                _ = await store.CreateOrReplayTermAsync(committed);
            }

            var retry = CreateTermCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                requestId,
                "SHA256:SPEC006-COMMITTED",
                "2026-COMMITTED");
            await using (var retryScope = provider.CreateAsyncScope())
            {
                var store = retryScope.ServiceProvider
                    .GetRequiredService<IRegistrationWindowStore>();
                var replayed = await store.CreateOrReplayTermAsync(retry);

                Assert.Equal(AcademicTermCreationOutcome.Replayed, replayed.Outcome);
                Assert.Equal(committedTermId.ToString("D"), replayed.Term?.Id);
            }

            await AssertPersistedCountsAsync(
                provider,
                terms: 1,
                windows: 1,
                audits: 1);
        }
        finally
        {
            var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
                .UseSqlServer(connectionString)
                .Options;
            await using var cleanup = new StudentRegistrationDbContext(options);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    private static async Task AssertPersistedCountsAsync(
        ServiceProvider provider,
        int terms,
        int windows,
        int audits)
    {
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider
            .GetRequiredService<StudentRegistrationDbContext>();
        Assert.Equal(terms, await context.Set<AcademicTerm>().CountAsync());
        Assert.Equal(windows, await context.Set<RegistrationWindow>().CountAsync());
        Assert.Equal(audits, await context.AuditEvents.CountAsync());
    }

    private static CreateAcademicTermStoreCommand CreateTermCommand(
        Guid termId,
        Guid windowId,
        Guid requestId,
        string payloadHash,
        string code)
    {
        var term = new AcademicTerm(
            termId,
            code,
            requestId,
            payloadHash,
            code.Replace('-', ' '),
            new DateOnly(2026, 9, 1),
            new DateOnly(2027, 1, 15),
            "Africa/Cairo",
            TermState.Draft);
        var window = new RegistrationWindow(
            windowId,
            termId,
            RegistrationWindowScopeType.AllStudents,
            null,
            new DateTime(2026, 8, 1, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 8, 15, 20, 0, 0, DateTimeKind.Utc),
            RegistrationWindowLifecycleState.Draft);

        return new(
            term,
            [window],
            new AuditEventDraft(
                "admin:spec006",
                termId.ToString("D"),
                "academic-term-created",
                nameof(AcademicTerm),
                termId.ToString("D"),
                "Verify idempotent SQL mutation behavior.",
                null,
                "{\"state\":\"draft\"}",
                Guid.NewGuid().ToString("N"),
                new DateTime(2026, 7, 16, 8, 0, 0, DateTimeKind.Utc)));
    }

    private static ServiceProvider CreateSqlProvider(string connectionString)
    {
        var services = new ServiceCollection();
        services.AddSingleton(TimeProvider.System);
        services.AddStudentRegistrationSqlServer(Configuration(connectionString));
        services.AddScoped<IAuditEventWriter, AuditTransactionWriter>();
        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });
    }

    private static IConfiguration Configuration(string connectionString) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"ConnectionStrings:{SqlServerPersistenceRegistration.ConnectionStringName}"] =
                    connectionString
            })
            .Build();
}
