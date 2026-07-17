using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.IntegrationTests.Infrastructure;
using StudentRegistration.StaffAdministration.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec017;

public sealed class ExportJobModelTests
{
    private static readonly DateTime CreatedAtUtc =
        new(2026, 7, 17, 8, 30, 0, DateTimeKind.Utc);

    [Fact]
    public void New_job_binds_owner_scope_request_and_starts_pending()
    {
        var ownerId = Guid.NewGuid();
        var clientRequestId = Guid.NewGuid();
        var job = new ExportJob(
            Guid.NewGuid(),
            ownerId,
            clientRequestId,
            "SHA256:SCOPE",
            "SHA256:REQUEST",
            CreatedAtUtc);

        Assert.Equal(ownerId, job.OwnerId);
        Assert.Equal(clientRequestId, job.ClientRequestId);
        Assert.Equal("SHA256:SCOPE", job.ScopeHash);
        Assert.Equal("SHA256:REQUEST", job.RequestHash);
        Assert.Equal(ExportJobState.Pending, job.State);
        Assert.Equal(CreatedAtUtc, job.CreatedAtUtc);
        Assert.Equal(0, job.AttemptCount);
        Assert.Null(job.LeaseOwnerId);
        Assert.Null(job.LeaseExpiresAtUtc);
        Assert.Null(job.ArtifactId);
        Assert.Null(job.FailureCode);
        Assert.Null(job.CompletedAtUtc);
        Assert.Null(job.ExpiresAtUtc);
        Assert.Empty(job.Version);
    }

    [Fact]
    public void Lease_is_sixty_seconds_renewable_and_only_current_owner_can_act()
    {
        var job = Create();

        Assert.True(job.TryClaim("replica-a", CreatedAtUtc));
        Assert.Equal(ExportJobState.Running, job.State);
        Assert.Equal(1, job.AttemptCount);
        Assert.Equal("replica-a", job.LeaseOwnerId);
        Assert.Equal(CreatedAtUtc.AddSeconds(60), job.LeaseExpiresAtUtc);
        Assert.False(job.TryClaim("replica-b", CreatedAtUtc.AddSeconds(30)));
        Assert.Throws<InvalidOperationException>(() =>
            job.RenewLease("replica-b", CreatedAtUtc.AddSeconds(30)));

        job.RenewLease("replica-a", CreatedAtUtc.AddSeconds(30));

        Assert.Equal(CreatedAtUtc.AddSeconds(90), job.LeaseExpiresAtUtc);
    }

    [Fact]
    public void Expired_lease_can_be_reclaimed_but_attempts_are_capped_at_three()
    {
        var job = Create();

        Assert.True(job.TryClaim("replica-a", CreatedAtUtc));
        Assert.True(job.TryClaim("replica-b", CreatedAtUtc.AddSeconds(61)));
        Assert.True(job.TryClaim("replica-c", CreatedAtUtc.AddSeconds(122)));
        Assert.Equal(3, job.AttemptCount);
        Assert.False(job.TryClaim("replica-d", CreatedAtUtc.AddSeconds(183)));
        Assert.Equal(ExportJobState.Failed, job.State);
        Assert.Equal("EXPORT_ATTEMPTS_EXHAUSTED", job.FailureCode);
        Assert.Null(job.LeaseOwnerId);
        Assert.Null(job.LeaseExpiresAtUtc);
    }

    [Fact]
    public void Current_lease_owner_publishes_exactly_one_expiring_artifact()
    {
        var job = Create();
        var artifactId = Guid.NewGuid();
        job.TryClaim("replica-a", CreatedAtUtc);

        job.Complete(
            "replica-a",
            artifactId,
            CreatedAtUtc.AddSeconds(30),
            CreatedAtUtc.AddHours(1));

        Assert.Equal(ExportJobState.Complete, job.State);
        Assert.Equal(artifactId, job.ArtifactId);
        Assert.Equal(CreatedAtUtc.AddSeconds(30), job.CompletedAtUtc);
        Assert.Equal(CreatedAtUtc.AddHours(1), job.ExpiresAtUtc);
        Assert.Null(job.LeaseOwnerId);
        Assert.Null(job.LeaseExpiresAtUtc);
        Assert.Throws<InvalidOperationException>(() => job.Complete(
            "replica-a",
            Guid.NewGuid(),
            CreatedAtUtc.AddMinutes(6),
            CreatedAtUtc.AddHours(2)));

        Assert.True(job.Expire(CreatedAtUtc.AddHours(1)));
        Assert.Equal(ExportJobState.Expired, job.State);
        Assert.False(job.Expire(CreatedAtUtc.AddHours(2)));
    }

