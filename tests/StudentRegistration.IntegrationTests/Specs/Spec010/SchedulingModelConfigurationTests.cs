using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.IntegrationTests.Infrastructure;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec010;

public sealed class SchedulingModelConfigurationTests
{
    private const string ModelConnectionString =
        "Server=localhost;Database=Spec010ModelOnly;User Id=sa;Password=NotUsed!42;TrustServerCertificate=True";

    [Fact]
    public void Mapping_contributes_the_eight_owned_scheduling_entities()
    {
        using var context = CreateContext(ModelConnectionString);
        var expected = new[]
        {
            typeof(CourseOffering),
            typeof(GroupStaffAssignment),
            typeof(MeetingSlot),
            typeof(Room),
            typeof(ScheduleImpactAlert),
            typeof(SectionGroup),
            typeof(StaffAvailability),
            typeof(StaffTermAvailability),
        };

        Assert.Equal(
            expected.OrderBy(type => type.FullName, StringComparer.Ordinal),
            context.Model.GetEntityTypes()
                .Where(entity => entity.GetSchema() == "scheduling")
                .Select(entity => entity.ClrType)
                .Where(expected.Contains)
                .OrderBy(type => type.FullName, StringComparer.Ordinal));

        Assert.Equal("CourseOfferings", Entity<CourseOffering>(context).GetTableName());
        Assert.Equal("SectionGroups", Entity<SectionGroup>(context).GetTableName());
        Assert.Equal("MeetingSlots", Entity<MeetingSlot>(context).GetTableName());
        Assert.Equal("Rooms", Entity<Room>(context).GetTableName());
        Assert.Equal("GroupStaffAssignments", Entity<GroupStaffAssignment>(context).GetTableName());
        Assert.Equal("StaffTermAvailabilities", Entity<StaffTermAvailability>(context).GetTableName());
        Assert.Equal("StaffAvailabilities", Entity<StaffAvailability>(context).GetTableName());
        Assert.Equal("ScheduleImpactAlerts", Entity<ScheduleImpactAlert>(context).GetTableName());
    }

