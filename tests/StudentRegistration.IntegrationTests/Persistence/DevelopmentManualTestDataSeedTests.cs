using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.Academics.Domain;
using StudentRegistration.Api.Development;
using StudentRegistration.Contracts.Auditing;
using StudentRegistration.IdentityAccess.Application;
using StudentRegistration.IdentityAccess.Application.Ports;
using StudentRegistration.IdentityAccess.Domain;
using StudentRegistration.Infrastructure.SqlServer.Academics;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Infrastructure.SqlServer.Scheduling;
using StudentRegistration.Registration.Domain;
using StudentRegistration.IntegrationTests.Infrastructure;
using StudentRegistration.Scheduling.Application.Ports;
using StudentRegistration.Scheduling.Domain;
using Testcontainers.MsSql;
using CatalogueProgram = StudentRegistration.Academics.Domain.Program;

namespace StudentRegistration.IntegrationTests.Persistence;

public sealed class DevelopmentManualTestDataSeedTests
{
    private static readonly DateTime SeedNowUtc =
        new(2026, 7, 20, 18, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Student_personas_cover_first_and_later_terms_without_blanket_blocking_holds()
    {
        var store = new CapturingAcademicSeedStore();
        var contributor = new DemoStudentProfileSeedContributor(store);
        var termId = Guid.NewGuid();

        foreach (var ordinal in new[] { 1, 2, 3, 7 })
        {
            var universityId = $"AI26{ordinal:00000}";
            await contributor.ContributeAsync(
                "Development",
                "synthetic-fixture/2.0",
                ordinal,
                StableGuid(universityId),
                universityId,
                termId);
        }

        Assert.Equal(1, store.Commands.Single(item => item.FixtureOrdinal == 1)
            .StudentTermAcademicState.ProgramTermOrdinal);
        Assert.All(
            store.Commands.Where(item => item.FixtureOrdinal is 2 or 3),
            item => Assert.True(item.StudentTermAcademicState.ProgramTermOrdinal >= 2));

        var activeBlockers = store.Commands
            .SelectMany(command => command.Holds.Select(hold => (command, hold)))
            .Where(item => item.hold.BlocksRegistration
                && item.hold.IsActiveAt(item.command.Student.DataAsOfUtc))
            .ToArray();
        Assert.Single(activeBlockers);
        Assert.Equal(7, activeBlockers[0].command.FixtureOrdinal);
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Manual_test_seed_upgrades_v1_profiles_and_replays_the_complete_graph_in_real_sql()
    {
        await using var container = new MsSqlBuilder(
                SqlServerTestDatabaseFixture.SqlServerImage)
            .WithPassword($"Srs!1{Guid.NewGuid():N}a")
            .Build();
        await container.StartAsync();

        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(container.GetConnectionString())
            .Options;
        await using var context = new StudentRegistrationDbContext(options);
        await context.Database.EnsureCreatedAsync();
        var termId = await SeedPrerequisitesAsync(context);
        var clock = new FixedTimeProvider(new DateTimeOffset(SeedNowUtc));
        var academicStore = new AcademicStore(context, new NoOpAuditWriter(), clock);
        var profileContributor = new DemoStudentProfileSeedContributor(academicStore);
        const string universityId = "AI2600001";
        var applicationUserId = StableGuid(universityId);
        await profileContributor.ContributeAsync(
            "Development",
            "synthetic-fixture/1.0",
            1,
            applicationUserId,
            universityId,
            termId);
        await profileContributor.ContributeAsync(
            "Development",
            "synthetic-fixture/2.0",
            1,
            applicationUserId,
            universityId,
            termId);
        await profileContributor.ContributeAsync(
            "Development",
            "synthetic-fixture/2.0",
            1,
            applicationUserId,
            universityId,
            termId);

        var upgradedStudent = await context.Set<Student>()
            .AsNoTracking()
            .SingleAsync(item => item.ApplicationUserId == applicationUserId);
        var upgradedState = await context.Set<StudentTermAcademicState>()
            .AsNoTracking()
            .SingleAsync(item => item.StudentId == upgradedStudent.Id && item.TermId == termId);
        Assert.Equal("synthetic-fixture/2.0", upgradedStudent.DataVersion);
        Assert.Equal(1, upgradedState.ProgramTermOrdinal);
        Assert.Equal(
            ["BA101", "GN112"],
            await context.Set<TranscriptAttempt>()
                .Where(item => item.StudentId == upgradedStudent.Id)
                .OrderBy(item => item.CourseCode)
                .Select(item => item.CourseCode)
                .ToArrayAsync());
        Assert.DoesNotContain(
            await context.Set<StudentHold>()
                .Where(item => item.StudentId == upgradedStudent.Id)
                .ToArrayAsync(),
            item => item.BlocksRegistration && item.IsActiveAt(upgradedStudent.DataAsOfUtc));

        var catalogueService = new CataloguePublicationService();
        ICatalogueAdministrationStore catalogueStore =
            new SqlCatalogueAdministrationStore(context, catalogueService, clock);
        IOfferingStore offeringStore = new SqlOfferingStore(context);
        var seeder = new DevelopmentManualTestDataSeeder(
            context,
            catalogueService,
            new PolicyAdministrationService(),
            catalogueStore,
            offeringStore,
            clock);

        await seeder.SeedAsync(termId, "synthetic-fixture/2.0", CancellationToken.None);
        await seeder.SeedAsync(termId, "synthetic-fixture/2.0", CancellationToken.None);

        var version = await context.Set<CatalogueVersion>()
            .AsNoTracking()
            .SingleAsync(item => item.State == CatalogueVersionState.Published);
        var courses = await context.Set<Course>()
            .AsNoTracking()
            .Where(item => item.CatalogueVersionId == version.Id)
            .ToArrayAsync();
        var curriculum = await context.Set<CurriculumCourse>()
            .AsNoTracking()
            .Where(item => item.CatalogueVersionId == version.Id)
            .ToArrayAsync();
        var prerequisiteCourseIds = await context.Set<CoursePrerequisite>()
            .AsNoTracking()
            .Where(item => item.CatalogueVersionId == version.Id)
            .Select(item => item.CourseId)
            .Distinct()
            .ToArrayAsync();

        Assert.Equal(19, courses.Length);
        Assert.All(courses, course => Assert.Equal(3m, course.Credits));
        Assert.Equal(19, curriculum.Length);
        Assert.All(
            curriculum.Where(item => item.RecommendedTerm > 1),
            item => Assert.Contains(item.CourseId, prerequisiteCourseIds));

        var policy = await context.Set<PolicySet>()
            .AsNoTracking()
            .SingleAsync(item => item.TermId == termId && item.State == PolicySetState.Published);
        var rules = await context.Set<PolicyRule>()
            .AsNoTracking()
            .Where(item => item.PolicySetId == policy.Id)
            .ToDictionaryAsync(item => item.Code, item => item.Value);
        Assert.Equal("12", rules["PROBATION_MAX_CREDITS"]);
        Assert.Equal("18", rules["NORMAL_MAX_CREDITS"]);
        Assert.Equal("21", rules["OVERLOAD_MAX_CREDITS"]);
        Assert.Equal("3.0", rules["OVERLOAD_MIN_GPA"]);

        Assert.Equal(19, await context.Set<CourseOffering>().CountAsync());
        Assert.All(
            await context.Set<CourseOffering>().AsNoTracking().ToArrayAsync(),
            item => Assert.Equal(CourseOfferingState.Published, item.State));
        Assert.Equal(19, await context.Set<SectionGroup>().CountAsync());
        Assert.All(
            await context.Set<SectionGroup>().AsNoTracking().ToArrayAsync(),
            item => Assert.Equal(SectionGroupState.Published, item.State));
        Assert.Equal(38, await context.Set<MeetingSlot>().CountAsync());
        Assert.Equal(38, await context.Set<GroupStaffAssignment>().CountAsync());
        Assert.Equal(2, await context.Set<Room>().CountAsync());
        Assert.Single(await context.Set<CatalogueProgram>().ToArrayAsync());
        Assert.Single(await context.Set<CatalogueVersion>().ToArrayAsync());
        Assert.Single(await context.Set<PolicySet>().ToArrayAsync());
        var automaticSubmission = await context.Set<RegistrationSubmission>()
            .AsNoTracking()
            .SingleAsync(item => item.StudentId == upgradedStudent.Id
                && item.TermId == termId
                && item.Origin == RegistrationSubmissionOrigin.FirstTermAutomatic);
        Assert.Equal(RegistrationSubmissionState.Accepted, automaticSubmission.ProcessingState);
        Assert.Equal(12m, automaticSubmission.RequestedCredits);
        Assert.Equal(4, await context.Set<Enrollment>()
            .CountAsync(item => item.SubmissionId == automaticSubmission.Id));
        Assert.Empty(await context.Set<RegistrationSeatHold>().ToArrayAsync());
        Assert.Empty(await context.Set<RegistrationApprovalDecision>().ToArrayAsync());
        var automaticBatch = await context.Set<FirstTermAutoEnrollmentBatch>()
            .Include(item => item.Items)
            .SingleAsync(item => item.TermId == termId);
        Assert.Equal(FirstTermAutoEnrollmentBatchState.Complete, automaticBatch.State);
        Assert.Equal(1, automaticBatch.AcceptedStudents);
        Assert.Equal(4, await context.Set<SectionGroup>()
            .SumAsync(item => item.EnrolledCount));
    }

    private static async Task<Guid> SeedPrerequisitesAsync(
        StudentRegistrationDbContext context)
    {
        var termId = Guid.NewGuid();
        context.Add(new AcademicTerm(
            termId,
            "DEMO-2026-FALL",
            Guid.NewGuid(),
            "manual-test-seed",
            "Synthetic AI Demo Fall 2026",
            new DateOnly(2026, 9, 20),
            new DateOnly(2027, 1, 15),
            "Africa/Cairo",
            StudentRegistration.Contracts.TermState.RegistrationOpen));

        AddStaff("LEC-0001", "Demo Lecturer");
        AddStaff("TA-0001", "Demo Teaching Assistant");
        var studentUserId = StableGuid("AI2600001");
        context.Add(new ApplicationUser(
            studentUserId,
            "AI2600001",
            "AI2600001",
            "AI2600001",
            "test-password-hash",
            "test-security-stamp"));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        return termId;

        void AddStaff(string staffNumber, string displayName)
        {
            var userId = StableGuid(staffNumber);
            context.Add(new ApplicationUser(
                userId,
                staffNumber,
                staffNumber,
                universityId: null,
                "test-password-hash",
                "test-security-stamp"));
            context.Add(new Staff(
                StableGuid($"{userId:N}|staff"),
                userId,
                staffNumber,
                displayName));
        }
    }

    private static Guid StableGuid(string value)
    {
        var hash = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(value));
        return new Guid(hash.AsSpan(0, 16));
    }

    private sealed class CapturingAcademicSeedStore : IDemoStudentProfileSeedStore
    {
        public List<DemoStudentProfileSeedCommand> Commands { get; } = [];

        public Task<DemoStudentProfileSeedResult> ReconcileAsync(
            DemoStudentProfileSeedCommand command,
            CancellationToken cancellationToken = default)
        {
            Commands.Add(command);
            return Task.FromResult(new DemoStudentProfileSeedResult(
                command.Student.Id,
                Created: true));
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class NoOpAuditWriter : IAuditEventWriter
    {
        public Task AppendAsync(
            AuditEventDraft auditEvent,
            CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