    [Fact]
    public void Failure_can_retry_within_bound_and_become_terminal()
    {
        var job = Create();
        job.TryClaim("replica-a", CreatedAtUtc);

        job.RecordFailure(
            "replica-a",
            CreatedAtUtc.AddSeconds(10),
            retryable: true,
            "EXPORT_STORAGE_FAILED");
        Assert.Equal(ExportJobState.Pending, job.State);
        Assert.Null(job.FailureCode);

        job.TryClaim("replica-b", CreatedAtUtc.AddSeconds(11));
        job.RecordFailure(
            "replica-b",
            CreatedAtUtc.AddSeconds(12),
            retryable: false,
            "EXPORT_PAYLOAD_INVALID");
        Assert.Equal(ExportJobState.Failed, job.State);
        Assert.Equal(CreatedAtUtc.AddSeconds(12), job.CompletedAtUtc);
        Assert.Equal("EXPORT_PAYLOAD_INVALID", job.FailureCode);
        Assert.False(job.TryClaim("replica-c", CreatedAtUtc.AddMinutes(1)));
    }

    [Fact]
    public void Invalid_identity_hash_time_and_transitions_are_rejected()
    {
        Assert.Throws<ArgumentException>(() => Create(id: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(ownerId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(clientRequestId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(scopeHash: " "));
        Assert.Throws<ArgumentException>(() => Create(requestHash: " "));
        Assert.Throws<ArgumentException>(() => Create(
            createdAtUtc: DateTime.SpecifyKind(CreatedAtUtc, DateTimeKind.Local)));

        var job = Create();
        Assert.Throws<ArgumentException>(() => job.TryClaim(" ", CreatedAtUtc));
        Assert.Throws<ArgumentException>(() => job.TryClaim(
            "replica-a",
            DateTime.SpecifyKind(CreatedAtUtc, DateTimeKind.Unspecified)));
        Assert.Throws<InvalidOperationException>(() => job.Complete(
            "replica-a",
            Guid.NewGuid(),
            CreatedAtUtc,
            CreatedAtUtc.AddHours(1)));
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Sql_model_enforces_idempotency_artifact_lease_and_rowversion_rules()
    {
        await using var sqlServer = new SqlServerContainerFixture();
        await sqlServer.StartAsync();
        var connectionString = new SqlConnectionStringBuilder(sqlServer.ConnectionString)
        {
            InitialCatalog = $"StudentRegistration_Test_ExportJob_{Guid.NewGuid():N}"
        }.ConnectionString;

        await using var setup = CreateContext(connectionString);
        try
        {
            await setup.Database.MigrateAsync();
            var ownerId = Guid.NewGuid();
            var clientRequestId = Guid.NewGuid();
            var persisted = Create(ownerId: ownerId, clientRequestId: clientRequestId);
            setup.Add(persisted);
            await setup.SaveChangesAsync();
            Assert.NotEmpty(persisted.Version);

            await using (var duplicateContext = CreateContext(connectionString))
            {
                duplicateContext.Add(Create(
                    ownerId: ownerId,
                    clientRequestId: clientRequestId,
                    requestHash: "SHA256:DIFFERENT"));
                await Assert.ThrowsAsync<DbUpdateException>(
                    () => duplicateContext.SaveChangesAsync());
            }

            await AssertSqlRejectedAsync(
                connectionString,
                "running",
                artifactId: null,
                completedAtUtc: null,
                expiresAtUtc: null,
                leaseOwnerId: null,
                leaseExpiresAtUtc: null,
                attemptCount: 1,
                failureCode: null);
            await AssertSqlRejectedAsync(
                connectionString,
                "complete",
                artifactId: null,
                completedAtUtc: CreatedAtUtc.AddSeconds(10),
                expiresAtUtc: CreatedAtUtc.AddMinutes(15),
                leaseOwnerId: null,
                leaseExpiresAtUtc: null,
                attemptCount: 1,
                failureCode: null);

            var sharedArtifactId = Guid.NewGuid();
            var firstArtifactJob = Create();
            var secondArtifactJob = Create();
            setup.AddRange(firstArtifactJob, secondArtifactJob);
            await setup.SaveChangesAsync();
            setup.ChangeTracker.Clear();

            await CompleteAsync(connectionString, firstArtifactJob.Id, sharedArtifactId);
            await Assert.ThrowsAsync<DbUpdateException>(() =>
                CompleteAsync(connectionString, secondArtifactJob.Id, sharedArtifactId));

            var rowversionJob = Create();
            setup.Add(rowversionJob);
            await setup.SaveChangesAsync();
            setup.ChangeTracker.Clear();

            await using var replicaOne = CreateContext(connectionString);
            await using var replicaTwo = CreateContext(connectionString);
            var firstCopy = await replicaOne.Set<ExportJob>().SingleAsync(
                candidate => candidate.Id == rowversionJob.Id);
            var secondCopy = await replicaTwo.Set<ExportJob>().SingleAsync(
                candidate => candidate.Id == rowversionJob.Id);
            Assert.True(firstCopy.TryClaim("replica-one", CreatedAtUtc));
            Assert.True(secondCopy.TryClaim("replica-two", CreatedAtUtc));
            await replicaOne.SaveChangesAsync();
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                () => replicaTwo.SaveChangesAsync());
        }
        finally
        {
            await setup.Database.EnsureDeletedAsync();
        }
    }

    private static ExportJob Create(
        Guid? id = null,
        Guid? ownerId = null,
        Guid? clientRequestId = null,
        string scopeHash = "SHA256:SCOPE",
        string requestHash = "SHA256:REQUEST",
        DateTime? createdAtUtc = null) =>
        new(
            id ?? Guid.NewGuid(),
            ownerId ?? Guid.NewGuid(),
            clientRequestId ?? Guid.NewGuid(),
            scopeHash,
            requestHash,
            createdAtUtc ?? CreatedAtUtc);

    private static StudentRegistrationDbContext CreateContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new StudentRegistrationDbContext(options);
    }

    private static async Task CompleteAsync(
        string connectionString,
        Guid jobId,
        Guid artifactId)
    {
        await using var context = CreateContext(connectionString);
        var job = await context.Set<ExportJob>().SingleAsync(candidate => candidate.Id == jobId);
        Assert.True(job.TryClaim($"worker-{jobId:N}", CreatedAtUtc));
        job.Complete(
            $"worker-{jobId:N}",
            artifactId,
            CreatedAtUtc.AddSeconds(10),
            CreatedAtUtc.AddMinutes(15));
        await context.SaveChangesAsync();
    }

    private static async Task AssertSqlRejectedAsync(
        string connectionString,
        string state,
        Guid? artifactId,
        DateTime? completedAtUtc,
        DateTime? expiresAtUtc,
        string? leaseOwnerId,
        DateTime? leaseExpiresAtUtc,
        int attemptCount,
        string? failureCode)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO [administration].[ExportJobs]
            ([JobId], [OwnerId], [ClientRequestId], [ScopeHash], [RequestHash],
             [State], [ArtifactId], [FailureCode], [CreatedAtUtc], [CompletedAtUtc],
             [ExpiresAtUtc], [LeaseOwnerId], [LeaseExpiresAtUtc], [AttemptCount])
            VALUES
            (@jobId, @ownerId, @clientRequestId, N'SHA256:SCOPE', N'SHA256:REQUEST',
             @state, @artifactId, @failureCode, @createdAtUtc, @completedAtUtc,
             @expiresAtUtc, @leaseOwnerId, @leaseExpiresAtUtc, @attemptCount);
            """;
        command.Parameters.AddWithValue("@jobId", Guid.NewGuid());
        command.Parameters.AddWithValue("@ownerId", Guid.NewGuid());
        command.Parameters.AddWithValue("@clientRequestId", Guid.NewGuid());
        command.Parameters.AddWithValue("@state", state);
        command.Parameters.AddWithValue("@artifactId", (object?)artifactId ?? DBNull.Value);
        command.Parameters.AddWithValue("@failureCode", (object?)failureCode ?? DBNull.Value);
        command.Parameters.AddWithValue("@createdAtUtc", CreatedAtUtc);
        command.Parameters.AddWithValue("@completedAtUtc", (object?)completedAtUtc ?? DBNull.Value);
        command.Parameters.AddWithValue("@expiresAtUtc", (object?)expiresAtUtc ?? DBNull.Value);
        command.Parameters.AddWithValue("@leaseOwnerId", (object?)leaseOwnerId ?? DBNull.Value);
        command.Parameters.AddWithValue("@leaseExpiresAtUtc", (object?)leaseExpiresAtUtc ?? DBNull.Value);
        command.Parameters.AddWithValue("@attemptCount", attemptCount);

        await Assert.ThrowsAsync<SqlException>(command.ExecuteNonQueryAsync);
    }
}
