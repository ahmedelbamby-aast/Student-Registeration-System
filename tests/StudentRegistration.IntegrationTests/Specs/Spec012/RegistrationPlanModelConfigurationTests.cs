using System.Reflection;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Infrastructure.SqlServer.Registration;
using StudentRegistration.IntegrationTests.Infrastructure;
using StudentRegistration.Registration.Application.Ports;
using StudentRegistration.Registration.Domain;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec012;

public sealed class RegistrationPlanModelConfigurationTests
{
    private const string ModelConnectionString =
        "Server=localhost;Database=Spec012ModelOnly;User Id=sa;Password=NotUsed!42;TrustServerCertificate=True";

    [Fact]
    public void Model_maps_plan_items_conflicts_snapshot_constraints_and_query_indexes()
    {
        using var context = CreateContext(ModelConnectionString);
        var plan = Entity<RegistrationPlan>(context);
        var item = Entity<RegistrationPlanItem>(context);

        Assert.Equal("RegistrationPlans", plan.GetTableName());
        Assert.Equal("registration", plan.GetSchema());
        Assert.Equal("RegistrationPlanItems", item.GetTableName());
        Assert.Equal("registration", item.GetSchema());

        AssertUnique(plan, nameof(RegistrationPlan.StudentId), nameof(RegistrationPlan.TermId));
        AssertRowVersion(plan, nameof(RegistrationPlan.Version));
        Assert.Equal("ConflictsJson", plan.FindProperty("Conflicts")?.GetColumnName());
        Assert.Equal(
            "ValidationSnapshotJson",
            plan.FindProperty("Validation")?.GetColumnName());
        Assert.Equal("nvarchar(max)", plan.FindProperty("Conflicts")?.GetColumnType());
        Assert.Equal(
            "nvarchar(max)",
            plan.FindProperty("Validation")?.GetColumnType());

        AssertUnique(
            item,
            nameof(RegistrationPlanItem.PlanId),
            nameof(RegistrationPlanItem.OfferingId));
        AssertIndex(
            item,
            nameof(RegistrationPlanItem.PlanId),
            nameof(RegistrationPlanItem.SelectedGroupId));
        AssertForeignKey<RegistrationPlanItem, RegistrationPlan>(
            item,
            nameof(RegistrationPlanItem.PlanId));
        AssertForeignKey<RegistrationPlanItem, CourseOffering>(
            item,
            nameof(RegistrationPlanItem.OfferingId));
        AssertForeignKey<RegistrationPlanItem, SectionGroup>(
            item,
            nameof(RegistrationPlanItem.OfferingId),
            nameof(RegistrationPlanItem.SelectedGroupId));

        var group = Entity<SectionGroup>(context);
        Assert.Contains(
            group.GetKeys(),
            key => key.Properties.Select(property => property.Name).SequenceEqual(
                [nameof(SectionGroup.OfferingId), nameof(SectionGroup.Id)]));

        var script = context.Database.GenerateCreateScript();
        Assert.Contains("CK_RegistrationPlans_State", script, StringComparison.Ordinal);
        Assert.Contains("CK_RegistrationPlans_TotalCredits", script, StringComparison.Ordinal);
        Assert.Contains("ConflictsJson", script, StringComparison.Ordinal);
        Assert.Contains("ValidationSnapshotJson", script, StringComparison.Ordinal);
    }

