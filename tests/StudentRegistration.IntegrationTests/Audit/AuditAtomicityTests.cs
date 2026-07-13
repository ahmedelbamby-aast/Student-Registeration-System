using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using StudentRegistration.Contracts.Auditing;
using StudentRegistration.Infrastructure.SqlServer.Audit;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.IntegrationTests.Infrastructure;

namespace StudentRegistration.IntegrationTests.Audit;

public sealed class AuditAtomicityTests : IAsyncLifetime
{
    private readonly SqlServerContainerFixture _sqlServer = new();

    public Task InitializeAsync() => _sqlServer.StartAsync();

    public async Task DisposeAsync() => await _sqlServer.DisposeAsync();

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Business_state_and_audit_commit_in_the_callers_transaction()
    {
        await using var database = await AuditProbeDatabase.CreateAsync(
            _sqlServer.ConnectionString);
        await using var transaction = await database.Context.Database
            .BeginTransactionAsync();
        var callerTransaction = database.Context.Database.CurrentTransaction;

        await database.InsertBusinessStateAsync();
        await CreateWriter(database.Context).AppendAsync(Draft(), default);

        Assert.Same(callerTransaction, database.Context.Database.CurrentTransaction);
        await database.Context.SaveChangesAsync();
        await transaction.CommitAsync();

        Assert.Equal(1, await database.BusinessStateCountAsync());
        Assert.Equal(1, await database.Context.AuditEvents.AsNoTracking().CountAsync());
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Fault_injected_audit_save_rolls_back_business_state()
    {
        await using var database = await AuditProbeDatabase.CreateAsync(
            _sqlServer.ConnectionString,
            new AuditFailureInterceptor());
        await using var transaction = await database.Context.Database
            .BeginTransactionAsync();

        await database.InsertBusinessStateAsync();
        await CreateWriter(database.Context).AppendAsync(Draft(), default);

        var failure = await Assert.ThrowsAsync<InvalidOperationException>(
            () => database.Context.SaveChangesAsync());
        Assert.Contains("SPEC004_INJECTED_AUDIT_FAILURE", failure.Message);
        await transaction.RollbackAsync();
        database.Context.ChangeTracker.Clear();

        Assert.Equal(0, await database.BusinessStateCountAsync());
        Assert.Equal(0, await database.Context.AuditEvents.AsNoTracking().CountAsync());
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Caller_rollback_removes_both_saved_business_and_audit_rows()
    {
        await using var database = await AuditProbeDatabase.CreateAsync(
            _sqlServer.ConnectionString);
        await using var transaction = await database.Context.Database
            .BeginTransactionAsync();

        await database.InsertBusinessStateAsync();
        await CreateWriter(database.Context).AppendAsync(Draft(), default);
        await database.Context.SaveChangesAsync();
        await transaction.RollbackAsync();
        database.Context.ChangeTracker.Clear();

        Assert.Equal(0, await database.BusinessStateCountAsync());
        Assert.Equal(0, await database.Context.AuditEvents.AsNoTracking().CountAsync());
    }

    private static IAuditEventWriter CreateWriter(
        StudentRegistrationDbContext context)
    {
        var writerType = typeof(StudentRegistrationDbContext).Assembly.GetType(
            "StudentRegistration.Infrastructure.SqlServer.Audit.AuditTransactionWriter");
        Assert.NotNull(writerType);
        var writer = Activator.CreateInstance(writerType, context) as IAuditEventWriter;
        Assert.NotNull(writer);
        return writer;
    }

    private static AuditEventDraft Draft() => new(
        ActorReference: "student:synthetic-1001",
        SubjectReference: "registration:synthetic-plan",
        Action: "Spec004AtomicityProbe",
        EntityType: "SyntheticBusinessState",
        EntityId: Guid.NewGuid().ToString("N"),
        Reason: "Prove the shared local transaction boundary.",
        BeforeSummaryJson: null,
        AfterSummaryJson: "{\"state\":\"created\"}",
        CorrelationId: Guid.NewGuid().ToString("N"),
        OccurredAtUtc: DateTime.UtcNow);

    private sealed class AuditFailureInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context?.ChangeTracker.Entries<AuditEvent>()
                .Any(entry => entry.State == EntityState.Added) == true)
            {
                throw new InvalidOperationException(
                    "SPEC004_INJECTED_AUDIT_FAILURE");
            }

            return ValueTask.FromResult(result);
        }
    }

    private sealed class AuditProbeDatabase : IAsyncDisposable
    {
        private AuditProbeDatabase(StudentRegistrationDbContext context)
        {
            Context = context;
        }

        public StudentRegistrationDbContext Context { get; }

        public static async Task<AuditProbeDatabase> CreateAsync(
            string masterConnectionString,
            SaveChangesInterceptor? interceptor = null)
        {
            var databaseName = $"SrsSpec004Audit{Guid.NewGuid():N}";
            var connection = new SqlConnectionStringBuilder(masterConnectionString)
            {
                InitialCatalog = databaseName
            };
            var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
                .UseSqlServer(connection.ConnectionString);
            if (interceptor is not null)
            {
                options.AddInterceptors(interceptor);
            }

            var context = new StudentRegistrationDbContext(options.Options);
            await context.Database.EnsureCreatedAsync();
            await context.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE [Spec004BusinessState]
                (
                    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                    [Value] nvarchar(100) NOT NULL
                );
                """);
            return new AuditProbeDatabase(context);
        }

        public Task InsertBusinessStateAsync() =>
            Context.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO [Spec004BusinessState] ([Id], [Value]) VALUES ({Guid.NewGuid()}, {"synthetic"})");

        public Task<int> BusinessStateCountAsync() =>
            Context.Database.SqlQueryRaw<int>(
                    "SELECT COUNT(*) AS [Value] FROM [Spec004BusinessState]")
                .SingleAsync();

        public async ValueTask DisposeAsync()
        {
            await Context.Database.EnsureDeletedAsync();
            await Context.DisposeAsync();
        }
    }
}
