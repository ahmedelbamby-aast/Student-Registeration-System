using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Data.SqlClient;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Infrastructure.SqlServer.Registration;
using StudentRegistration.IntegrationTests.Infrastructure;
using StudentRegistration.Registration.Domain;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.IntegrationTests.Registration;

[Collection(Spec014SqlWorkstreamCollection.Name)]
public sealed class SqlSeatAllocatorConcurrencyTests(
    Spec014SqlWorkstreamDatabase database)
{
    [Fact]
    public void Allocator_exposes_only_bounded_atomic_operations()
    {
        var methods = typeof(SqlSeatAllocator).GetMethods()
            .Where(method => method.DeclaringType == typeof(SqlSeatAllocator))
            .Select(method => method.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("AllocateAsync", methods);
        Assert.Contains("ReduceCapacityAsync", methods);
        Assert.Contains("AllocateWithSavepointAsync", methods);
        Assert.Contains("AllocateAllOrRejectAsync", methods);
        Assert.Contains("HoldAllOrRejectAsync", methods);
        Assert.Contains("ConvertHoldsToEnrollmentsAsync", methods);
        Assert.Contains("ReleaseHoldsAsync", methods);
        Assert.DoesNotContain("DeallocateAsync", methods);
    }

    [Fact]
    public void Database_model_keeps_capacity_and_duplicate_enrollment_as_final_guards()
    {
        using var context = CreateContext();
        var model = context.GetService<IDesignTimeModel>().Model;
        var group = model.FindEntityType(typeof(SectionGroup));
        var enrollment = model.FindEntityType(typeof(Enrollment));

        Assert.NotNull(group);
        Assert.NotNull(enrollment);
        Assert.Contains(group!.GetCheckConstraints(), constraint =>
            constraint.Sql!.Contains("[EnrolledCount] + [HeldSeatCount] <= [Capacity]", StringComparison.Ordinal));
        Assert.Contains(enrollment!.GetIndexes(), index =>
            index.IsUnique &&
            index.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(Enrollment.StudentId), nameof(Enrollment.OfferingId)]));
        Assert.Contains(enrollment.GetForeignKeys(), foreignKey =>
            foreignKey.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(Enrollment.OfferingId), nameof(Enrollment.GroupId)]));
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Concurrent_hold_and_enrollment_compete_for_one_occupied_seat()
    {
        var seed = await database.SeedAsync(groupCapacities: [1]);
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var ready = 0;

        async Task<SeatAllocationBatchResult> HoldAsync()
        {
            await using var context = database.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync();
            var allocator = new SqlSeatAllocator(context);
            if (Interlocked.Increment(ref ready) == 2) gate.TrySetResult();
            await gate.Task.WaitAsync(TimeSpan.FromSeconds(10));
            var result = await allocator.HoldAllOrRejectAsync(seed.GroupIds);
            await transaction.CommitAsync();
            return result;
        }

        async Task<SeatAllocationBatchResult> EnrollAsync()
        {
            await using var context = database.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync();
            var allocator = new SqlSeatAllocator(context);
            if (Interlocked.Increment(ref ready) == 2) gate.TrySetResult();
            await gate.Task.WaitAsync(TimeSpan.FromSeconds(10));
            var result = await allocator.AllocateAllOrRejectAsync(seed.GroupIds);
            await transaction.CommitAsync();
            return result;
        }

        var results = await Task.WhenAll(HoldAsync(), EnrollAsync());

        Assert.Single(results, result => result.IsAccepted);
        Assert.Single(results, result => !result.IsAccepted && result.ReasonCode == "GROUP_FULL");
        Assert.Equal(1, await database.ScalarAsync<int>(
            "SELECT [EnrolledCount] + [HeldSeatCount] FROM [scheduling].[SectionGroups] WHERE [Id] = @id",
            new SqlParameter("@id", seed.GroupIds[0])));
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Hold_batch_rolls_back_all_lines_and_conversion_preserves_occupied_count()
    {
        var rejectedSeed = await database.SeedAsync(groupCapacities: [1, 0]);
        await using (var context = database.CreateContext())
        await using (var transaction = await context.Database.BeginTransactionAsync())
        {
            var result = await new SqlSeatAllocator(context)
                .HoldAllOrRejectAsync(rejectedSeed.GroupIds);
            Assert.False(result.IsAccepted);
            await transaction.CommitAsync();
        }
        Assert.Equal(0, await database.ScalarAsync<int>(
            "SELECT SUM([HeldSeatCount]) FROM [scheduling].[SectionGroups] WHERE [OfferingId] = @id",
            new SqlParameter("@id", rejectedSeed.OfferingId)));

        var acceptedSeed = await database.SeedAsync(groupCapacities: [1]);
        await using (var context = database.CreateContext())
        await using (var transaction = await context.Database.BeginTransactionAsync())
        {
            var allocator = new SqlSeatAllocator(context);
            Assert.True((await allocator.HoldAllOrRejectAsync(acceptedSeed.GroupIds)).IsAccepted);
            await allocator.ConvertHoldsToEnrollmentsAsync(acceptedSeed.GroupIds);
            await transaction.CommitAsync();
        }

        Assert.Equal(1, await database.ScalarAsync<int>(
            "SELECT [EnrolledCount] FROM [scheduling].[SectionGroups] WHERE [Id] = @id",
            new SqlParameter("@id", acceptedSeed.GroupIds[0])));
        Assert.Equal(0, await database.ScalarAsync<int>(
            "SELECT [HeldSeatCount] FROM [scheduling].[SectionGroups] WHERE [Id] = @id",
            new SqlParameter("@id", acceptedSeed.GroupIds[0])));
    }

    [Fact]
    public async Task Allocator_rejects_invalid_batches_and_requires_the_callers_transaction()
    {
        Assert.Throws<ArgumentNullException>(() => new SqlSeatAllocator(null!));
        await using var context = CreateContext();
        var allocator = new SqlSeatAllocator(context);

        await Assert.ThrowsAsync<ArgumentException>(() => allocator.AllocateAsync(Guid.Empty));
        await Assert.ThrowsAsync<InvalidOperationException>(() => allocator.AllocateAsync(Guid.NewGuid()));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            allocator.ReduceCapacityAsync(Guid.NewGuid(), -1));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            allocator.AllocateWithSavepointAsync(null!));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            allocator.AllocateWithSavepointAsync([]));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            allocator.RollbackAllocationAsync());

        await using var databaseContext = database.CreateContext();
        await using var transaction = await databaseContext.Database.BeginTransactionAsync();
        var transactionalAllocator = new SqlSeatAllocator(databaseContext);
        await Assert.ThrowsAsync<ArgumentException>(() =>
            transactionalAllocator.AllocateWithSavepointAsync([]));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            transactionalAllocator.AllocateWithSavepointAsync([Guid.Empty]));
        var duplicate = Guid.NewGuid();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            transactionalAllocator.AllocateWithSavepointAsync([duplicate, duplicate]));
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Conditional_final_seat_update_has_one_winner_and_never_overbooks()
    {
        var seed = await database.SeedAsync(groupCapacities: [1]);
        var gate = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var ready = 0;

        async Task<SeatAllocationResult> AllocateAsync()
        {
            await using var context = database.CreateContext();
            await using var transaction =
                await context.Database.BeginTransactionAsync();
            var allocator = new SqlSeatAllocator(context);
            if (Interlocked.Increment(ref ready) == 2)
            {
                gate.TrySetResult();
            }

            await gate.Task.WaitAsync(TimeSpan.FromSeconds(10));
            var result = await allocator.AllocateAsync(seed.GroupIds[0]);
            await transaction.CommitAsync();
            return result;
        }

        var results = await Task.WhenAll(AllocateAsync(), AllocateAsync());

        Assert.Single(results, result => result.IsAllocated);
        Assert.Single(results, result =>
            !result.IsAllocated && result.ReasonCode == "GROUP_FULL");
        Assert.Equal(
            1,
            await database.ScalarAsync<int>(
                "SELECT [EnrolledCount] FROM [scheduling].[SectionGroups] WHERE [Id] = @id",
                new SqlParameter("@id", seed.GroupIds[0])));
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Multi_group_failure_rolls_every_seat_back_to_allocation_savepoint()
    {
        var seed = await database.SeedAsync(groupCapacities: [1, 0]);
        await using var context = database.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync();
        var allocator = new SqlSeatAllocator(context);

        var result = await allocator.AllocateWithSavepointAsync(seed.GroupIds);

        Assert.False(result.IsAccepted);
        Assert.Equal("GROUP_FULL", result.ReasonCode);
        Assert.Empty(result.AllocatedGroupIds);
        await transaction.CommitAsync();
        Assert.Equal(
            0,
            await database.ScalarAsync<int>(
                "SELECT SUM([EnrolledCount]) FROM [scheduling].[SectionGroups] WHERE [OfferingId] = @id",
                new SqlParameter("@id", seed.OfferingId)));
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Successful_batch_and_capacity_reduction_share_the_callers_transaction()
    {
        var seed = await database.SeedAsync(groupCapacities: [2, 2]);
        await using var context = database.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync();
        var allocator = new SqlSeatAllocator(context);

        Assert.True(await allocator.ReduceCapacityAsync(seed.GroupIds[0], 2));
        var result = await allocator.AllocateAllOrRejectAsync(seed.GroupIds);
        Assert.True(result.IsAccepted);
        Assert.Equal(seed.GroupIds.Order(), result.AllocatedGroupIds);
        Assert.False(await allocator.ReduceCapacityAsync(seed.GroupIds[0], 0));

        await transaction.CommitAsync();
        Assert.Equal(
            2,
            await database.ScalarAsync<int>(
                "SELECT SUM([EnrolledCount]) FROM [scheduling].[SectionGroups] WHERE [OfferingId] = @id",
                new SqlParameter("@id", seed.OfferingId)));
    }

    private static StudentRegistrationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Spec014Model;Trusted_Connection=True")
            .Options;
        return new StudentRegistrationDbContext(options);
    }
}

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class Spec014SqlWorkstreamCollection :
    ICollectionFixture<Spec014SqlWorkstreamDatabase>
{
    public const string Name = "SPEC-014 SQL workstreams";
}