    [Fact]
    public void Current_plan_query_is_owner_term_scoped_parameterized_and_no_tracking()
    {
        using var context = CreateContext(ModelConnectionString);
        var adapter = new RegistrationDiscoveryQueryAdapter(context);
        var studentId = Guid.Parse("12000000-0000-0000-0000-000000000001");
        var termId = Guid.Parse("12000000-0000-0000-0000-000000000002");

        var query = PrivateQuery<RegistrationPlan>(
            adapter,
            "CurrentPlanForStudentTermQuery",
            studentId,
            termId);
        var sql = query.ToQueryString();

        Assert.Contains("DECLARE @studentId", sql, StringComparison.Ordinal);
        Assert.Contains("DECLARE @termId", sql, StringComparison.Ordinal);
        Assert.Contains("[r].[StudentId] = @studentId", sql, StringComparison.Ordinal);
        Assert.Contains("[r].[TermId] = @termId", sql, StringComparison.Ordinal);
        Assert.Equal(
            QueryTrackingBehavior.NoTracking,
            context.ChangeTracker.QueryTrackingBehavior);
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Real_sql_round_trips_plan_values_and_enforces_unique_scope_and_offering()
    {
        await using var sqlServer = new SqlServerContainerFixture();
        await sqlServer.StartAsync();
        var connectionString = new SqlConnectionStringBuilder(sqlServer.ConnectionString)
        {
            InitialCatalog = $"StudentRegistration_Test_Spec012_{Guid.NewGuid():N}"
        }.ConnectionString;

        var commandCounter = new ReaderCommandCounter();
        await using var context = CreateContext(connectionString, commandCounter);
        try
        {
            Assert.True(await context.Database.EnsureCreatedAsync());
            await SeedPlanDependenciesAsync(context);

            var plan = CreatePlan();
            plan.ReplaceSelections(
                [new(
                    Guid.Parse("12000000-0000-0000-0000-000000000020"),
                    Guid.Parse("12000000-0000-0000-0000-000000000006"),
                    Guid.Parse("12000000-0000-0000-0000-000000000007"),
                    "offering/1",
                    "group/1")],
                plan.TotalCredits,
                plan.State,
                plan.Conflicts,
                plan.Validation!);
            var item = Assert.Single(plan.Items);
            context.Add(plan);
            await context.SaveChangesAsync();
            Assert.NotEmpty(plan.Version);

            context.ChangeTracker.Clear();
            var stored = await context.Set<RegistrationPlan>()
                .AsNoTracking()
                .SingleAsync(candidate => candidate.Id == plan.Id);
            Assert.Single(stored.Conflicts);
            Assert.Equal("MEETING_OVERLAP", stored.Conflicts[0].Code);
            Assert.NotNull(stored.Validation);
            Assert.Single(stored.Validation!.OfferingVersions);

            var reader = new RegistrationDiscoveryQueryAdapter(context);
            commandCounter.Reset();
            var current = await reader.ReadAsync(plan.StudentId, plan.TermId);
            Assert.Equal(1, commandCounter.Count);
            Assert.Equal(3m, current.Credits);
            var selection = Assert.Single(current.Selections);
            Assert.Equal(item.OfferingId, selection.OfferingId);
            Assert.Equal(item.SelectedGroupId, selection.GroupId);
            Assert.Single(selection.Meetings);
            Assert.NotEqual("initial-empty/1", current.Version);

            context.Add(new RegistrationPlan(
                Guid.NewGuid(),
                plan.StudentId,
                plan.TermId,
                0m,
                RegistrationPlanState.Draft));
            await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
            context.ChangeTracker.Clear();

            context.Add(CreateUnownedItem(
                Guid.NewGuid(),
                plan.Id,
                item.OfferingId,
                item.SelectedGroupId,
                "offering/1",
                "group/1"));
            await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
            context.ChangeTracker.Clear();

            context.Add(CreateUnownedItem(
                Guid.NewGuid(),
                Guid.Parse("12000000-0000-0000-0000-000000000099"),
                Guid.Parse("12000000-0000-0000-0000-000000000016"),
                Guid.Parse("12000000-0000-0000-0000-000000000017"),
                "offering/2",
                "group/2"));
            await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
            context.ChangeTracker.Clear();

            context.Add(CreateUnownedItem(
                Guid.NewGuid(),
                plan.Id,
                Guid.Parse("12000000-0000-0000-0000-000000000016"),
                item.SelectedGroupId,
                "offering/2",
                "group/1"));
            await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
            context.ChangeTracker.Clear();

            var expectedVersion = Convert.ToBase64String(stored.Version);
            var replacementValidation = new ValidationSnapshot(
                new DateTime(2026, 7, 16, 10, 0, 0, DateTimeKind.Utc),
                "academic/2",
                "policy/2",
                "catalogue/1",
                new Dictionary<Guid, string>
                {
                    [Guid.Parse("12000000-0000-0000-0000-000000000016")] = "offering/2"
                },
                new Dictionary<Guid, string>
                {
                    [Guid.Parse("12000000-0000-0000-0000-000000000017")] = "group/2"
                });
            var replacement = new RegistrationPlanStoreCommand(
                plan.StudentId,
                plan.TermId,
                expectedVersion,
                [new(
                    Guid.NewGuid(),
                    Guid.Parse("12000000-0000-0000-0000-000000000016"),
                    Guid.Parse("12000000-0000-0000-0000-000000000017"),
                    "offering/2",
                    "group/2")],
                3m,
                RegistrationPlanState.Draft,
                [],
                replacementValidation);

            await using var firstEditor = CreateContext(connectionString);
            await using var secondEditor = CreateContext(connectionString);
            var firstAdapter = new RegistrationPlanSqlServerAdapter(firstEditor);
            var secondAdapter = new RegistrationPlanSqlServerAdapter(secondEditor);

            Assert.Equal(
                plan.StudentId,
                await firstAdapter.ResolveStudentIdAsync(
                    Guid.Parse("12000000-0000-0000-0000-000000000001"),
                    plan.TermId));
            Assert.Null(await ((IRegistrationPlanStore)firstAdapter).ReadAsync(
                Guid.Parse("12000000-0000-0000-0000-000000000099"),
                plan.TermId));

            var winner = await ((IRegistrationPlanStore)firstAdapter)
                .ReplaceAsync(replacement);
            var stale = await ((IRegistrationPlanStore)secondAdapter)
                .ReplaceAsync(replacement);

            Assert.Equal(RegistrationPlanStoreOutcome.Updated, winner.Outcome);
            Assert.Equal(RegistrationPlanStoreOutcome.StaleVersion, stale.Outcome);
            Assert.NotNull(winner.Plan);
            Assert.NotNull(stale.Plan);
            Assert.NotEqual(expectedVersion, Convert.ToBase64String(winner.Plan.Version));
            Assert.Equal(winner.Plan.Version, stale.Plan.Version);
            var winningItem = Assert.Single(stale.Plan.Items);
            Assert.Equal(
                Guid.Parse("12000000-0000-0000-0000-000000000016"),
                winningItem.OfferingId);
            Assert.Equal(
                Guid.Parse("12000000-0000-0000-0000-000000000017"),
                winningItem.SelectedGroupId);
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
        }
    }

    private static RegistrationPlan CreatePlan()
    {
        var firstGroupId = Guid.Parse("12000000-0000-0000-0000-000000000007");
        var secondGroupId = Guid.Parse("12000000-0000-0000-0000-000000000008");
        var conflict = new ScheduleConflict(
            "MEETING_OVERLAP",
            new(
                firstGroupId,
                "G1",
                "CS101",
                "Algorithms",
                new TimeOnly(10, 0),
                new TimeOnly(11, 30)),
            new(
                secondGroupId,
                "G2",
                "DS201",
                "Data Science",
                new TimeOnly(11, 0),
                new TimeOnly(12, 0)),
            DayOfWeek.Monday,
            new TimeOnly(11, 0),
            new TimeOnly(11, 30),
            "Algorithms overlaps Data Science.",
            [
                new("change-group", firstGroupId, "Change G1", "/student/subjects/1"),
                new("remove-group", firstGroupId, "Remove G1", "/student/schedule"),
                new("change-group", secondGroupId, "Change G2", "/student/subjects/2"),
                new("remove-group", secondGroupId, "Remove G2", "/student/schedule")
            ]);
        var snapshot = new ValidationSnapshot(
            new DateTime(2026, 7, 16, 9, 0, 0, DateTimeKind.Utc),
            "academic/1",
            "policy/1",
            "catalogue/1",
            new Dictionary<Guid, string>
            {
                [Guid.Parse("12000000-0000-0000-0000-000000000006")] = "offering/1"
            },
            new Dictionary<Guid, string>
            {
                [firstGroupId] = "group/1"
            });
        return new RegistrationPlan(
            Guid.Parse("12000000-0000-0000-0000-000000000019"),
            Guid.Parse("12000000-0000-0000-0000-000000000003"),
            Guid.Parse("12000000-0000-0000-0000-000000000004"),
            3m,
            RegistrationPlanState.ReviewBlocked,
            [conflict],
            snapshot);
    }

    private static RegistrationPlanItem CreateUnownedItem(
        Guid id,
        Guid planId,
        Guid offeringId,
        Guid groupId,
        string offeringVersion,
        string groupVersion) =>
        (RegistrationPlanItem)(Activator.CreateInstance(
            typeof(RegistrationPlanItem),
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            args: [id, planId, offeringId, groupId, offeringVersion, groupVersion],
            culture: null) ?? throw new InvalidOperationException("Item construction failed."));

    private static async Task SeedPlanDependenciesAsync(StudentRegistrationDbContext context)
    {
        await context.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO [auth].[ApplicationUsers]
                ([Id], [UserName], [NormalizedUserName], [UniversityId], [PasswordHash],
                 [SecurityStamp], [IsEnabled], [AccessFailedCount], [LockoutEndUtc])
            VALUES
                ('12000000-0000-0000-0000-000000000001', N'student@test', N'STUDENT@TEST',
                 N'S012', N'hash', N'stamp', 1, 0, NULL);
            INSERT INTO [academics].[AcademicTerms]
                ([Id], [Code], [CreationClientRequestId], [CreationPayloadHash], [DisplayName],
                 [TeachingStartsOn], [TeachingEndsOn], [TimeZoneId], [State])
            VALUES
                ('12000000-0000-0000-0000-000000000004', N'2026-FALL',
                 '12000000-0000-0000-0000-000000000040', N'hash', N'Fall 2026',
                 '2026-09-01', '2026-12-31', N'Africa/Cairo', N'registrationOpen');
            INSERT INTO [academics].[Students]
                ([Id], [ApplicationUserId], [ProgramCode], [Cohort], [CurrentGpa], [EarnedCredits],
                 [Standing], [IsActive], [Source], [SourceReference], [DataVersion],
                 [DataAsOfUtc], [ImportedAtUtc])
            VALUES
                ('12000000-0000-0000-0000-000000000003',
                 '12000000-0000-0000-0000-000000000001', N'AI-DS', N'2023', 3.2, 96,
                 N'Active', 1, N'demo', N'SRC-DATA-SCIENCE', N'1',
                 '2026-07-13T00:00:00', '2026-07-13T00:00:00');
            INSERT INTO [academics].[StudentTermAcademicStates]
                ([Id], [StudentId], [TermId], [GpaAtStart], [EarnedCreditsAtStart],
                 [StandingAtStart], [Source], [SourceReference], [DataVersion], [DataAsOfUtc])
            VALUES
                ('12000000-0000-0000-0000-000000000043',
                 '12000000-0000-0000-0000-000000000003',
                 '12000000-0000-0000-0000-000000000004', 3.2, 96,
                 N'Active', N'demo', N'SRC-DATA-SCIENCE', N'1',
                 '2026-07-13T00:00:00');
            INSERT INTO [academics].[CatalogueDrafts]
                ([Id], [ScopeCode], [BasedOnVersionId], [CanonicalContentHash],
                 [ContentJson], [ValidationSummaryJson], [State])
            VALUES
                ('12000000-0000-0000-0000-000000000041', N'AI-DS', NULL, N'hash',
                 N'{{}}', N'{{}}', N'published');
            INSERT INTO [academics].[CatalogueVersions]
                ([Id], [SourceDraftId], [SupersedesId], [ScopeCode], [VersionCode],
                 [SourceReference], [EffectiveFromUtc], [PublishedAtUtc], [PublishedBy], [State])
            VALUES
                ('12000000-0000-0000-0000-000000000042',
                 '12000000-0000-0000-0000-000000000041', NULL, N'AI-DS', N'CAT-1',
                 N'SRC-DATA-SCIENCE', '2026-07-13T00:00:00', '2026-07-13T00:00:00',
                 N'Ahmed ELbamby', N'published');
            INSERT INTO [academics].[Courses]
                ([Id], [CatalogueVersionId], [Code], [Title], [Credits], [IsActive],
                 [ProvenanceSourceReference], [ProvenanceAccessedOn],
                 [ProvenanceSourceKind], [ProvenanceSyntheticFieldsJson])
            VALUES
                ('12000000-0000-0000-0000-000000000005',
                 '12000000-0000-0000-0000-000000000042', N'CS101', N'Algorithms', 3, 1,
                 N'SRC-DATA-SCIENCE', '2026-07-13', N'official-source', N'[]'),
                ('12000000-0000-0000-0000-000000000015',
                 '12000000-0000-0000-0000-000000000042', N'DS201', N'Data Science', 3, 1,
                 N'SRC-DATA-SCIENCE', '2026-07-13', N'official-source', N'[]');
            INSERT INTO [scheduling].[CourseOfferings] ([Id], [TermId], [CourseId], [State])
            VALUES
                ('12000000-0000-0000-0000-000000000006',
                 '12000000-0000-0000-0000-000000000004',
                 '12000000-0000-0000-0000-000000000005', N'published'),
                ('12000000-0000-0000-0000-000000000016',
                 '12000000-0000-0000-0000-000000000004',
                 '12000000-0000-0000-0000-000000000015', N'published');
            INSERT INTO [scheduling].[Rooms] ([Id], [Code], [Location], [Capacity], [AvailabilityState])
            VALUES
                ('12000000-0000-0000-0000-000000000009', N'R101', N'Main', 30, N'available');
            INSERT INTO [scheduling].[SectionGroups]
                ([Id], [OfferingId], [GroupCode], [Capacity], [EnrolledCount], [State], [RegistrationPaused])
            VALUES
                ('12000000-0000-0000-0000-000000000007',
                 '12000000-0000-0000-0000-000000000006', N'G1', 30, 10, N'published', 0),
                ('12000000-0000-0000-0000-000000000008',
                 '12000000-0000-0000-0000-000000000006', N'G2', 30, 10, N'published', 0);
            INSERT INTO [scheduling].[SectionGroups]
                ([Id], [OfferingId], [GroupCode], [Capacity], [EnrolledCount], [State], [RegistrationPaused])
            VALUES
                ('12000000-0000-0000-0000-000000000017',
                 '12000000-0000-0000-0000-000000000016', N'G3', 30, 10, N'published', 0);
            INSERT INTO [scheduling].[MeetingSlots]
                ([Id], [GroupId], [RoomId], [ActivityType], [DayOfWeek], [StartLocal], [EndLocal])
            VALUES
                ('12000000-0000-0000-0000-000000000010',
                 '12000000-0000-0000-0000-000000000007',
                 '12000000-0000-0000-0000-000000000009', N'lecture', 1, '10:00', '11:30');
            """);
    }

    private static StudentRegistrationDbContext CreateContext(
        string connectionString,
        params IInterceptor[] interceptors)
    {
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(connectionString)
            .AddInterceptors(interceptors)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .Options;
        return new StudentRegistrationDbContext(options);
    }

    private sealed class ReaderCommandCounter : DbCommandInterceptor
    {
        public int Count { get; private set; }

        public void Reset() => Count = 0;

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            Count++;
            return ValueTask.FromResult(result);
        }
    }

    private static IEntityType Entity<TEntity>(StudentRegistrationDbContext context) =>
        context.Model.FindEntityType(typeof(TEntity))
        ?? throw new InvalidOperationException($"{typeof(TEntity).Name} is missing.");

    private static IQueryable<TEntity> PrivateQuery<TEntity>(
        object target,
        string method,
        params object[] arguments)
        where TEntity : class =>
        Assert.IsAssignableFrom<IQueryable<TEntity>>(
            target.GetType().GetMethod(
                method,
                BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(target, arguments));

    private static void AssertRowVersion(IEntityType entity, string propertyName)
    {
        var property = entity.FindProperty(propertyName);
        Assert.NotNull(property);
        Assert.True(property.IsConcurrencyToken);
        Assert.Equal(ValueGenerated.OnAddOrUpdate, property.ValueGenerated);
    }

    private static void AssertUnique(IEntityType entity, params string[] properties)
    {
        var index = FindIndex(entity, properties);
        Assert.True(index.IsUnique);
    }

    private static void AssertIndex(IEntityType entity, params string[] properties) =>
        Assert.NotNull(FindIndex(entity, properties));

    private static IIndex FindIndex(IEntityType entity, params string[] properties) =>
        entity.GetIndexes().Single(index =>
            index.Properties.Select(property => property.Name).SequenceEqual(properties));

    private static void AssertForeignKey<TDependent, TPrincipal>(
        IEntityType dependent,
        params string[] properties) =>
        Assert.Contains(
            dependent.GetForeignKeys(),
            foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(TPrincipal) &&
                foreignKey.Properties.Select(property => property.Name).SequenceEqual(properties));
}
