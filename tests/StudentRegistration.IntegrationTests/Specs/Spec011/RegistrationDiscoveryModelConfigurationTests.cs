using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Microsoft.Data.SqlClient;
using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.Api.Composition;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Infrastructure.SqlServer.Persistence.Configurations;
using StudentRegistration.Infrastructure.SqlServer.Registration;
using StudentRegistration.IntegrationTests.Infrastructure;
using StudentRegistration.Registration.Application.Ports;
using StudentRegistration.Scheduling.Application.Ports;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec011;

public sealed class RegistrationDiscoveryModelConfigurationTests
{
    private const string ModelConnectionString =
        "Server=localhost;Database=Spec011ModelOnly;User Id=sa;Password=NotUsed!42;TrustServerCertificate=True";

    [Fact]
    public void Discovery_projections_are_keyless_read_only_and_create_no_owned_tables()
    {
        using var context = CreateContext();

        AssertReadOnlyProjection<OfferingEligibilityReadModel>(
            context,
            "OfferingEligibility");
        AssertReadOnlyProjection<EligibilityReasonReadModel>(
            context,
            "EligibilityReason");
        AssertReadOnlyProjection<GroupSummaryReadModel>(
            context,
            "GroupSummary");

        var createScript = context.Database.GenerateCreateScript();
        Assert.DoesNotContain(
            "CREATE TABLE [registration].[OfferingEligibility]",
            createScript,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            "CREATE TABLE [registration].[EligibilityReason]",
            createScript,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            "CREATE TABLE [registration].[GroupSummary]",
            createScript,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            "CurrentPlan",
            createScript,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Adapter_contributes_all_narrow_read_ports_and_no_writer()
    {
        var services = new ServiceCollection();
        services.AddDbContext<StudentRegistrationDbContext>(options =>
            options.UseSqlServer(ModelConnectionString));
        services.AddStudentRegistrationDiscoverySqlServer();
        services.AddStudentRegistrationRegistrationModule();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var adapter = scope.ServiceProvider
            .GetRequiredService<RegistrationDiscoveryQueryAdapter>();
        Assert.Same(
            adapter,
            scope.ServiceProvider.GetRequiredService<IEligibilityAcademicReader>());
        Assert.Same(
            adapter,
            scope.ServiceProvider.GetRequiredService<IEligibilityOfferingReader>());
        Assert.Same(
            adapter,
            scope.ServiceProvider.GetRequiredService<ICurrentPlanReader>());
        Assert.DoesNotContain(
            adapter.GetType().GetInterfaces(),
            contract => contract.Name.Contains(
                "Store",
                StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Source_model_has_the_required_bounded_discovery_indexes()
    {
        using var context = CreateContext();

        AssertIndex<CourseOffering>(
            context,
            nameof(CourseOffering.TermId),
            nameof(CourseOffering.State),
            nameof(CourseOffering.Id));
        AssertIndex<SectionGroup>(
            context,
            nameof(SectionGroup.OfferingId),
            nameof(SectionGroup.State),
            nameof(SectionGroup.RegistrationPaused),
            nameof(SectionGroup.Id));
        AssertIndex<MeetingSlot>(
            context,
            nameof(MeetingSlot.GroupId),
            nameof(MeetingSlot.ActivityType),
            nameof(MeetingSlot.DayOfWeek),
            nameof(MeetingSlot.StartLocal),
            nameof(MeetingSlot.Id));
    }

    [Fact]
    public void Provider_predicates_are_parameterized_and_queries_are_no_tracking()
    {
        using var context = CreateContext();
        var adapter = new RegistrationDiscoveryQueryAdapter(context);
        var termId = Guid.Parse("01100000-0000-0000-0000-000000000003");
        var applicationUserId =
            Guid.Parse("01100000-0000-0000-0000-000000000001");

        var offeringSql = PrivateQuery<CourseOffering>(
            adapter,
            "OfferingsForTermQuery",
            termId).ToQueryString();
        var studentSql = PrivateQuery<StudentRegistration.Academics.Domain.Student>(
            adapter,
            "StudentForApplicationUserQuery",
            applicationUserId).ToQueryString();

        Assert.Contains("DECLARE @termId", offeringSql, StringComparison.Ordinal);
        Assert.Contains(
            "[c].[TermId] = @termId",
            offeringSql,
            StringComparison.Ordinal);
        Assert.Contains(
            "DECLARE @applicationUserId",
            studentSql,
            StringComparison.Ordinal);
        Assert.Contains(
            "[s].[ApplicationUserId] = @applicationUserId",
            studentSql,
            StringComparison.Ordinal);
        Assert.Equal(
            QueryTrackingBehavior.NoTracking,
            context.ChangeTracker.QueryTrackingBehavior);
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Real_sql_adapter_returns_complete_academic_and_scheduling_values()
    {
        await using var sqlServer = new SqlServerContainerFixture();
        await sqlServer.StartAsync();

        var connectionString = new SqlConnectionStringBuilder(
            sqlServer.ConnectionString)
        {
            InitialCatalog = $"StudentRegistration_Test_Spec011_{Guid.NewGuid():N}"
        }.ConnectionString;
        await using var context = CreateContext(connectionString);
        try
        {
            Assert.True(await context.Database.EnsureCreatedAsync());
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            await SeedDiscoveryGraphAsync(connection);

            var adapter = new RegistrationDiscoveryQueryAdapter(context);
            var applicationUserId =
                Guid.Parse("01100000-0000-0000-0000-000000000001");
            var termId = Guid.Parse("01100000-0000-0000-0000-000000000003");
            var courseId = Guid.Parse("01100000-0000-0000-0000-000000000004");

            var offerings = await adapter.ListForTermAsync(termId);
            var offering = Assert.Single(offerings);
            Assert.Equal("published", offering.State);
            Assert.NotEmpty(offering.RowVersion);
            var group = Assert.Single(offering.Groups);
            Assert.Equal(2, group.Meetings.Count);
            Assert.All(group.Meetings, meeting => Assert.Single(meeting.Staff));
            Assert.All(group.Meetings, meeting => Assert.True(meeting.RoomAvailable));

            var academic = await adapter.ReadAsync(
                applicationUserId,
                termId,
                [courseId],
                new DateTime(2026, 7, 16, 9, 0, 0, DateTimeKind.Utc));
            Assert.NotNull(academic);
            Assert.Equal("AI-DS", academic!.ProgramCode);
            Assert.True(academic.RegistrationWindow.IsOpen);
            Assert.Contains(
                academic.CurrentTranscriptLeaves,
                item => item.CourseCode == "DS312" && item.Status == "passed");
            var course = Assert.Single(academic.Catalogue!.Courses);
            Assert.Equal("DS413", course.Code);
            Assert.Equal(["DS312"], course.PrerequisiteCourseCodes);
            Assert.Equal(2m, course.MinimumGpa);
            Assert.Equal(96m, course.MinimumEarnedCredits);
            Assert.Equal("DEMO-POC-2026.1", academic.Policy!.Version);
            Assert.Equal(11, academic.Policy.Rules.Count);
            Assert.Empty(context.ChangeTracker.Entries());

            var plan = await adapter.ReadAsync(academic.StudentId, termId);
            Assert.Equal("initial-empty/1", plan.Version);
            Assert.Equal(0m, plan.Credits);
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
        }
    }

    private static StudentRegistrationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(ModelConnectionString)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .Options;
        return new StudentRegistrationDbContext(options);
    }

    private static StudentRegistrationDbContext CreateContext(
        string connectionString)
    {
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new StudentRegistrationDbContext(options);
    }

    private static async Task SeedDiscoveryGraphAsync(SqlConnection connection)
    {
        await ExecuteAsync(
            connection,
            """
            INSERT INTO [auth].[ApplicationUsers]
                ([Id], [UserName], [NormalizedUserName], [UniversityId], [PasswordHash],
                 [SecurityStamp], [IsEnabled], [AccessFailedCount], [LockoutEndUtc])
            VALUES
                ('01100000-0000-0000-0000-000000000001', N'student@test', N'STUDENT@TEST', N'S001', N'hash', N's1', 1, 0, NULL),
                ('01100000-0000-0000-0000-000000000020', N'lecturer@test', N'LECTURER@TEST', NULL, N'hash', N's2', 1, 0, NULL),
                ('01100000-0000-0000-0000-000000000021', N'ta@test', N'TA@TEST', NULL, N'hash', N's3', 1, 0, NULL);
            INSERT INTO [auth].[Staff]
                ([Id], [ApplicationUserId], [StaffNumber], [DisplayName], [IsActive])
            VALUES
                ('01100000-0000-0000-0000-000000000022', '01100000-0000-0000-0000-000000000020', N'L001', N'Dr. Ada', 1),
                ('01100000-0000-0000-0000-000000000023', '01100000-0000-0000-0000-000000000021', N'T001', N'Eng. Noor', 1);
            INSERT INTO [academics].[AcademicTerms]
                ([Id], [Code], [CreationClientRequestId], [CreationPayloadHash], [DisplayName],
                 [TeachingStartsOn], [TeachingEndsOn], [TimeZoneId], [State])
            VALUES
                ('01100000-0000-0000-0000-000000000003', N'2026-FALL', '01100000-0000-0000-0000-000000000030',
                 N'hash', N'Fall 2026', '2026-09-01', '2026-12-31', N'Africa/Cairo', 'registrationOpen');
            INSERT INTO [academics].[Students]
                ([Id], [ApplicationUserId], [ProgramCode], [Cohort], [CurrentGpa], [EarnedCredits],
                 [Standing], [IsActive], [Source], [SourceReference], [DataVersion], [DataAsOfUtc], [ImportedAtUtc])
            VALUES
                ('01100000-0000-0000-0000-000000000002', '01100000-0000-0000-0000-000000000001',
                 N'AI-DS', N'2023', 3.20, 96, N'Active', 1, N'demo', N'SRC-DATA-SCIENCE', N'1',
                 '2026-07-13T00:00:00', '2026-07-13T00:00:00');
            INSERT INTO [academics].[StudentTermAcademicStates]
                ([Id], [StudentId], [TermId], [GpaAtStart], [EarnedCreditsAtStart], [StandingAtStart],
                 [Source], [SourceReference], [DataVersion], [DataAsOfUtc])
            VALUES
                ('01100000-0000-0000-0000-000000000031', '01100000-0000-0000-0000-000000000002',
                 '01100000-0000-0000-0000-000000000003', 3.20, 96, N'Active', N'demo', N'SRC-DATA-SCIENCE', N'1', '2026-07-13T00:00:00');
            INSERT INTO [academics].[RegistrationWindows]
                ([Id], [TermId], [ScopeType], [ScopeValue], [OpensAtUtc], [ClosesAtUtc], [State])
            VALUES
                ('01100000-0000-0000-0000-000000000032', '01100000-0000-0000-0000-000000000003',
                 N'all-students', NULL, '2026-07-01T00:00:00', '2026-08-01T00:00:00', N'published');
            INSERT INTO [academics].[TranscriptAttempts]
                ([Id], [StudentId], [TermId], [SupersedesAttemptId], [CourseCode], [Credits], [GradeCode],
                 [Status], [Source], [SourceReference], [ImportedAtUtc])
            VALUES
                ('01100000-0000-0000-0000-000000000033', '01100000-0000-0000-0000-000000000002',
                 '01100000-0000-0000-0000-000000000003', NULL, N'DS312', 3, N'A', N'passed', N'demo', N'SRC-DATA-SCIENCE', '2026-07-13T00:00:00');
            """);

        await ExecuteAsync(
            connection,
            """
            INSERT INTO [academics].[CatalogueDrafts]
                ([Id], [ScopeCode], [BasedOnVersionId], [CanonicalContentHash], [ContentJson], [ValidationSummaryJson], [State])
            VALUES
                ('01100000-0000-0000-0000-000000000040', N'AI-DS', NULL, N'hash', N'{}', N'{}', N'published');
            INSERT INTO [academics].[CatalogueVersions]
                ([Id], [SourceDraftId], [SupersedesId], [ScopeCode], [VersionCode], [SourceReference],
                 [EffectiveFromUtc], [PublishedAtUtc], [PublishedBy], [State])
            VALUES
                ('01100000-0000-0000-0000-000000000041', '01100000-0000-0000-0000-000000000040', NULL,
                 N'AI-DS', N'CATALOGUE-2026.1', N'SRC-DATA-SCIENCE', '2026-07-13T00:00:00',
                 '2026-07-13T00:00:00', N'Ahmed ELbamby', N'published');
            INSERT INTO [academics].[Programs]
                ([Id], [CatalogueVersionId], [Code], [DisplayName], [IsActive], [ProvenanceSourceReference],
                 [ProvenanceAccessedOn], [ProvenanceSourceKind], [ProvenanceSyntheticFieldsJson])
            VALUES
                ('01100000-0000-0000-0000-000000000042', '01100000-0000-0000-0000-000000000041',
                 N'AI-DS', N'Data Science', 1, N'SRC-DATA-SCIENCE', '2026-07-13', N'official-source', N'[]');
            INSERT INTO [academics].[Courses]
                ([Id], [CatalogueVersionId], [Code], [Title], [Credits], [IsActive], [ProvenanceSourceReference],
                 [ProvenanceAccessedOn], [ProvenanceSourceKind], [ProvenanceSyntheticFieldsJson])
            VALUES
                ('01100000-0000-0000-0000-000000000004', '01100000-0000-0000-0000-000000000041',
                 N'DS413', N'Project I', 3, 1, N'SRC-DATA-SCIENCE', '2026-07-13', N'official-source', N'[]'),
                ('01100000-0000-0000-0000-000000000043', '01100000-0000-0000-0000-000000000041',
                 N'DS312', N'Applied Data Science', 3, 1, N'SRC-DATA-SCIENCE', '2026-07-13', N'official-source', N'[]');
            INSERT INTO [academics].[CurriculumCourses]
                ([CatalogueVersionId], [ProgramId], [CourseId], [Level], [RecommendedTerm], [IsRequired], [CohortScope],
                 [ProvenanceSourceReference], [ProvenanceAccessedOn], [ProvenanceSourceKind], [ProvenanceSyntheticFieldsJson])
            VALUES
                ('01100000-0000-0000-0000-000000000041', '01100000-0000-0000-0000-000000000042',
                 '01100000-0000-0000-0000-000000000004', 4, 7, 1, NULL, N'SRC-DATA-SCIENCE', '2026-07-13', N'official-source', N'[]');
            INSERT INTO [academics].[CoursePrerequisites]
                ([CatalogueVersionId], [CourseId], [RequiredCourseId], [MinimumGrade], [ProvenanceSourceReference],
                 [ProvenanceAccessedOn], [ProvenanceSourceKind], [ProvenanceSyntheticFieldsJson])
            VALUES
                ('01100000-0000-0000-0000-000000000041', '01100000-0000-0000-0000-000000000004',
                 '01100000-0000-0000-0000-000000000043', NULL, N'SRC-DATA-SCIENCE', '2026-07-13', N'official-source', N'[]');
            INSERT INTO [academics].[PolicySets]
                ([Id], [VersionCode], [TermId], [ProgramId], [ScopeCode], [EffectiveFromUtc], [EffectiveToUtc], [State])
            VALUES
                ('01100000-0000-0000-0000-000000000044', N'DEMO-POC-2026.1', '01100000-0000-0000-0000-000000000003',
                 '01100000-0000-0000-0000-000000000042', N'AI-DS', '2026-07-13T00:00:00', NULL, N'published');
            """);

        await ExecuteAsync(
            connection,
            """
            INSERT INTO [academics].[PolicyRules]
                ([Id], [PolicySetId], [Code], [ReasonCode], [ValueType], [Value], [SourceReference], [SourceKind])
            VALUES
                (NEWID(), '01100000-0000-0000-0000-000000000044', N'REGISTRATION_WINDOW_OPEN', N'REGISTRATION_WINDOW_CLOSED', N'boolean', N'true', N'DEMO-APPROVAL-2026.1', N'synthetic-demo'),
                (NEWID(), '01100000-0000-0000-0000-000000000044', N'ACADEMIC_STANDING_ALLOWED', N'ACADEMIC_STANDING_UNAVAILABLE', N'string-list', N'["Active"]', N'SRC-GENERAL-2016', N'official-source'),
                (NEWID(), '01100000-0000-0000-0000-000000000044', N'PREREQUISITES_REQUIRED', N'PREREQUISITE_NOT_COMPLETED', N'boolean', N'true', N'SRC-GENERAL-2016', N'official-source'),
                (NEWID(), '01100000-0000-0000-0000-000000000044', N'NORMAL_MAX_CREDITS', N'LOAD_ABOVE_NORMAL_MAXIMUM', N'number', N'18', N'SRC-GENERAL-2016', N'official-source'),
                (NEWID(), '01100000-0000-0000-0000-000000000044', N'PROBATION_MAX_CREDITS', N'PROBATION_LOAD_EXCEEDED', N'number', N'12', N'SRC-GENERAL-2016', N'official-source'),
                (NEWID(), '01100000-0000-0000-0000-000000000044', N'CAPACITY_REQUIRED', N'GROUP_FULL', N'boolean', N'true', N'DEMO-APPROVAL-2026.1', N'synthetic-demo'),
                (NEWID(), '01100000-0000-0000-0000-000000000044', N'CONFLICT_BLOCKED', N'MEETING_CONFLICT', N'boolean', N'true', N'DEMO-APPROVAL-2026.1', N'synthetic-demo'),
                (NEWID(), '01100000-0000-0000-0000-000000000044', N'DS413_MIN_GPA', N'MINIMUM_GPA_NOT_MET', N'number', N'2.0', N'SRC-DATA-SCIENCE', N'official-source'),
                (NEWID(), '01100000-0000-0000-0000-000000000044', N'DS413_MIN_EARNED_CREDITS', N'MINIMUM_EARNED_CREDITS_NOT_MET', N'number', N'96', N'SRC-DATA-SCIENCE', N'official-source');
            INSERT INTO [scheduling].[CourseOfferings] ([Id], [TermId], [CourseId], [State])
            VALUES ('01100000-0000-0000-0000-000000000005', '01100000-0000-0000-0000-000000000003',
                    '01100000-0000-0000-0000-000000000004', N'published');
            INSERT INTO [scheduling].[Rooms] ([Id], [Code], [Location], [Capacity], [AvailabilityState])
            VALUES
                ('01100000-0000-0000-0000-000000000050', N'R101', N'Main', 30, N'available'),
                ('01100000-0000-0000-0000-000000000051', N'L201', N'Labs', 30, N'available');
            INSERT INTO [scheduling].[SectionGroups]
                ([Id], [OfferingId], [GroupCode], [Capacity], [EnrolledCount], [State], [RegistrationPaused])
            VALUES ('01100000-0000-0000-0000-000000000006', '01100000-0000-0000-0000-000000000005',
                    N'G1', 30, 12, N'published', 0);
            INSERT INTO [scheduling].[MeetingSlots]
                ([Id], [GroupId], [RoomId], [ActivityType], [DayOfWeek], [StartLocal], [EndLocal])
            VALUES
                ('01100000-0000-0000-0000-000000000052', '01100000-0000-0000-0000-000000000006',
                 '01100000-0000-0000-0000-000000000050', N'lecture', 1, '09:00', '10:00'),
                ('01100000-0000-0000-0000-000000000053', '01100000-0000-0000-0000-000000000006',
                 '01100000-0000-0000-0000-000000000051', N'tutorial', 1, '10:00', '11:00');
            INSERT INTO [scheduling].[GroupStaffAssignments]
                ([GroupId], [MeetingSlotId], [ActivityType], [StaffId], [TeachingRole])
            VALUES
                ('01100000-0000-0000-0000-000000000006', '01100000-0000-0000-0000-000000000052',
                 N'lecture', '01100000-0000-0000-0000-000000000022', N'lecturer'),
                ('01100000-0000-0000-0000-000000000006', '01100000-0000-0000-0000-000000000053',
                 N'tutorial', '01100000-0000-0000-0000-000000000023', N'teaching-assistant');
            """);
    }

    private static async Task ExecuteAsync(SqlConnection connection, string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }

    private static void AssertReadOnlyProjection<T>(
        StudentRegistrationDbContext context,
        string expectedView)
        where T : class
    {
        var entity = context.Model.FindEntityType(typeof(T));
        Assert.NotNull(entity);
        Assert.Null(entity!.FindPrimaryKey());
        Assert.Equal(expectedView, entity.GetViewName());
        Assert.Equal("registration", entity.GetViewSchema());
        Assert.Null(entity.GetTableName());
    }

    private static void AssertIndex<T>(
        StudentRegistrationDbContext context,
        params string[] properties)
        where T : class
    {
        var entity = context.Model.FindEntityType(typeof(T));
        Assert.NotNull(entity);
        Assert.Contains(
            entity!.GetIndexes(),
            index => index.Properties.Select(property => property.Name)
                .SequenceEqual(properties));
    }

    private static IQueryable<T> PrivateQuery<T>(
        RegistrationDiscoveryQueryAdapter adapter,
        string methodName,
        Guid argument)
        where T : class
    {
        var method = typeof(RegistrationDiscoveryQueryAdapter).GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);
        return Assert.IsAssignableFrom<IQueryable<T>>(
            method!.Invoke(adapter, [argument]));
    }
}