    [Fact]
    public void Mapping_enforces_aggregate_keys_resource_uniqueness_and_collision_indexes()
    {
        using var context = CreateContext(ModelConnectionString);
        var offering = Entity<CourseOffering>(context);
        var group = Entity<SectionGroup>(context);
        var meeting = Entity<MeetingSlot>(context);
        var room = Entity<Room>(context);
        var assignment = Entity<GroupStaffAssignment>(context);
        var availability = Entity<StaffTermAvailability>(context);
        var range = Entity<StaffAvailability>(context);
        var alert = Entity<ScheduleImpactAlert>(context);

        AssertUnique(offering, nameof(CourseOffering.TermId), nameof(CourseOffering.CourseId));
        AssertUnique(group, nameof(SectionGroup.OfferingId), nameof(SectionGroup.GroupCode));
        AssertUnique(room, nameof(Room.Code));
        AssertUnique(
            availability,
            nameof(StaffTermAvailability.StaffId),
            nameof(StaffTermAvailability.TermId));

        Assert.Equal(
            [nameof(GroupStaffAssignment.MeetingSlotId), nameof(GroupStaffAssignment.StaffId), nameof(GroupStaffAssignment.TeachingRole)],
            assignment.FindPrimaryKey()!.Properties.Select(property => property.Name));
        Assert.Contains(
            group.GetKeys(),
            key => key.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(SectionGroup.Id), nameof(SectionGroup.OfferingId)]));
        Assert.Contains(
            meeting.GetKeys(),
            key => key.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(MeetingSlot.Id), nameof(MeetingSlot.GroupId)]));

        AssertIndex(
            meeting,
            nameof(MeetingSlot.RoomId),
            nameof(MeetingSlot.DayOfWeek),
            nameof(MeetingSlot.StartLocal),
            nameof(MeetingSlot.EndLocal),
            nameof(MeetingSlot.GroupId));
        AssertIndex(
            assignment,
            nameof(GroupStaffAssignment.StaffId),
            nameof(GroupStaffAssignment.GroupId),
            nameof(GroupStaffAssignment.MeetingSlotId));
        AssertIndex(
            range,
            nameof(StaffAvailability.StaffTermAvailabilityId),
            nameof(StaffAvailability.DayOfWeek),
            nameof(StaffAvailability.StartLocal),
            nameof(StaffAvailability.EndLocal));
        AssertIndex(
            alert,
            nameof(ScheduleImpactAlert.State),
            nameof(ScheduleImpactAlert.DetectedAtUtc),
            nameof(ScheduleImpactAlert.Id));

        AssertForeignKey<CourseOffering, StudentRegistration.Academics.Domain.AcademicTerm>(
            offering,
            nameof(CourseOffering.TermId));
        AssertForeignKey<CourseOffering, StudentRegistration.Academics.Domain.Course>(
            offering,
            nameof(CourseOffering.CourseId));
        AssertForeignKey<SectionGroup, CourseOffering>(group, nameof(SectionGroup.OfferingId));
        AssertForeignKey<MeetingSlot, SectionGroup>(meeting, nameof(MeetingSlot.GroupId));
        AssertForeignKey<MeetingSlot, Room>(meeting, nameof(MeetingSlot.RoomId));
        AssertForeignKey<GroupStaffAssignment, SectionGroup>(
            assignment,
            nameof(GroupStaffAssignment.GroupId));
        AssertForeignKey<GroupStaffAssignment, MeetingSlot>(
            assignment,
            nameof(GroupStaffAssignment.MeetingSlotId),
            nameof(GroupStaffAssignment.GroupId));
        AssertForeignKey<GroupStaffAssignment, StudentRegistration.IdentityAccess.Domain.Staff>(
            assignment,
            nameof(GroupStaffAssignment.StaffId));
        AssertForeignKey<StaffTermAvailability, StudentRegistration.IdentityAccess.Domain.Staff>(
            availability,
            nameof(StaffTermAvailability.StaffId));
        AssertForeignKey<StaffTermAvailability, StudentRegistration.Academics.Domain.AcademicTerm>(
            availability,
            nameof(StaffTermAvailability.TermId));
        AssertForeignKey<StaffAvailability, StaffTermAvailability>(
            range,
            nameof(StaffAvailability.StaffTermAvailabilityId));
        AssertForeignKey<ScheduleImpactAlert, SectionGroup>(
            alert,
            nameof(ScheduleImpactAlert.GroupId));
        AssertForeignKey<ScheduleImpactAlert, Room>(
            alert,
            nameof(ScheduleImpactAlert.RoomId));
        AssertForeignKey<ScheduleImpactAlert, StaffTermAvailability>(
            alert,
            nameof(ScheduleImpactAlert.StaffTermAvailabilityId));
    }

    [Fact]
    public void Mutable_roots_use_rowversion_and_database_checks_bound_states_ranges_and_capacity()
    {
        using var context = CreateContext(ModelConnectionString);

        AssertRowVersion<CourseOffering>(context, nameof(CourseOffering.Version));
        AssertRowVersion<SectionGroup>(context, nameof(SectionGroup.Version));
        AssertRowVersion<Room>(context, nameof(Room.Version));
        AssertRowVersion<StaffTermAvailability>(
            context,
            nameof(StaffTermAvailability.Version));
        AssertRowVersion<ScheduleImpactAlert>(
            context,
            nameof(ScheduleImpactAlert.Version));

        var script = context.Database.GenerateCreateScript();
        foreach (var constraint in new[]
                 {
                     "CK_CourseOfferings_State",
                     "CK_SectionGroups_Capacity",
                     "CK_SectionGroups_State",
                     "CK_MeetingSlots_Range",
                     "CK_MeetingSlots_DayOfWeek",
                     "CK_MeetingSlots_ActivityType",
                     "CK_Rooms_Capacity",
                     "CK_Rooms_AvailabilityState",
                     "CK_GroupStaffAssignments_RoleActivity",
                     "CK_StaffAvailabilities_Range",
                     "CK_StaffAvailabilities_DayOfWeek",
                     "CK_StaffAvailabilities_Kind",
                     "CK_ScheduleImpactAlerts_State",
                     "CK_ScheduleImpactAlerts_Resource",
                 })
        {
            Assert.Contains(constraint, script, StringComparison.Ordinal);
        }
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Real_sql_enforces_capacity_resource_range_alert_and_rowversion_invariants()
    {
        await using var sqlServer = new SqlServerContainerFixture();
        await sqlServer.StartAsync();

        var connectionString = DatabaseConnection(sqlServer.ConnectionString);
        await using var context = CreateContext(connectionString);
        try
        {
            Assert.True(await context.Database.EnsureCreatedAsync());
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            await SeedUpstreamRowsAsync(connection);

            await ExecuteAsync(
                connection,
                """
                INSERT INTO [scheduling].[CourseOfferings]
                    ([Id], [TermId], [CourseId], [State])
                VALUES
                    ('10000000-0000-0000-0000-000000000001',
                     '20000000-0000-0000-0000-000000000001',
                     '30000000-0000-0000-0000-000000000001',
                     'draft');
                INSERT INTO [scheduling].[Rooms]
                    ([Id], [Code], [Location], [Capacity], [AvailabilityState])
                VALUES
                    ('40000000-0000-0000-0000-000000000001',
                     N'R101', N'Main Building', 35, 'available');
                INSERT INTO [scheduling].[SectionGroups]
                    ([Id], [OfferingId], [GroupCode], [Capacity], [EnrolledCount], [State], [RegistrationPaused])
                VALUES
                    ('50000000-0000-0000-0000-000000000001',
                     '10000000-0000-0000-0000-000000000001',
                     N'G01', 30, 30, 'published', 0);
                INSERT INTO [scheduling].[StaffTermAvailabilities]
                    ([Id], [StaffId], [TermId], [DeadlineUtc])
                VALUES
                    ('60000000-0000-0000-0000-000000000001',
                     '70000000-0000-0000-0000-000000000001',
                     '20000000-0000-0000-0000-000000000001',
                     '2026-08-01T00:00:00');
                """);

            await AssertSqlFailureAsync(
                connection,
                """
                UPDATE [scheduling].[SectionGroups]
                SET [Capacity] = 29
                WHERE [Id] = '50000000-0000-0000-0000-000000000001';
                """);
            await AssertSqlFailureAsync(
                connection,
                """
                INSERT INTO [scheduling].[Rooms]
                    ([Id], [Code], [Location], [Capacity], [AvailabilityState])
                VALUES
                    ('40000000-0000-0000-0000-000000000002',
                     N'R101', N'Other Building', 20, 'available');
                """);
            await AssertSqlFailureAsync(
                connection,
                """
                INSERT INTO [scheduling].[StaffAvailabilities]
                    ([Id], [StaffTermAvailabilityId], [DayOfWeek], [StartLocal], [EndLocal], [Kind])
                VALUES
                    ('80000000-0000-0000-0000-000000000001',
                     '60000000-0000-0000-0000-000000000001',
                     1, '12:00', '11:00', 'available');
                """);
            await AssertSqlFailureAsync(
                connection,
                """
                INSERT INTO [scheduling].[StaffTermAvailabilities]
                    ([Id], [StaffId], [TermId], [DeadlineUtc])
                VALUES
                    ('60000000-0000-0000-0000-000000000002',
                     '70000000-0000-0000-0000-000000000001',
                     '20000000-0000-0000-0000-000000000001',
                     '2026-08-02T00:00:00');
                """);
            await AssertSqlFailureAsync(
                connection,
                """
                INSERT INTO [scheduling].[ScheduleImpactAlerts]
                    ([Id], [GroupId], [StaffTermAvailabilityId], [RoomId], [ReasonCode],
                     [DetectedGroupVersion], [DetectedResourceVersion], [State],
                     [DetectedAtUtc], [LastRevalidationPassed])
                VALUES
                    ('90000000-0000-0000-0000-000000000001',
                     '50000000-0000-0000-0000-000000000001',
                     NULL, NULL, N'RESOURCE_CHANGED',
                     0x0000000000000001, 0x0000000000000001, 'open',
                     '2026-07-16T00:00:00', 0);
                """);
            await AssertSqlFailureAsync(
                connection,
                """
                INSERT INTO [scheduling].[ScheduleImpactAlerts]
                    ([Id], [GroupId], [StaffTermAvailabilityId], [RoomId], [ReasonCode],
                     [DetectedGroupVersion], [DetectedResourceVersion], [State],
                     [DetectedAtUtc], [LastRevalidationPassed])
                VALUES
                    ('90000000-0000-0000-0000-000000000002',
                     '50000000-0000-0000-0000-000000000001',
                     '60000000-0000-0000-0000-000000000001', NULL, N'RESOURCE_CHANGED',
                     0x0000000000000001, 0x0000000000000001, 'dismissed',
                     '2026-07-16T00:00:00', 0);
                """);

            Assert.Equal(
                5,
                await ExecuteCountAsync(
                    connection,
                    """
                    SELECT COUNT(*)
                    FROM sys.columns AS c
                    INNER JOIN sys.tables AS t ON t.object_id = c.object_id
                    INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id
                    WHERE s.name = N'scheduling'
                      AND c.system_type_id = 189
                      AND t.name IN
                      (
                          N'CourseOfferings',
                          N'SectionGroups',
                          N'Rooms',
                          N'StaffTermAvailabilities',
                          N'ScheduleImpactAlerts'
                      );
                    """));
            Assert.Equal(
                3,
                await ExecuteCountAsync(
                    connection,
                    """
                    SELECT COUNT(*)
                    FROM sys.indexes
                    WHERE name IN
                    (
                        N'IX_MeetingSlots_RoomId_DayOfWeek_StartLocal_EndLocal_GroupId',
                        N'IX_GroupStaffAssignments_StaffId_GroupId_MeetingSlotId',
                        N'IX_StaffAvailabilities_StaffTermAvailabilityId_DayOfWeek_StartLocal_EndLocal'
                    );
                    """));
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
        }
    }

    private static StudentRegistrationDbContext CreateContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new StudentRegistrationDbContext(options);
    }

    private static string DatabaseConnection(string serverConnectionString) =>
        new SqlConnectionStringBuilder(serverConnectionString)
        {
            InitialCatalog = $"StudentRegistration_Test_Spec010_{Guid.NewGuid():N}"
        }.ConnectionString;

    private static IEntityType Entity<TEntity>(StudentRegistrationDbContext context) =>
        context.Model.FindEntityType(typeof(TEntity))
        ?? throw new InvalidOperationException($"{typeof(TEntity).Name} is missing.");

    private static void AssertRowVersion<TEntity>(
        StudentRegistrationDbContext context,
        string propertyName)
    {
        var property = Entity<TEntity>(context).FindProperty(propertyName);
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
            foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(TPrincipal)
                && foreignKey.Properties.Select(property => property.Name).SequenceEqual(properties));

    private static async Task SeedUpstreamRowsAsync(SqlConnection connection)
    {
        await ExecuteAsync(
            connection,
            """
            INSERT INTO [auth].[ApplicationUsers]
                ([Id], [UserName], [NormalizedUserName], [UniversityId], [PasswordHash],
                 [SecurityStamp], [IsEnabled], [AccessFailedCount], [LockoutEndUtc])
            VALUES
                ('71000000-0000-0000-0000-000000000001',
                 N'lecturer@example.test', N'LECTURER@EXAMPLE.TEST', NULL,
                 N'not-a-real-hash', N'stamp', 1, 0, NULL);
            INSERT INTO [auth].[Staff]
                ([Id], [ApplicationUserId], [StaffNumber], [DisplayName], [IsActive])
            VALUES
                ('70000000-0000-0000-0000-000000000001',
                 '71000000-0000-0000-0000-000000000001',
                 N'S-001', N'Lecturer One', 1);
            INSERT INTO [academics].[AcademicTerms]
                ([Id], [Code], [CreationClientRequestId], [CreationPayloadHash],
                 [DisplayName], [TeachingStartsOn], [TeachingEndsOn], [TimeZoneId], [State])
            VALUES
                ('20000000-0000-0000-0000-000000000001',
                 N'2026-FALL', '22000000-0000-0000-0000-000000000001',
                 N'hash', N'Fall 2026', '2026-09-01', '2026-12-31',
                 N'Africa/Cairo', 'draft');
            INSERT INTO [academics].[CatalogueDrafts]
                ([Id], [ScopeCode], [BasedOnVersionId], [CanonicalContentHash],
                 [ContentJson], [ValidationSummaryJson], [State])
            VALUES
                ('31000000-0000-0000-0000-000000000001',
                 N'AI', NULL, N'hash', N'{}', N'{}', 'validated');
            INSERT INTO [academics].[CatalogueVersions]
                ([Id], [SourceDraftId], [SupersedesId], [ScopeCode], [VersionCode],
                 [SourceReference], [EffectiveFromUtc], [PublishedAtUtc], [PublishedBy], [State])
            VALUES
                ('32000000-0000-0000-0000-000000000001',
                 '31000000-0000-0000-0000-000000000001', NULL, N'AI', N'2026.1',
                 N'synthetic', '2026-07-01', '2026-07-01', N'admin', 'published');
            INSERT INTO [academics].[Courses]
                ([Id], [CatalogueVersionId], [Code], [Title], [Credits], [IsActive],
                 [ProvenanceSourceReference], [ProvenanceAccessedOn],
                 [ProvenanceSourceKind], [ProvenanceSyntheticFieldsJson])
            VALUES
                ('30000000-0000-0000-0000-000000000001',
                 '32000000-0000-0000-0000-000000000001',
                 N'CS101', N'Introduction', 3, 1,
                 N'synthetic', '2026-07-01', N'synthetic-demo', N'[]');
            """);
    }

    private static async Task AssertSqlFailureAsync(
        SqlConnection connection,
        string sql) =>
        await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync(connection, sql));

    private static async Task ExecuteAsync(SqlConnection connection, string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<int> ExecuteCountAsync(
        SqlConnection connection,
        string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }
}