public sealed class Spec014SqlWorkstreamDatabase : IAsyncLifetime
{
    private readonly SqlServerContainerFixture _container = new();
    private string _connectionString = string.Empty;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        var databaseName = $"Spec014Workstreams_{Guid.NewGuid():N}";
        await using (var connection = new SqlConnection(_container.ConnectionString))
        {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = $"CREATE DATABASE [{databaseName}]";
            await command.ExecuteNonQueryAsync();
        }

        _connectionString = new SqlConnectionStringBuilder(_container.ConnectionString)
        {
            InitialCatalog = databaseName
        }.ConnectionString;
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    public StudentRegistrationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(_connectionString)
            .Options;
        return new StudentRegistrationDbContext(options);
    }

    public async Task<Spec014SqlSeed> SeedAsync(
        IReadOnlyList<int>? groupCapacities = null,
        int initialEnrolledCount = 0,
        int activeEnrollmentCount = 0)
    {
        groupCapacities ??= [1];
        var userId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var termId = Guid.NewGuid();
        var secondTermId = Guid.NewGuid();
        var draftId = Guid.NewGuid();
        var catalogueVersionId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var offeringId = Guid.NewGuid();
        var groupIds = groupCapacities.Select(_ => Guid.NewGuid()).ToArray();
        var token = Guid.NewGuid().ToString("N");
        var now = DateTime.UtcNow;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        async Task ExecuteAsync(string sql, params SqlParameter[] parameters)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = (SqlTransaction)transaction;
            command.CommandText = sql;
            command.Parameters.AddRange(parameters);
            await command.ExecuteNonQueryAsync();
        }

        await ExecuteAsync(
            """
            INSERT [academics].[AcademicTerms]
              ([Id],[Code],[CreationClientRequestId],[CreationPayloadHash],[DisplayName],[TeachingStartsOn],[TeachingEndsOn],[TimeZoneId],[State])
            VALUES
              (@term,@code,@request,@hash,@display,'2026-09-01','2026-12-31','Africa/Cairo','draft'),
              (@term2,@code2,@request2,@hash,@display2,'2027-02-01','2027-06-01','Africa/Cairo','draft')
            """,
            new SqlParameter("@term", termId),
            new SqlParameter("@term2", secondTermId),
            new SqlParameter("@code", $"T-{token}"),
            new SqlParameter("@code2", $"T2-{token}"),
            new SqlParameter("@request", Guid.NewGuid()),
            new SqlParameter("@request2", Guid.NewGuid()),
            new SqlParameter("@hash", token),
            new SqlParameter("@display", $"Term {token}"),
            new SqlParameter("@display2", $"Term 2 {token}"));
        await ExecuteAsync(
            """
            INSERT [auth].[ApplicationUsers]
              ([Id],[UserName],[NormalizedUserName],[UniversityId],[PasswordHash],[SecurityStamp],[IsEnabled],[AccessFailedCount],[LockoutEndUtc])
            VALUES (@id,@name,@normalized,@university,'hash','stamp',1,0,NULL)
            """,
            new SqlParameter("@id", userId),
            new SqlParameter("@name", $"student-{token}"),
            new SqlParameter("@normalized", $"STUDENT-{token}"),
            new SqlParameter("@university", token[..12]));
        await ExecuteAsync(
            """
            INSERT [academics].[Students]
              ([Id],[ApplicationUserId],[ProgramCode],[Cohort],[CurrentGpa],[EarnedCredits],[Standing],[IsActive],[Source],[SourceReference],[DataVersion],[DataAsOfUtc],[ImportedAtUtc])
            VALUES (@id,@user,'AI','2026',3.0,0,'good',1,'synthetic','SPEC-014','v1',@now,@now)
            """,
            new SqlParameter("@id", studentId),
            new SqlParameter("@user", userId),
            new SqlParameter("@now", now));
        await ExecuteAsync(
            """
            INSERT [academics].[CatalogueDrafts]
              ([Id],[ScopeCode],[BasedOnVersionId],[CanonicalContentHash],[ContentJson],[ValidationSummaryJson],[State])
            VALUES (@id,@scope,NULL,@hash,'{}','{}','published')
            """,
            new SqlParameter("@id", draftId),
            new SqlParameter("@scope", $"S-{token}"),
            new SqlParameter("@hash", token));
        await ExecuteAsync(
            """
            INSERT [academics].[CatalogueVersions]
              ([Id],[SourceDraftId],[SupersedesId],[ScopeCode],[VersionCode],[SourceReference],[EffectiveFromUtc],[PublishedAtUtc],[PublishedBy],[State])
            VALUES (@id,@draft,NULL,@scope,@version,'SPEC-014',@now,@now,'test','published')
            """,
            new SqlParameter("@id", catalogueVersionId),
            new SqlParameter("@draft", draftId),
            new SqlParameter("@scope", $"S-{token}"),
            new SqlParameter("@version", $"V-{token}"),
            new SqlParameter("@now", now));
        await ExecuteAsync(
            """
            INSERT [academics].[Courses]
              ([Id],[CatalogueVersionId],[Code],[Title],[Credits],[IsActive],[ProvenanceSourceReference],[ProvenanceAccessedOn],[ProvenanceSourceKind],[ProvenanceSyntheticFieldsJson])
            VALUES (@id,@catalogue,@code,'Concurrency Test',3,1,'SPEC-014','2026-07-17','synthetic','{}')
            """,
            new SqlParameter("@id", courseId),
            new SqlParameter("@catalogue", catalogueVersionId),
            new SqlParameter("@code", $"C-{token[..10]}"));
        await ExecuteAsync(
            """
            INSERT [scheduling].[CourseOfferings] ([Id],[TermId],[CourseId],[State])
            VALUES (@id,@term,@course,'published')
            """,
            new SqlParameter("@id", offeringId),
            new SqlParameter("@term", termId),
            new SqlParameter("@course", courseId));
        for (var index = 0; index < groupIds.Length; index++)
        {
            await ExecuteAsync(
                """
                INSERT [scheduling].[SectionGroups]
                  ([Id],[OfferingId],[GroupCode],[Capacity],[EnrolledCount],[State],[RegistrationPaused])
                VALUES (@id,@offering,@code,@capacity,@enrolled,'published',0)
                """,
                new SqlParameter("@id", groupIds[index]),
                new SqlParameter("@offering", offeringId),
                new SqlParameter("@code", $"G-{index}-{token[..8]}"),
                new SqlParameter("@capacity", groupCapacities[index]),
                new SqlParameter("@enrolled", initialEnrolledCount));
        }

        Guid? submissionId = null;
        if (activeEnrollmentCount > 0)
        {
            submissionId = Guid.NewGuid();
            await ExecuteAsync(
                """
                INSERT [registration].[RegistrationSubmissions]
                  ([Id],[StudentId],[TermId],[ClientRequestId],[PayloadHash],[ProcessingState],[ResultCode],[Reference],[ReceiptSnapshotJson],[DecisionSnapshotJson],[ReceivedAtUtc],[UpdatedAtUtc],[CompletedAtUtc])
                VALUES (@id,@student,@term,@request,'seed','accepted','ACCEPTED',@reference,'{}','{}',@now,@now,@now)
                """,
                new SqlParameter("@id", submissionId.Value),
                new SqlParameter("@student", studentId),
                new SqlParameter("@term", termId),
                new SqlParameter("@request", Guid.NewGuid()),
                new SqlParameter("@reference", $"REG-{token[..12]}"),
                new SqlParameter("@now", now));
            for (var index = 0; index < activeEnrollmentCount; index++)
            {
                await ExecuteAsync(
                    """
                    INSERT [registration].[Enrollments]
                      ([Id],[StudentId],[OfferingId],[GroupId],[SubmissionId],[State],[RegisteredAtUtc])
                    VALUES (@id,@student,@offering,@groupId,@submission,'active',@now)
                    """,
                    new SqlParameter("@id", Guid.NewGuid()),
                    new SqlParameter("@student", studentId),
                    new SqlParameter("@offering", offeringId),
                    new SqlParameter("@groupId", groupIds[0]),
                    new SqlParameter("@submission", submissionId.Value),
                    new SqlParameter("@now", now));
            }
        }

        await transaction.CommitAsync();
        return new Spec014SqlSeed(
            studentId,
            termId,
            secondTermId,
            offeringId,
            groupIds,
            submissionId);
    }

    public async Task<T> ScalarAsync<T>(
        string sql,
        params SqlParameter[] parameters)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddRange(parameters);
        var value = await command.ExecuteScalarAsync();
        Assert.NotNull(value);
        return (T)Convert.ChangeType(value, typeof(T))!;
    }
}

public sealed record Spec014SqlSeed(
    Guid StudentId,
    Guid TermId,
    Guid SecondTermId,
    Guid OfferingId,
    Guid[] GroupIds,
    Guid? SubmissionId);
