using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.Academics.Domain;
using StudentRegistration.Api.Composition;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Academics;
using StudentRegistration.Contracts.Auditing;
using StudentRegistration.IdentityAccess.Domain;
using StudentRegistration.Infrastructure.SqlServer.Audit;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.IntegrationTests.Infrastructure;

namespace StudentRegistration.IntegrationTests.Specs.Spec008;

public sealed class AcademicContextModelConfigurationTests
{
    private const string ModelConnectionString =
        "Server=localhost;Database=Spec008ModelOnly;User Id=sa;Password=NotUsed!42;TrustServerCertificate=True";

    private static readonly Type[] AcademicStorePorts =
    [
        typeof(IAcademicContextReader),
        typeof(IAcademicStudentScopeReader),
        typeof(IAdminAcademicStore),
        typeof(IRegistrationWindowStore),
        typeof(IStudentAcademicProfileStore),
        typeof(IDemoStudentProfileSeedStore)
    ];

    [Fact]
    public void Academic_mapping_contributes_only_the_six_owned_entities_and_relational_invariants()
    {
        using var context = CreateContext(ModelConnectionString);

        var term = RequiredEntity<AcademicTerm>(context);
        var window = RequiredEntity<RegistrationWindow>(context);
        var student = RequiredEntity<Student>(context);
        var attempt = RequiredEntity<TranscriptAttempt>(context);
        var hold = RequiredEntity<StudentHold>(context);
        var state = RequiredEntity<StudentTermAcademicState>(context);
        var entities = new[] { term, window, student, attempt, hold, state };

        Assert.Equal(
            new[]
            {
                typeof(AcademicTerm),
                typeof(RegistrationWindow),
                typeof(Student),
                typeof(StudentHold),
                typeof(StudentTermAcademicState),
                typeof(TranscriptAttempt)
            },
            context.Model.GetEntityTypes()
                .Where(entity => entities.Contains(entity))
                .Select(entity => entity.ClrType)
                .OrderBy(type => type.FullName, StringComparer.Ordinal));

        Assert.All(entities, entity => Assert.Equal("academics", entity.GetSchema()));
        Assert.Equal("AcademicTerms", term.GetTableName());
        Assert.Equal("RegistrationWindows", window.GetTableName());
        Assert.Equal("Students", student.GetTableName());
        Assert.Equal("TranscriptAttempts", attempt.GetTableName());
        Assert.Equal("StudentHolds", hold.GetTableName());
        Assert.Equal("StudentTermAcademicStates", state.GetTableName());

        AssertUniqueIndex(term, nameof(AcademicTerm.Code));
        AssertUniqueIndex(term, nameof(AcademicTerm.CreationClientRequestId));
        AssertUniqueIndex(student, nameof(Student.ApplicationUserId));
        AssertUniqueIndex(
            state,
            nameof(StudentTermAcademicState.StudentId),
            nameof(StudentTermAcademicState.TermId));
        AssertUniqueIndex(attempt, nameof(TranscriptAttempt.SupersedesAttemptId));
        Assert.Contains(
            "IS NOT NULL",
            FindIndex(attempt, nameof(TranscriptAttempt.SupersedesAttemptId)).GetFilter(),
            StringComparison.OrdinalIgnoreCase);
        AssertFilteredUniqueIndex(term, nameof(AcademicTerm.State), "registrationOpen");
        AssertFilteredUniqueIndex(term, nameof(AcademicTerm.State), "teaching");
        AssertIndex(
            window,
            nameof(RegistrationWindow.TermId),
            nameof(RegistrationWindow.State),
            nameof(RegistrationWindow.OpensAtUtc),
            nameof(RegistrationWindow.ClosesAtUtc));
        AssertIndex(
            attempt,
            nameof(TranscriptAttempt.StudentId),
            nameof(TranscriptAttempt.CourseCode));
        AssertIndex(
            hold,
            nameof(StudentHold.StudentId),
            nameof(StudentHold.TermId),
            nameof(StudentHold.EffectiveFromUtc),
            nameof(StudentHold.EffectiveToUtc));

        AssertRowVersion<AcademicTerm>(term, nameof(AcademicTerm.Version));
        AssertRowVersion<RegistrationWindow>(window, nameof(RegistrationWindow.Version));
        AssertRowVersion<Student>(student, nameof(Student.Version));
        AssertRowVersion<StudentTermAcademicState>(
            state,
            nameof(StudentTermAcademicState.Version));

        AssertForeignKey<RegistrationWindow, AcademicTerm>(
            window,
            nameof(RegistrationWindow.TermId));
        AssertForeignKey<Student, ApplicationUser>(student, nameof(Student.ApplicationUserId));
        AssertForeignKey<TranscriptAttempt, Student>(
            attempt,
            nameof(TranscriptAttempt.StudentId));
        AssertForeignKey<TranscriptAttempt, AcademicTerm>(
            attempt,
            nameof(TranscriptAttempt.TermId));
        AssertForeignKey<TranscriptAttempt, TranscriptAttempt>(
            attempt,
            nameof(TranscriptAttempt.SupersedesAttemptId));
        AssertForeignKey<StudentHold, Student>(hold, nameof(StudentHold.StudentId));
        AssertForeignKey<StudentHold, AcademicTerm>(hold, nameof(StudentHold.TermId));
        AssertForeignKey<StudentTermAcademicState, Student>(
            state,
            nameof(StudentTermAcademicState.StudentId));
        AssertForeignKey<StudentTermAcademicState, AcademicTerm>(
            state,
            nameof(StudentTermAcademicState.TermId));
        Assert.Equal(
            new[]
            {
                "RegistrationWindow.TermId->AcademicTerm",
                "Student.ApplicationUserId->ApplicationUser",
                "StudentHold.StudentId->Student",
                "StudentHold.TermId->AcademicTerm",
                "StudentTermAcademicState.StudentId->Student",
                "StudentTermAcademicState.TermId->AcademicTerm",
                "TranscriptAttempt.StudentId->Student",
                "TranscriptAttempt.SupersedesAttemptId->TranscriptAttempt",
                "TranscriptAttempt.TermId->AcademicTerm"
            },
            entities
                .SelectMany(entity => entity.GetForeignKeys())
                .Select(foreignKey =>
                    $"{foreignKey.DeclaringEntityType.ClrType.Name}." +
                    $"{string.Join("+", foreignKey.Properties.Select(property => property.Name))}" +
                    $"->{foreignKey.PrincipalEntityType.ClrType.Name}")
                .Order(StringComparer.Ordinal));

        AssertDateTime2(window, nameof(RegistrationWindow.OpensAtUtc));
        AssertDateTime2(window, nameof(RegistrationWindow.ClosesAtUtc));
        AssertDateTime2(student, nameof(Student.DataAsOfUtc));
        AssertDateTime2(student, nameof(Student.ImportedAtUtc));
        AssertDateTime2(attempt, nameof(TranscriptAttempt.ImportedAtUtc));
        AssertDateTime2(hold, nameof(StudentHold.EffectiveFromUtc));
        AssertDateTime2(hold, nameof(StudentHold.EffectiveToUtc));
        AssertDateTime2(hold, nameof(StudentHold.ImportedAtUtc));
        AssertDateTime2(state, nameof(StudentTermAcademicState.DataAsOfUtc));
        Assert.Equal("date", term.FindProperty(nameof(AcademicTerm.TeachingStartsOn))!.GetColumnType());
        Assert.Equal("date", term.FindProperty(nameof(AcademicTerm.TeachingEndsOn))!.GetColumnType());

        AssertEnumStoredAsString(term, nameof(AcademicTerm.State), 30);
        AssertEnumStoredAsString(window, nameof(RegistrationWindow.ScopeType), 30);
        AssertEnumStoredAsString(window, nameof(RegistrationWindow.State), 30);
        AssertEnumStoredAsString(attempt, nameof(TranscriptAttempt.Status), 30);
        AssertDecimal(student, nameof(Student.CurrentGpa), precision: 4, scale: 2);
        AssertDecimal(student, nameof(Student.EarnedCredits), precision: 6, scale: 2);
        AssertDecimal(attempt, nameof(TranscriptAttempt.Credits), precision: 6, scale: 2);
        AssertDecimal(state, nameof(StudentTermAcademicState.GpaAtStart), precision: 4, scale: 2);
        AssertDecimal(state, nameof(StudentTermAcademicState.EarnedCreditsAtStart), precision: 6, scale: 2);

        AssertCheckConstraintContains(context, term, "TeachingEndsOn", ">", "TeachingStartsOn");
        AssertCheckConstraintsContainAll(
            context,
            term,
            "draft",
            "registrationOpen",
            "registrationClosed",
            "teaching",
            "completed",
            "archived");
        AssertCheckConstraintContains(context, window, "ClosesAtUtc", ">", "OpensAtUtc");
        AssertCheckConstraintsContainAll(
            context,
            window,
            "all-students",
            "program",
            "cohort",
            "ScopeValue",
            "IS NULL");
        AssertCheckConstraintsContainAll(
            context,
            window,
            "draft",
            "published",
            "emergencyClosed",
            "superseded");
        AssertCheckConstraintContains(context, student, "CurrentGpa", "0", "4");
        AssertCheckConstraintContains(context, student, "EarnedCredits", ">=", "0");
        AssertCheckConstraintContains(context, attempt, "Credits", ">", "0");
        AssertCheckConstraintContains(context, attempt, "SupersedesAttemptId", "Id", "<>");
        AssertCheckConstraintsContainAll(
            context,
            attempt,
            "in-progress",
            "passed",
            "failed",
            "withdrawn");
        AssertCheckConstraintContains(context, hold, "EffectiveToUtc", "EffectiveFromUtc", ">");
        AssertCheckConstraintContains(context, state, "GpaAtStart", "0", "4");
        AssertCheckConstraintContains(context, state, "EarnedCreditsAtStart", ">=", "0");
    }

    [Fact]
    public void Academic_source_references_are_bounded_scalars_without_downstream_catalogue_keys()
    {
        using var context = CreateContext(ModelConnectionString);
        var term = RequiredEntity<AcademicTerm>(context);
        var window = RequiredEntity<RegistrationWindow>(context);
        var student = RequiredEntity<Student>(context);
        var attempt = RequiredEntity<TranscriptAttempt>(context);
        var hold = RequiredEntity<StudentHold>(context);
        var state = RequiredEntity<StudentTermAcademicState>(context);

        AssertBoundedRequired(term, nameof(AcademicTerm.Code), 50);
        AssertBoundedRequired(term, nameof(AcademicTerm.CreationPayloadHash), 128);
        AssertBoundedRequired(term, nameof(AcademicTerm.DisplayName), 200);
        AssertBoundedRequired(term, nameof(AcademicTerm.TimeZoneId), 100);
        AssertBoundedRequired(window, nameof(RegistrationWindow.ScopeType), 30);
        AssertBoundedOptional(window, nameof(RegistrationWindow.ScopeValue), 100);
        AssertBoundedRequired(window, nameof(RegistrationWindow.State), 30);
        AssertBoundedRequired(student, nameof(Student.ProgramCode), 50);
        AssertBoundedRequired(student, nameof(Student.Cohort), 50);
        AssertBoundedRequired(student, nameof(Student.Standing), 100);
        AssertBoundedRequired(student, nameof(Student.Source), 200);
        AssertBoundedRequired(student, nameof(Student.SourceReference), 200);
        AssertBoundedRequired(student, nameof(Student.DataVersion), 100);
        AssertBoundedRequired(attempt, nameof(TranscriptAttempt.CourseCode), 50);
        AssertBoundedOptional(attempt, nameof(TranscriptAttempt.GradeCode), 50);
        AssertBoundedRequired(attempt, nameof(TranscriptAttempt.Status), 30);
        AssertBoundedRequired(attempt, nameof(TranscriptAttempt.Source), 200);
        AssertBoundedRequired(attempt, nameof(TranscriptAttempt.SourceReference), 200);
        AssertBoundedRequired(hold, nameof(StudentHold.Code), 100);
        AssertBoundedRequired(hold, nameof(StudentHold.Message), 500);
        AssertBoundedRequired(hold, nameof(StudentHold.Source), 200);
        AssertBoundedRequired(hold, nameof(StudentHold.SourceReference), 200);
        AssertBoundedRequired(state, nameof(StudentTermAcademicState.StandingAtStart), 100);
        AssertBoundedRequired(state, nameof(StudentTermAcademicState.Source), 200);
        AssertBoundedRequired(state, nameof(StudentTermAcademicState.SourceReference), 200);
        AssertBoundedRequired(state, nameof(StudentTermAcademicState.DataVersion), 100);

        AssertRequired(term, nameof(AcademicTerm.CreationClientRequestId));
        AssertRequired(term, nameof(AcademicTerm.TeachingStartsOn));
        AssertRequired(term, nameof(AcademicTerm.TeachingEndsOn));
        AssertRequired(window, nameof(RegistrationWindow.TermId));
        AssertRequired(window, nameof(RegistrationWindow.OpensAtUtc));
        AssertRequired(window, nameof(RegistrationWindow.ClosesAtUtc));
        AssertRequired(student, nameof(Student.ApplicationUserId));
        AssertRequired(student, nameof(Student.CurrentGpa));
        AssertRequired(student, nameof(Student.EarnedCredits));
        AssertRequired(student, nameof(Student.IsActive));
        AssertRequired(student, nameof(Student.DataAsOfUtc));
        AssertRequired(student, nameof(Student.ImportedAtUtc));
        AssertRequired(attempt, nameof(TranscriptAttempt.StudentId));
        AssertRequired(attempt, nameof(TranscriptAttempt.TermId));
        AssertRequired(attempt, nameof(TranscriptAttempt.Credits));
        AssertRequired(attempt, nameof(TranscriptAttempt.ImportedAtUtc));
        AssertRequired(hold, nameof(StudentHold.StudentId));
        AssertRequired(hold, nameof(StudentHold.TermId));
        AssertRequired(hold, nameof(StudentHold.BlocksRegistration));
        AssertRequired(hold, nameof(StudentHold.EffectiveFromUtc));
        AssertRequired(hold, nameof(StudentHold.ImportedAtUtc));
        AssertRequired(state, nameof(StudentTermAcademicState.StudentId));
        AssertRequired(state, nameof(StudentTermAcademicState.TermId));
        AssertRequired(state, nameof(StudentTermAcademicState.GpaAtStart));
        AssertRequired(state, nameof(StudentTermAcademicState.EarnedCreditsAtStart));
        AssertRequired(state, nameof(StudentTermAcademicState.DataAsOfUtc));

        Assert.DoesNotContain(
            student.GetForeignKeys().SelectMany(foreignKey => foreignKey.Properties),
            property => property.Name == nameof(Student.ProgramCode));
        Assert.DoesNotContain(
            attempt.GetForeignKeys().SelectMany(foreignKey => foreignKey.Properties),
            property => property.Name == nameof(TranscriptAttempt.CourseCode));
        Assert.All(
            new[] { student, attempt, hold, state }.SelectMany(entity => entity.GetForeignKeys()),
            foreignKey => Assert.DoesNotContain(
                "Catalogue",
                foreignKey.PrincipalEntityType.ClrType.Namespace ?? string.Empty,
                StringComparison.Ordinal));

        Assert.All(
            typeof(TranscriptAttempt).GetProperties(),
            property => Assert.Null(property.SetMethod));
        Assert.All(
            typeof(StudentHold).GetProperties(),
            property => Assert.Null(property.SetMethod));
        Assert.Equal(
            ValueGenerated.Never,
            attempt.FindProperty(nameof(TranscriptAttempt.Id))!.ValueGenerated);
        Assert.Equal(
            ValueGenerated.Never,
            hold.FindProperty(nameof(StudentHold.Id))!.ValueGenerated);
    }

    [Fact]
    public void Application_composition_has_no_sql_adapter_and_sql_composition_resolves_one_scoped_store()
    {
        var applicationServices = new ServiceCollection();
        applicationServices.AddStudentRegistrationAcademicModule();
        Assert.DoesNotContain(
            applicationServices,
            descriptor => AcademicStorePorts.Contains(descriptor.ServiceType));

        var sqlServices = new ServiceCollection();
        sqlServices.AddSingleton(TimeProvider.System);
        sqlServices.AddStudentRegistrationSqlServer(Configuration(ModelConnectionString));
        sqlServices.AddStudentRegistrationRegistrationModule();
        sqlServices.AddScoped<IAuditEventWriter, AuditTransactionWriter>();

        var descriptors = AcademicStorePorts
            .Select(port => Assert.Single(
                sqlServices,
                descriptor => descriptor.ServiceType == port))
            .ToArray();
        Assert.All(
            descriptors,
            descriptor => Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime));

        using var provider = sqlServices.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateScopes = true,
                ValidateOnBuild = true
            });
        using var firstScope = provider.CreateScope();
        var firstInstances = AcademicStorePorts
            .Select(port => firstScope.ServiceProvider.GetRequiredService(port))
            .ToArray();
        var first = Assert.Single(firstInstances.Distinct(ReferenceEqualityComparer.Instance));
        var academicStore = first.GetType();
        Assert.Equal(
            "StudentRegistration.Infrastructure.SqlServer.Persistence.AcademicStore",
            academicStore.FullName);
        Assert.All(AcademicStorePorts, port => Assert.True(port.IsAssignableFrom(academicStore)));

        using var secondScope = provider.CreateScope();
        var secondInstances = AcademicStorePorts
            .Select(port => secondScope.ServiceProvider.GetRequiredService(port))
            .ToArray();
        var second = Assert.Single(secondInstances.Distinct(ReferenceEqualityComparer.Instance));
        Assert.NotSame(first, second);
        Assert.Single(
            sqlServices,
            descriptor => descriptor.ServiceType == typeof(StudentRegistrationDbContext));

        var infrastructure = typeof(StudentRegistrationDbContext).Assembly;
        Assert.Single(
            infrastructure.GetTypes(),
            type => !type.IsAbstract && typeof(DbContext).IsAssignableFrom(type));
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Real_sql_enforces_replay_identity_transcript_history_and_atomic_audit()
    {
        await using var sqlServer = new SqlServerContainerFixture();
        await sqlServer.StartAsync();

        var databaseName = $"StudentRegistration_Test_Spec008_{Guid.NewGuid():N}";
        var connectionString = new SqlConnectionStringBuilder(sqlServer.ConnectionString)
        {
            InitialCatalog = databaseName
        }.ConnectionString;

        await using var provider = CreateSqlProvider(connectionString);
        try
        {
            await using (var setupScope = provider.CreateAsyncScope())
            {
                var context = setupScope.ServiceProvider
                    .GetRequiredService<StudentRegistrationDbContext>();
                await context.Database.MigrateAsync();
                Assert.Contains(
                    "20260713010000_IdentityAcademicFoundation",
                    await context.Database.GetAppliedMigrationsAsync());
            }

            var requestId = Guid.NewGuid();
            var originalTermId = Guid.NewGuid();
            var originalWindowId = Guid.NewGuid();
            var original = CreateTermCommand(
                originalTermId,
                originalWindowId,
                requestId,
                "SHA256:SPEC008-PAYLOAD-A",
                "2026-FALL");
            var replay = CreateTermCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                requestId,
                "SHA256:SPEC008-PAYLOAD-A",
                "2026-FALL");
            var mismatch = CreateTermCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                requestId,
                "SHA256:SPEC008-PAYLOAD-B",
                "2026-FALL-REUSED");

            await using (var commandScope = provider.CreateAsyncScope())
            {
                var store = commandScope.ServiceProvider
                    .GetRequiredService<IRegistrationWindowStore>();
                var created = await store.CreateOrReplayTermAsync(original);
                var replayed = await store.CreateOrReplayTermAsync(replay);
                var rejected = await store.CreateOrReplayTermAsync(mismatch);

                Assert.Equal(AcademicTermCreationOutcome.Created, created.Outcome);
                Assert.Equal(AcademicTermCreationOutcome.Replayed, replayed.Outcome);
                Assert.Equal(created.Term?.Id, replayed.Term?.Id);
                Assert.Equal(originalTermId.ToString("D"), created.Term?.Id);
                Assert.Equal(
                    AcademicTermCreationOutcome.IdempotencyKeyReused,
                    rejected.Outcome);
                Assert.Null(rejected.Term);
            }

            await using (var evidenceScope = provider.CreateAsyncScope())
            {
                var context = evidenceScope.ServiceProvider
                    .GetRequiredService<StudentRegistrationDbContext>();
                Assert.Equal(1, await context.Set<AcademicTerm>().CountAsync());
                Assert.Equal(1, await context.Set<RegistrationWindow>().CountAsync());
                Assert.Equal(1, await context.AuditEvents.CountAsync());
                var persisted = await context.Set<AcademicTerm>().SingleAsync();
                Assert.Equal(requestId, persisted.CreationClientRequestId);
                Assert.Equal("SHA256:SPEC008-PAYLOAD-A", persisted.CreationPayloadHash);

                context.Add(new AcademicTerm(
                    Guid.NewGuid(),
                    "2027-SPRING",
                    requestId,
                    "SHA256:OTHER",
                    "Spring 2027",
                    new DateOnly(2027, 2, 1),
                    new DateOnly(2027, 6, 1),
                    "Africa/Cairo",
                    TermState.Draft));
                await AssertSqlUniqueViolationAsync(() => context.SaveChangesAsync());
                context.ChangeTracker.Clear();

                await ProveStudentAndTranscriptUniquenessAsync(
                    context,
                    originalTermId);
            }

            await using var faultProvider = CreateSqlProvider(
                connectionString,
                failAudit: true);
            await using (var faultScope = faultProvider.CreateAsyncScope())
            {
                var store = faultScope.ServiceProvider
                    .GetRequiredService<IRegistrationWindowStore>();
                var failed = await store.CreateOrReplayTermAsync(CreateTermCommand(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    "SHA256:FAULTED",
                    "2027-SUMMER"));
                Assert.Equal(AcademicTermCreationOutcome.StorageUnavailable, failed.Outcome);
            }

            await using (var finalScope = provider.CreateAsyncScope())
            {
                var context = finalScope.ServiceProvider
                    .GetRequiredService<StudentRegistrationDbContext>();
                Assert.Equal(1, await context.Set<AcademicTerm>().CountAsync());
                Assert.Equal(1, await context.Set<RegistrationWindow>().CountAsync());
                Assert.Equal(1, await context.AuditEvents.CountAsync());
            }
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

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Real_sql_ports_enforce_versions_overlap_bounded_profiles_registration_and_seed_replay()
    {
        await using var sqlServer = new SqlServerContainerFixture();
        await sqlServer.StartAsync();

        var connectionString = new SqlConnectionStringBuilder(sqlServer.ConnectionString)
        {
            InitialCatalog = $"StudentRegistration_Test_Spec008_Ports_{Guid.NewGuid():N}"
        }.ConnectionString;
        await using var provider = CreateSqlProvider(connectionString);

        try
        {
            await using (var setupScope = provider.CreateAsyncScope())
            {
                var context = setupScope.ServiceProvider
                    .GetRequiredService<StudentRegistrationDbContext>();
                await context.Database.MigrateAsync();
                Assert.Contains(
                    "20260713010000_IdentityAcademicFoundation",
                    await context.Database.GetAppliedMigrationsAsync());
            }

            var nowUtc = new DateTime(2026, 8, 5, 10, 0, 0, DateTimeKind.Utc);
            var termId = Guid.NewGuid();
            var firstWindowId = Guid.NewGuid();
            var secondWindowId = Guid.NewGuid();
            var firstWindow = Window(
                firstWindowId,
                termId,
                nowUtc.AddDays(-1),
                nowUtc.AddDays(5));
            var secondWindow = Window(
                secondWindowId,
                termId,
                nowUtc.AddDays(1),
                nowUtc.AddDays(7),
                RegistrationWindowScopeType.Program,
                "AI");

            AdminTermDto createdTerm;
            await using (var termScope = provider.CreateAsyncScope())
            {
                var store = termScope.ServiceProvider
                    .GetRequiredService<IRegistrationWindowStore>();
                var created = await store.CreateOrReplayTermAsync(CreateTermCommand(
                    termId,
                    Guid.NewGuid(),
                    "SHA256:SPEC008-PORT-BEHAVIOR",
                    "2026-FALL-PORTS",
                    firstWindow,
                    secondWindow));
                Assert.Equal(AcademicTermCreationOutcome.Created, created.Outcome);
                Assert.NotNull(created.Term);
                createdTerm = created.Term!;

                var originalTermVersion = DecodeVersion(createdTerm.RowVersion);
                var originalWindowVersions = createdTerm.Windows.ToDictionary(
                    window => Guid.Parse(window.Id),
                    window => DecodeVersion(window.RowVersion));
                var stale = await store.UpdateTermAsync(new UpdateAcademicTermStoreCommand(
                    termId,
                    new byte[8],
                    originalWindowVersions,
                    ToTermInput(createdTerm),
                    createdTerm.Windows.Select(ToTermWindowInput).ToArray(),
                    Audit("academic-term-updated", nameof(AcademicTerm), termId, nowUtc)));
                Assert.Equal(AcademicTermMutationOutcome.StaleVersion, stale.Outcome);
                Assert.Equal(createdTerm.RowVersion, stale.CurrentVersion);
                Assert.Null(stale.Term);

                var published = await store.PublishRegistrationWindowAsync(
                    new PublishRegistrationWindowStoreCommand(
                        termId,
                        firstWindowId,
                        originalTermVersion,
                        originalWindowVersions[firstWindowId],
                        Audit(
                            "registration-window-published",
                            nameof(RegistrationWindow),
                            firstWindowId,
                            nowUtc)));
                Assert.Equal(AcademicTermMutationOutcome.Succeeded, published.Outcome);
                Assert.NotNull(published.Term);
                var current = published.Term!;
                Assert.Equal(
                    RegistrationWindowLifecycle.Published,
                    Assert.Single(current.Windows, window => window.Id == firstWindowId.ToString("D"))
                        .LifecycleState);

                var stalePublish = await store.PublishRegistrationWindowAsync(
                    new PublishRegistrationWindowStoreCommand(
                        termId,
                        secondWindowId,
                        originalTermVersion,
                        originalWindowVersions[secondWindowId],
                        Audit(
                            "registration-window-published",
                            nameof(RegistrationWindow),
                            secondWindowId,
                            nowUtc)));
                Assert.Equal(AcademicTermMutationOutcome.StaleVersion, stalePublish.Outcome);
                Assert.Null(stalePublish.Term);

                var overlap = await store.PublishRegistrationWindowAsync(
                    new PublishRegistrationWindowStoreCommand(
                        termId,
                        secondWindowId,
                        DecodeVersion(current.RowVersion),
                        DecodeVersion(Assert.Single(
                            current.Windows,
                            window => window.Id == secondWindowId.ToString("D")).RowVersion),
                        Audit(
                            "registration-window-published",
                            nameof(RegistrationWindow),
                            secondWindowId,
                            nowUtc)));
                Assert.Equal(AcademicTermMutationOutcome.WindowOverlap, overlap.Outcome);
                Assert.Null(overlap.Term);
            }

            var primaryUserId = Guid.NewGuid();
            var overflowUserId = Guid.NewGuid();
            const string primaryUniversityId = "AI2600083";
            const string overflowUniversityId = "AI2600184";
            await using (var identityScope = provider.CreateAsyncScope())
            {
                var context = identityScope.ServiceProvider
                    .GetRequiredService<StudentRegistrationDbContext>();
                context.AddRange(
                    CreateApplicationUser(primaryUserId, primaryUniversityId),
                    CreateApplicationUser(overflowUserId, overflowUniversityId));
                await context.SaveChangesAsync();
            }

            var seedCommand = DemoSeed(
                primaryUserId,
                primaryUniversityId,
                termId,
                nowUtc,
                transcriptCount: 25,
                holdCount: 3,
                blockingFirstHold: true);
            await using (var seedScope = provider.CreateAsyncScope())
            {
                var seedStore = seedScope.ServiceProvider
                    .GetRequiredService<IDemoStudentProfileSeedStore>();
                var created = await seedStore.ReconcileAsync(seedCommand);
                var replayed = await seedStore.ReconcileAsync(seedCommand);

                Assert.True(created.Created);
                Assert.False(replayed.Created);
                Assert.Equal(seedCommand.Student.Id, created.StudentId);
                Assert.Equal(created.StudentId, replayed.StudentId);

                var mismatched = DemoSeed(
                    primaryUserId,
                    primaryUniversityId,
                    termId,
                    nowUtc,
                    transcriptCount: 25,
                    holdCount: 3,
                    blockingFirstHold: true,
                    currentGpa: 1.25m);
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => seedStore.ReconcileAsync(mismatched));
            }

            var readRequest = new AcademicProfileReadRequest(
                TranscriptPage: 2,
                TranscriptPageSize: 10,
                ProvenancePage: 2,
                ProvenancePageSize: 10,
                ActiveHoldTake: 100,
                ServerNowUtc: nowUtc);
            AcademicProfileSnapshot profile;
            await using (var profileScope = provider.CreateAsyncScope())
            {
                var store = profileScope.ServiceProvider
                    .GetRequiredService<IStudentAcademicProfileStore>();
                var read = await store.ReadByApplicationUserIdAsync(
                    primaryUserId,
                    readRequest);
                Assert.Equal(AcademicProfileStoreOutcome.Succeeded, read.Outcome);
                Assert.NotNull(read.Profile);
                profile = read.Profile!;
                Assert.Equal(10, profile.TranscriptAttempts.Items.Count);
                Assert.Equal(25, profile.TranscriptAttempts.TotalCount);
                Assert.Equal(3, profile.ActiveHolds.Count);
                Assert.Contains(profile.ActiveHolds, hold => hold.BlocksRegistration);
                Assert.InRange(profile.Provenance.Items.Count, 1, 10);
                Assert.True(profile.Provenance.TotalCount > 10);
                Assert.NotEmpty(profile.StudentRowVersion);
                Assert.NotEmpty(profile.StudentTermStateRowVersion);

                var prior = seedCommand.TranscriptAttempts[0];
                var corrected = await store.CorrectAsync(new CorrectAcademicProfileStoreCommand(
                    seedCommand.Student.Id,
                    termId,
                    profile.StudentRowVersion,
                    profile.StudentTermStateRowVersion,
                    "synthetic-correction",
                    [
                        new AppendTranscriptAttemptMutation(
                            Guid.NewGuid(),
                            prior.Id,
                            prior.CourseCode,
                            "2026-FALL-PORTS",
                            prior.Credits,
                            "A",
                            TranscriptAttemptStatus.Passed,
                            "spec008-correction")
                    ],
                    readRequest with { TranscriptPage = 1 },
                    Audit(
                        "student-academic-profile-corrected",
                        nameof(Student),
                        seedCommand.Student.Id,
                        nowUtc)));
                Assert.Equal(AcademicProfileStoreOutcome.Succeeded, corrected.Outcome);
                Assert.NotNull(corrected.Profile);
                profile = corrected.Profile!;
                Assert.Equal(26, profile.TranscriptAttempts.TotalCount);

                var callbackInvoked = false;
                var blocked = await store.ExecuteRegistrationBoundaryAsync(
                    new RegistrationBoundaryStoreCommand(
                        seedCommand.Student.Id,
                        termId,
                        profile.StudentTermStateRowVersion,
                        nowUtc),
                    _ =>
                    {
                        callbackInvoked = true;
                        return Task.CompletedTask;
                    });
                Assert.Equal(AcademicProfileStoreOutcome.HoldBlocked, blocked.Outcome);
                Assert.False(callbackInvoked);

                var blockingHold = Assert.Single(
                    seedCommand.Holds,
                    hold => hold.BlocksRegistration);
                var unblocked = await store.CorrectAsync(new CorrectAcademicProfileStoreCommand(
                    seedCommand.Student.Id,
                    termId,
                    profile.StudentRowVersion,
                    profile.StudentTermStateRowVersion,
                    "synthetic-correction",
                    [new RemoveStudentHoldMutation(blockingHold.Id, "spec008-hold-removal")],
                    readRequest with { TranscriptPage = 1 },
                    Audit(
                        "student-academic-profile-corrected",
                        nameof(StudentHold),
                        blockingHold.Id,
                        nowUtc)));
                Assert.Equal(AcademicProfileStoreOutcome.Succeeded, unblocked.Outcome);
                Assert.NotNull(unblocked.Profile);
                profile = unblocked.Profile!;
                Assert.DoesNotContain(profile.ActiveHolds, hold => hold.BlocksRegistration);

                var accepted = await store.ExecuteRegistrationBoundaryAsync(
                    new RegistrationBoundaryStoreCommand(
                        seedCommand.Student.Id,
                        termId,
                        profile.StudentTermStateRowVersion,
                        nowUtc),
                    _ =>
                    {
                        callbackInvoked = true;
                        return Task.CompletedTask;
                    });
                Assert.Equal(AcademicProfileStoreOutcome.Succeeded, accepted.Outcome);
                Assert.True(callbackInvoked);
            }

            var overflowSeed = DemoSeed(
                overflowUserId,
                overflowUniversityId,
                termId,
                nowUtc,
                transcriptCount: 1,
                holdCount: 101,
                blockingFirstHold: true,
                fixtureOrdinal: 184);
            await using (var overflowScope = provider.CreateAsyncScope())
            {
                var seedStore = overflowScope.ServiceProvider
                    .GetRequiredService<IDemoStudentProfileSeedStore>();
                Assert.True((await seedStore.ReconcileAsync(overflowSeed)).Created);
                var profileStore = overflowScope.ServiceProvider
                    .GetRequiredService<IStudentAcademicProfileStore>();
                var overflow = await profileStore.ReadByStudentIdAsync(
                    overflowSeed.Student.Id,
                    termId,
                    readRequest);
                Assert.Equal(AcademicProfileStoreOutcome.ProfileNotReady, overflow.Outcome);
                Assert.Null(overflow.Profile);
            }

            await using (var queryScope = provider.CreateAsyncScope())
            {
                var scopeReader = queryScope.ServiceProvider
                    .GetRequiredService<IAcademicStudentScopeReader>();
                var studentScope = await scopeReader.FindByApplicationUserIdAsync(primaryUserId);
                Assert.NotNull(studentScope);
                Assert.Equal("AI", studentScope!.ProgramCode);
                Assert.Equal("2026", studentScope.Cohort);

                var contextReader = queryScope.ServiceProvider
                    .GetRequiredService<IAcademicContextReader>();
                var academicContext = await contextReader.ResolveContextAsync(studentScope);
                Assert.Single(academicContext.Terms, term => term.Id == termId);
                Assert.Single(
                    academicContext.PublishedWindows,
                    window => window.Id == firstWindowId);

                var adminStore = queryScope.ServiceProvider
                    .GetRequiredService<IAdminAcademicStore>();
                var terms = await adminStore.ListTermsAsync(
                    new AdminTermQuery("2026", null, 1, 1, "code,id"));
                Assert.Equal(AdminAcademicStoreOutcome.Succeeded, terms.Outcome);
                Assert.Equal(1, terms.TotalCount);
                Assert.Equal("2026-FALL-PORTS", Assert.Single(terms.Items).Code);

                var students = await adminStore.ListStudentsAsync(
                    new AdminStudentLocatorQuery(
                        termId,
                        "AI26",
                        1,
                        1,
                        "universityId,studentId"));
                Assert.Equal(AdminAcademicStoreOutcome.Succeeded, students.Outcome);
                Assert.Equal(2, students.TotalCount);
                Assert.Single(students.Items);
            }

            await using (var evidenceScope = provider.CreateAsyncScope())
            {
                var context = evidenceScope.ServiceProvider
                    .GetRequiredService<StudentRegistrationDbContext>();
                var prior = await context.Set<TranscriptAttempt>()
                    .AsNoTracking()
                    .SingleAsync(attempt => attempt.Id == seedCommand.TranscriptAttempts[0].Id);
                var successor = await context.Set<TranscriptAttempt>()
                    .AsNoTracking()
                    .SingleAsync(attempt => attempt.SupersedesAttemptId == prior.Id);
                Assert.Equal("B", prior.GradeCode);
                Assert.Equal("A", successor.GradeCode);
                Assert.Equal(prior.StudentId, successor.StudentId);
                Assert.Equal(prior.TermId, successor.TermId);
                Assert.Equal(prior.CourseCode, successor.CourseCode);
                Assert.Equal(26, await context.Set<TranscriptAttempt>()
                    .CountAsync(attempt => attempt.StudentId == seedCommand.Student.Id));
                Assert.Equal(4, await context.AuditEvents.CountAsync());

                Assert.All(
                    await context.Set<RegistrationWindow>().AsNoTracking().ToArrayAsync(),
                    window =>
                    {
                        Assert.Equal(DateTimeKind.Utc, window.OpensAtUtc.Kind);
                        Assert.Equal(DateTimeKind.Utc, window.ClosesAtUtc.Kind);
                    });
                var reloadedStudent = await context.Set<Student>()
                    .AsNoTracking()
                    .SingleAsync(student => student.Id == seedCommand.Student.Id);
                Assert.Equal(DateTimeKind.Utc, reloadedStudent.DataAsOfUtc.Kind);
                Assert.Equal(DateTimeKind.Utc, reloadedStudent.ImportedAtUtc.Kind);
                Assert.All(
                    await context.Set<StudentHold>()
                        .AsNoTracking()
                        .Where(hold => hold.StudentId == overflowSeed.Student.Id)
                        .ToArrayAsync(),
                    hold =>
                    {
                        Assert.Equal(DateTimeKind.Utc, hold.EffectiveFromUtc.Kind);
                        Assert.True(hold.EffectiveToUtc.HasValue);
                        Assert.Equal(DateTimeKind.Utc, hold.EffectiveToUtc.Value.Kind);
                        Assert.Equal(DateTimeKind.Utc, hold.ImportedAtUtc.Kind);
                    });
            }
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

    private static async Task ProveStudentAndTranscriptUniquenessAsync(
        StudentRegistrationDbContext context,
        Guid termId)
    {
        var applicationUserId = Guid.NewGuid();
        context.Add(new ApplicationUser(
            applicationUserId,
            $"student-{applicationUserId:N}",
            $"STUDENT-{applicationUserId:N}",
            $"91{applicationUserId:N}"[..12],
            "identity-v3-hash-only",
            Guid.NewGuid().ToString("N")));
        await context.SaveChangesAsync();

        var firstStudent = CreateStudent(applicationUserId);
        context.Add(firstStudent);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        context.Add(CreateStudent(applicationUserId));
        await AssertSqlUniqueViolationAsync(() => context.SaveChangesAsync());
        context.ChangeTracker.Clear();

        var student = await context.Set<Student>()
            .SingleAsync(candidate => candidate.ApplicationUserId == applicationUserId);

        var prior = Attempt(student.Id, termId, supersedesAttemptId: null);
        context.Add(prior);
        await context.SaveChangesAsync();
        context.AddRange(
            Attempt(student.Id, termId, prior.Id),
            Attempt(student.Id, termId, prior.Id));
        await AssertSqlUniqueViolationAsync(() => context.SaveChangesAsync());
        context.ChangeTracker.Clear();

        Assert.Equal(
            1,
            await context.Set<TranscriptAttempt>()
                .CountAsync(candidate => candidate.StudentId == student.Id));
    }

    private static Student CreateStudent(Guid applicationUserId)
    {
        var now = new DateTime(2026, 7, 16, 8, 0, 0, DateTimeKind.Utc);
        return new Student(
            Guid.NewGuid(),
            applicationUserId,
            "AI",
            "2026",
            3.25m,
            45m,
            "active",
            true,
            "synthetic-test",
            "spec008-model",
            "SPEC008-V1",
            now,
            now);
    }

    private static TranscriptAttempt Attempt(
        Guid studentId,
        Guid termId,
        Guid? supersedesAttemptId) =>
        new(
            Guid.NewGuid(),
            studentId,
            termId,
            supersedesAttemptId,
            "CC214",
            3m,
            "B+",
            TranscriptAttemptStatus.Passed,
            "synthetic-test",
            Guid.NewGuid().ToString("N"),
            new DateTime(2026, 7, 16, 8, 0, 0, DateTimeKind.Utc));

    private static ApplicationUser CreateApplicationUser(
        Guid applicationUserId,
        string universityId) =>
        new(
            applicationUserId,
            $"student-{applicationUserId:N}",
            $"STUDENT-{applicationUserId:N}",
            universityId,
            "identity-v3-hash-only",
            Guid.NewGuid().ToString("N"));

    private static RegistrationWindow Window(
        Guid id,
        Guid termId,
        DateTime opensAtUtc,
        DateTime closesAtUtc,
        RegistrationWindowScopeType scopeType = RegistrationWindowScopeType.AllStudents,
        string? scopeValue = null) =>
        new(
            id,
            termId,
            scopeType,
            scopeValue,
            opensAtUtc,
            closesAtUtc,
            RegistrationWindowLifecycleState.Draft);

    private static DemoStudentProfileSeedCommand DemoSeed(
        Guid applicationUserId,
        string universityId,
        Guid termId,
        DateTime nowUtc,
        int transcriptCount,
        int holdCount,
        bool blockingFirstHold,
        int fixtureOrdinal = 83,
        decimal currentGpa = 3.25m)
    {
        var studentId = StableGuid($"student:{applicationUserId:D}");
        var student = new Student(
            studentId,
            applicationUserId,
            "AI",
            "2026",
            currentGpa,
            45m,
            "active",
            true,
            "synthetic-seed",
            $"student-{fixtureOrdinal}",
            "SPEC008-SEED-V1",
            nowUtc,
            nowUtc);
        var attempts = Enumerable.Range(1, transcriptCount)
            .Select(index => new TranscriptAttempt(
                StableGuid($"attempt:{applicationUserId:D}:{index}"),
                studentId,
                termId,
                null,
                $"AI{index:000}",
                3m,
                "B",
                TranscriptAttemptStatus.Passed,
                "synthetic-seed",
                $"attempt-{fixtureOrdinal}-{index}",
                nowUtc.AddMinutes(-index)))
            .ToArray();
        var holds = Enumerable.Range(1, holdCount)
            .Select(index => new StudentHold(
                StableGuid($"hold:{applicationUserId:D}:{index}"),
                studentId,
                termId,
                $"HOLD-{index:000}",
                $"Synthetic hold {index:000}",
                blockingFirstHold && index == 1,
                nowUtc.AddDays(-1),
                nowUtc.AddDays(30),
                "synthetic-seed",
                $"hold-{fixtureOrdinal}-{index}",
                nowUtc.AddMinutes(-index)))
            .ToArray();
        var termState = new StudentTermAcademicState(
            StableGuid($"term-state:{applicationUserId:D}:{termId:D}"),
            studentId,
            termId,
            currentGpa,
            45m,
            "active",
            "synthetic-seed",
            $"term-state-{fixtureOrdinal}",
            "SPEC008-SEED-V1",
            nowUtc);
        var provenance = attempts
            .Select(attempt => new AcademicProvenanceDto(
                attempt.Source,
                attempt.SourceReference,
                attempt.ImportedAtUtc))
            .Concat(holds.Select(hold => new AcademicProvenanceDto(
                hold.Source,
                hold.SourceReference,
                hold.ImportedAtUtc)))
            .Append(new AcademicProvenanceDto(
                student.Source,
                student.SourceReference,
                student.ImportedAtUtc))
            .ToArray();

        return new DemoStudentProfileSeedCommand(
            "SPEC008-SEED-V1",
            fixtureOrdinal,
            universityId,
            student,
            attempts,
            holds,
            termState,
            provenance);
    }

    private static Guid StableGuid(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return new Guid(hash.AsSpan(0, 16));
    }

    private static byte[] DecodeVersion(string value) => Convert.FromBase64String(value);

    private static TermInput ToTermInput(AdminTermDto term) =>
        new(
            term.Code,
            term.DisplayName,
            term.TimeZoneId,
            term.TeachingStartsOn,
            term.TeachingEndsOn,
            term.State);

    private static TermWindowInput ToTermWindowInput(AdminRegistrationWindowDto window) =>
        new(
            window.Id,
            window.ScopeType,
            window.ScopeValue,
            window.OpensAtUtc,
            window.ClosesAtUtc,
            window.LifecycleState);

    private static AuditEventDraft Audit(
        string action,
        string entityType,
        Guid entityId,
        DateTime occurredAtUtc) =>
        new(
            "admin:spec008",
            entityId.ToString("D"),
            action,
            entityType,
            entityId.ToString("D"),
            "Perform a governed synthetic academic mutation.",
            null,
            "{\"result\":\"accepted\"}",
            Guid.NewGuid().ToString("N"),
            occurredAtUtc);

    private static CreateAcademicTermStoreCommand CreateTermCommand(
        Guid termId,
        Guid windowId,
        Guid requestId,
        string payloadHash,
        string code)
    {
        var window = Window(
            windowId,
            termId,
            new DateTime(2026, 8, 1, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 8, 15, 20, 0, 0, DateTimeKind.Utc));
        return CreateTermCommand(termId, requestId, payloadHash, code, window);
    }

    private static CreateAcademicTermStoreCommand CreateTermCommand(
        Guid termId,
        Guid requestId,
        string payloadHash,
        string code,
        params RegistrationWindow[] windows)
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

        return new(
            term,
            windows,
            new AuditEventDraft(
                "admin:spec008",
                termId.ToString("D"),
                "academic-term-created",
                nameof(AcademicTerm),
                termId.ToString("D"),
                "Create a governed synthetic academic term.",
                null,
                "{\"state\":\"draft\"}",
                Guid.NewGuid().ToString("N"),
                new DateTime(2026, 7, 16, 8, 0, 0, DateTimeKind.Utc)));
    }

    private static ServiceProvider CreateSqlProvider(
        string connectionString,
        bool failAudit = false)
    {
        var services = new ServiceCollection();
        services.AddSingleton(TimeProvider.System);
        services.AddStudentRegistrationSqlServer(Configuration(connectionString));
        services.AddStudentRegistrationRegistrationModule();
        if (failAudit)
        {
            services.AddScoped<IAuditEventWriter, ThrowingAuditWriter>();
        }
        else
        {
            services.AddScoped<IAuditEventWriter, AuditTransactionWriter>();
        }

        return services.BuildServiceProvider(
            new ServiceProviderOptions
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

    private static StudentRegistrationDbContext CreateContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new StudentRegistrationDbContext(options);
    }

    private static IEntityType RequiredEntity<TEntity>(StudentRegistrationDbContext context) =>
        context.Model.FindEntityType(typeof(TEntity))
        ?? throw new Xunit.Sdk.XunitException(
            $"The relational model does not contain {typeof(TEntity).Name}.");

    private static void AssertUniqueIndex(IEntityType entity, params string[] propertyNames) =>
        Assert.True(
            FindIndex(entity, propertyNames).IsUnique,
            $"{entity.ClrType.Name}({string.Join(", ", propertyNames)}) must be unique.");

    private static void AssertIndex(IEntityType entity, params string[] propertyNames) =>
        _ = FindIndex(entity, propertyNames);

    private static void AssertFilteredUniqueIndex(
        IEntityType entity,
        string propertyName,
        string filterFragment)
    {
        var index = Assert.Single(
            entity.GetIndexes(),
            candidate => candidate.IsUnique &&
                candidate.Properties.Select(property => property.Name)
                    .SequenceEqual([propertyName], StringComparer.Ordinal) &&
                candidate.GetFilter()?.Contains(
                    filterFragment,
                    StringComparison.OrdinalIgnoreCase) is true);
        Assert.NotNull(index.GetDatabaseName());
    }

    private static IIndex FindIndex(IEntityType entity, params string[] propertyNames) =>
        entity.GetIndexes().SingleOrDefault(
            index => index.Properties.Select(property => property.Name)
                .SequenceEqual(propertyNames, StringComparer.Ordinal))
        ?? throw new Xunit.Sdk.XunitException(
            $"Missing {entity.ClrType.Name} index ({string.Join(", ", propertyNames)}).");

    private static void AssertRowVersion<TEntity>(IEntityType entity, string propertyName)
    {
        var property = entity.FindProperty(propertyName)
            ?? throw new Xunit.Sdk.XunitException(
                $"Missing {typeof(TEntity).Name}.{propertyName}.");
        Assert.True(property.IsConcurrencyToken);
        Assert.False(property.IsNullable);
        Assert.Equal(ValueGenerated.OnAddOrUpdate, property.ValueGenerated);
        Assert.Equal("rowversion", property.GetColumnType());
    }

    private static void AssertForeignKey<TDependent, TPrincipal>(
        IEntityType dependent,
        string propertyName)
    {
        var foreignKey = dependent.GetForeignKeys().SingleOrDefault(
            candidate => candidate.PrincipalEntityType.ClrType == typeof(TPrincipal)
                && candidate.Properties.Count == 1
                && candidate.Properties[0].Name == propertyName);
        Assert.NotNull(foreignKey);
        Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
    }

    private static void AssertDateTime2(IEntityType entity, string propertyName) =>
        Assert.Equal("datetime2", entity.FindProperty(propertyName)!.GetColumnType());

    private static void AssertEnumStoredAsString(
        IEntityType entity,
        string propertyName,
        int maximumLength)
    {
        var property = entity.FindProperty(propertyName)
            ?? throw new Xunit.Sdk.XunitException(
                $"Missing {entity.ClrType.Name}.{propertyName}.");
        var converter = property.GetValueConverter() ?? property.GetTypeMapping().Converter;
        Assert.NotNull(converter);
        Assert.Equal(typeof(string), converter!.ProviderClrType);
        Assert.False(property.IsNullable);
        Assert.Equal(maximumLength, property.GetMaxLength());
    }

    private static void AssertDecimal(
        IEntityType entity,
        string propertyName,
        int precision,
        int scale)
    {
        var property = entity.FindProperty(propertyName)
            ?? throw new Xunit.Sdk.XunitException(
                $"Missing {entity.ClrType.Name}.{propertyName}.");
        Assert.False(property.IsNullable);
        Assert.Equal(precision, property.GetPrecision());
        Assert.Equal(scale, property.GetScale());
        Assert.Equal($"decimal({precision},{scale})", property.GetColumnType());
    }

    private static void AssertRequired(IEntityType entity, string propertyName)
    {
        var property = entity.FindProperty(propertyName)
            ?? throw new Xunit.Sdk.XunitException(
                $"Missing {entity.ClrType.Name}.{propertyName}.");
        Assert.False(property.IsNullable);
    }

    private static void AssertBoundedRequired(
        IEntityType entity,
        string propertyName,
        int maximumLength)
    {
        var property = entity.FindProperty(propertyName)
            ?? throw new Xunit.Sdk.XunitException(
                $"Missing {entity.ClrType.Name}.{propertyName}.");
        Assert.False(property.IsNullable);
        Assert.Equal(maximumLength, property.GetMaxLength());
    }

    private static void AssertBoundedOptional(
        IEntityType entity,
        string propertyName,
        int maximumLength)
    {
        var property = entity.FindProperty(propertyName)
            ?? throw new Xunit.Sdk.XunitException(
                $"Missing {entity.ClrType.Name}.{propertyName}.");
        Assert.True(property.IsNullable);
        Assert.Equal(maximumLength, property.GetMaxLength());
    }

    private static void AssertCheckConstraintContains(
        StudentRegistrationDbContext context,
        IEntityType runtimeEntity,
        params string[] fragments)
    {
        var designEntity = context.GetService<IDesignTimeModel>()
            .Model
            .FindEntityType(runtimeEntity.ClrType);
        Assert.NotNull(designEntity);
        Assert.Contains(
            designEntity!.GetCheckConstraints(),
            constraint => fragments.All(fragment =>
                constraint.Sql.Contains(fragment, StringComparison.OrdinalIgnoreCase)));
    }

    private static void AssertCheckConstraintsContainAll(
        StudentRegistrationDbContext context,
        IEntityType runtimeEntity,
        params string[] fragments)
    {
        var designEntity = context.GetService<IDesignTimeModel>()
            .Model
            .FindEntityType(runtimeEntity.ClrType);
        Assert.NotNull(designEntity);
        var sql = string.Join(
            Environment.NewLine,
            designEntity!.GetCheckConstraints().Select(constraint => constraint.Sql));
        Assert.All(
            fragments,
            fragment => Assert.Contains(fragment, sql, StringComparison.OrdinalIgnoreCase));
    }

    private static async Task AssertSqlUniqueViolationAsync(Func<Task> save)
    {
        var exception = await Assert.ThrowsAsync<DbUpdateException>(save);
        var sqlException = exception.GetBaseException() as SqlException;
        Assert.NotNull(sqlException);
        Assert.Contains(sqlException!.Number, new[] { 2601, 2627 });
    }

    private sealed class ThrowingAuditWriter : IAuditEventWriter
    {
        public Task AppendAsync(
            AuditEventDraft auditEvent,
            CancellationToken cancellationToken) =>
            throw new InvalidOperationException("SPEC008_INJECTED_AUDIT_FAILURE");
    }
}
