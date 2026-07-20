using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.Academics.Domain;
using StudentRegistration.Contracts.Academics;
using StudentRegistration.IdentityAccess.Domain;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Registration.Application;
using StudentRegistration.Registration.Domain;
using StudentRegistration.Scheduling.Application;
using StudentRegistration.Scheduling.Application.Ports;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.Api.Development;

public sealed class DevelopmentManualTestDataSeeder(
    StudentRegistrationDbContext dbContext,
    CataloguePublicationService catalogueService,
    PolicyAdministrationService policyService,
    ICatalogueAdministrationStore catalogueStore,
    IOfferingStore offeringStore,
    TimeProvider timeProvider)
{
    private const string ScopeCode = "AI-DS";
    private const string CatalogueSource =
        "https://aast.edu/en/programs-courses/program.php?language_id=1&program_id=283&unit_id=655";
    private static readonly DateOnly CatalogueAccessedOn = new(2026, 7, 13);

    public async Task SeedAsync(
        Guid termId,
        string seedProfileVersion,
        CancellationToken cancellationToken)
    {
        if (termId == Guid.Empty)
        {
            throw new ArgumentException("An academic term is required.", nameof(termId));
        }
        ArgumentException.ThrowIfNullOrWhiteSpace(seedProfileVersion);

        if (dbContext.Database.CurrentTransaction is not null)
        {
            await SeedCoreAsync(termId, seedProfileVersion.Trim(), cancellationToken)
                .ConfigureAwait(false);
            return;
        }

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database
                .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
                .ConfigureAwait(false);
            await SeedCoreAsync(termId, seedProfileVersion.Trim(), cancellationToken)
                .ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    private async Task SeedCoreAsync(
        Guid termId,
        string seedProfileVersion,
        CancellationToken cancellationToken)
    {
        var catalogueVersionId = await EnsureCatalogueAsync(
                seedProfileVersion,
                cancellationToken)
            .ConfigureAwait(false);
        await EnsurePolicyAsync(termId, cancellationToken).ConfigureAwait(false);
        var resources = await EnsureResourcesAsync(seedProfileVersion, cancellationToken)
            .ConfigureAwait(false);
        await EnsureOfferingsAsync(
                termId,
                catalogueVersionId,
                resources,
                cancellationToken)
            .ConfigureAwait(false);
        await EnsureFirstTermAutoEnrollmentsAsync(
                termId,
                catalogueVersionId,
                seedProfileVersion,
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<Guid> EnsureCatalogueAsync(
        string seedProfileVersion,
        CancellationToken cancellationToken)
    {
        var content = CataloguePublicationService.CreateDemoCurriculum();
        var validation = catalogueService.Validate(content);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(
                $"DEMO_CATALOGUE_INVALID: {string.Join(',', validation.Errors.Select(item => item.Code))}");
        }

        var existing = await dbContext.Set<CatalogueVersion>()
            .AsNoTracking()
            .Where(item => item.ScopeCode == ScopeCode
                && item.State == CatalogueVersionState.Published)
            .OrderByDescending(item => item.PublishedAtUtc)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        if (existing is not null)
        {
            await ValidateExistingCatalogueAsync(existing.Id, content, cancellationToken)
                .ConfigureAwait(false);
            return existing.Id;
        }

        var draftId = StableGuid($"catalogue-draft:{seedProfileVersion}:{ScopeCode}");
        if (await dbContext.Set<CatalogueDraft>()
            .AsNoTracking()
            .AnyAsync(item => item.Id == draftId, cancellationToken)
            .ConfigureAwait(false))
        {
            throw new InvalidOperationException(
                "DEMO_CATALOGUE_SEED_INCOMPLETE: Reset the local Development database and retry.");
        }

        dbContext.Add(new CatalogueDraft(
            draftId,
            ScopeCode,
            basedOnVersionId: null,
            validation.CanonicalContentHash,
            JsonSerializer.Serialize(content),
            validationSummaryJson: string.Empty,
            CatalogueDraftState.Editing));
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        dbContext.ChangeTracker.Clear();

        var import = await catalogueStore.CreateImportAsync(
                new CreateImportRequest(
                    draftId,
                    CatalogueSource,
                    CatalogueAccessedOn,
                    validation.CanonicalContentHash,
                    ["Credits", "IsActive"],
                    StableGuid($"catalogue-import:{seedProfileVersion}:{ScopeCode}")),
                cancellationToken)
            .ConfigureAwait(false);
        var draft = await catalogueStore.GetDraftAsync(draftId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("The demo catalogue draft was not persisted.");
        var actor = "development-demo-seed";
        var checkedImport = await catalogueStore.ValidateImportAsync(
                new(
                    import.Id,
                    import.RowVersion,
                    draft.RowVersion,
                    actor),
                cancellationToken)
            .ConfigureAwait(false);
        if (!checkedImport.Valid || string.IsNullOrWhiteSpace(checkedImport.PreviewToken))
        {
            throw new InvalidOperationException("The curated demo catalogue did not validate.");
        }

        var published = await catalogueStore.PublishImportAsync(
                new(
                    import.Id,
                    checkedImport.ExpectedDraftRowVersion,
                    checkedImport.PreviewToken,
                    StableGuid($"catalogue-publish:{seedProfileVersion}:{ScopeCode}"),
                    actor),
                cancellationToken)
            .ConfigureAwait(false);
        return published.Id;
    }

    private async Task ValidateExistingCatalogueAsync(
        Guid versionId,
        CatalogueDraftContent content,
        CancellationToken cancellationToken)
    {
        var expected = content.Courses.Select(item => item.Code).ToHashSet(StringComparer.Ordinal);
        var actual = await dbContext.Set<Course>()
            .AsNoTracking()
            .Where(item => item.CatalogueVersionId == versionId)
            .Select(item => new { item.Id, item.Code, item.Credits })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var curriculum = await dbContext.Set<CurriculumCourse>()
            .AsNoTracking()
            .Where(item => item.CatalogueVersionId == versionId)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var prerequisiteCourses = await dbContext.Set<CoursePrerequisite>()
            .AsNoTracking()
            .Where(item => item.CatalogueVersionId == versionId)
            .Select(item => item.CourseId)
            .Distinct()
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        if (actual.Length != expected.Count
            || actual.Any(item => item.Credits != 3m || !expected.Contains(item.Code))
            || curriculum.Length != expected.Count
            || curriculum.Where(item => item.RecommendedTerm > 1)
                .Any(item => !prerequisiteCourses.Contains(item.CourseId)))
        {
            throw new InvalidOperationException(
                "DEMO_CATALOGUE_SEED_MISMATCH: Reset the local Development database and retry.");
        }
    }

    private async Task EnsurePolicyAsync(
        Guid termId,
        CancellationToken cancellationToken)
    {
        var existing = await dbContext.Set<PolicySet>()
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.TermId == termId
                && item.ScopeCode == ScopeCode
                && item.State == PolicySetState.Published, cancellationToken)
            .ConfigureAwait(false);
        if (existing is not null)
        {
            var rules = await dbContext.Set<PolicyRule>()
                .AsNoTracking()
                .Where(item => item.PolicySetId == existing.Id)
                .ToDictionaryAsync(item => item.Code, item => item.Value, cancellationToken)
                .ConfigureAwait(false);
            if (!HasRequiredPolicyValues(rules))
            {
                throw new InvalidOperationException(
                    "DEMO_POLICY_SEED_MISMATCH: Reset the local Development database and retry.");
            }
            return;
        }

        var draft = PolicyAdministrationService.CreateDemoPolicySet(termId);
        var validation = policyService.Validate(draft);
        if (!validation.IsValid
            || policyService.Publish(draft, canPublish: true)
                is not PolicyPublishOutcome.Published)
        {
            throw new InvalidOperationException("The demo registration policy did not validate.");
        }

        dbContext.Add(draft.PolicySet);
        dbContext.AddRange(draft.Rules);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        dbContext.ChangeTracker.Clear();
    }

    private async Task<SeedResources> EnsureResourcesAsync(
        string seedProfileVersion,
        CancellationToken cancellationToken)
    {
        var rooms = new[]
        {
            new RoomDefinition(
                StableGuid($"room:{seedProfileVersion}:AI-R101"),
                "AI-R101",
                "AI Building - Room 101",
                60),
            new RoomDefinition(
                StableGuid($"room:{seedProfileVersion}:AI-LAB1"),
                "AI-LAB1",
                "AI Building - Laboratory 1",
                40),
        };
        foreach (var definition in rooms)
        {
            var existing = await dbContext.Set<Room>()
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.Id == definition.Id, cancellationToken)
                .ConfigureAwait(false);
            if (existing is null)
            {
                dbContext.Add(new Room(
                    definition.Id,
                    definition.Code,
                    definition.Location,
                    definition.Capacity,
                    RoomAvailabilityState.Available));
            }
            else if (!string.Equals(existing.Code, definition.Code, StringComparison.Ordinal)
                || existing.Capacity != definition.Capacity
                || existing.AvailabilityState != RoomAvailabilityState.Available)
            {
                throw new InvalidOperationException(
                    "DEMO_ROOM_SEED_MISMATCH: Reset the local Development database and retry.");
            }
        }
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        dbContext.ChangeTracker.Clear();

        var staff = await dbContext.Set<Staff>()
            .AsNoTracking()
            .Where(item => item.StaffNumber == "LEC-0001" || item.StaffNumber == "TA-0001")
            .ToDictionaryAsync(item => item.StaffNumber, item => item.Id, cancellationToken)
            .ConfigureAwait(false);
        if (!staff.TryGetValue("LEC-0001", out var lecturerId)
            || !staff.TryGetValue("TA-0001", out var teachingAssistantId))
        {
            throw new InvalidOperationException(
                "DEMO_STAFF_SEED_INCOMPLETE: Lecturer and TeachingAssistant identities are required.");
        }

        return new(rooms[0].Id, rooms[1].Id, lecturerId, teachingAssistantId);
    }

    private async Task EnsureOfferingsAsync(
        Guid termId,
        Guid catalogueVersionId,
        SeedResources resources,
        CancellationToken cancellationToken)
    {
        var courses = await dbContext.Set<Course>()
            .AsNoTracking()
            .Where(item => item.CatalogueVersionId == catalogueVersionId && item.IsActive)
            .OrderBy(item => item.Code)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        for (var index = 0; index < courses.Length; index++)
        {
            var course = courses[index];
            var existingId = await dbContext.Set<CourseOffering>()
                .AsNoTracking()
                .Where(item => item.TermId == termId && item.CourseId == course.Id)
                .Select(item => (Guid?)item.Id)
                .SingleOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
            if (existingId is not null)
            {
                await ValidateExistingOfferingAsync(existingId.Value, cancellationToken)
                    .ConfigureAwait(false);
                continue;
            }

            var created = await offeringStore.CreateAsync(
                    new(
                        termId,
                        course.Id,
                        [new CreateOfferingGroup("A", 30 + index % 3 * 5)],
                        "Development manual-test seed"),
                    cancellationToken)
                .ConfigureAwait(false);
            var group = created.Groups.Single();
            var day = index % 5;
            var startHour = 8 + index / 5 * 2;
            var lectureId = StableGuid($"meeting:{group.Id:N}:lecture");
            var tutorialId = StableGuid($"meeting:{group.Id:N}:tutorial");
            await offeringStore.UpdateGroupAsync(
                    new(
                        group.Id,
                        created.RowVersion,
                        group.RowVersion,
                        group.GroupCode,
                        group.Capacity,
                        RegistrationPaused: false,
                        [
                            new(
                                lectureId,
                                resources.LectureRoomId,
                                "Lecture",
                                day,
                                new TimeOnly(startHour, 0),
                                new TimeOnly(startHour + 1, 0)),
                            new(
                                tutorialId,
                                resources.TutorialRoomId,
                                "Tutorial",
                                day,
                                new TimeOnly(startHour + 1, 0),
                                new TimeOnly(startHour + 2, 0)),
                        ],
                        [
                            new(lectureId, resources.LecturerId, "Lecturer"),
                            new(tutorialId, resources.TeachingAssistantId, "TeachingAssistant"),
                        ],
                        "development-demo-seed",
                        "Complete the published manual-test activity bundle"),
                    cancellationToken)
                .ConfigureAwait(false);

            var offering = await dbContext.Set<CourseOffering>()
                .SingleAsync(item => item.Id == created.Id, cancellationToken)
                .ConfigureAwait(false);
            var trackedGroup = await dbContext.Set<SectionGroup>()
                .SingleAsync(item => item.Id == group.Id, cancellationToken)
                .ConfigureAwait(false);
            offering.Publish();
            trackedGroup.MarkPublished();
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            dbContext.ChangeTracker.Clear();
        }
    }

    private async Task ValidateExistingOfferingAsync(
        Guid offeringId,
        CancellationToken cancellationToken)
    {
        var snapshot = await offeringStore.LoadAsync(offeringId, cancellationToken)
            .ConfigureAwait(false);
        if (snapshot is null
            || !string.Equals(snapshot.State, "published", StringComparison.OrdinalIgnoreCase)
            || snapshot.Groups.Count != 1
            || snapshot.Groups.Any(group =>
                !string.Equals(group.State, "published", StringComparison.OrdinalIgnoreCase)
                || group.Meetings.Count != 2
                || group.Meetings.SelectMany(meeting => meeting.Staff).Count() != 2))
        {
            throw new InvalidOperationException(
                "DEMO_OFFERING_SEED_MISMATCH: Reset the local Development database and retry.");
        }
    }

    private async Task EnsureFirstTermAutoEnrollmentsAsync(
        Guid termId,
        Guid catalogueVersionId,
        string seedProfileVersion,
        CancellationToken cancellationToken)
    {
        const string purpose = "required-first-term-curriculum";
        var batchId = StableGuid(
            $"first-term-batch:{seedProfileVersion}:{termId:D}:{catalogueVersionId:D}");
        var firstTermStudents = await (
                from state in dbContext.Set<StudentTermAcademicState>()
                join student in dbContext.Set<Student>() on state.StudentId equals student.Id
                where state.TermId == termId
                    && state.ProgramTermOrdinal == 1
                    && student.IsActive
                orderby student.Id
                select new { State = state, Student = student })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var existingBatch = await dbContext.Set<FirstTermAutoEnrollmentBatch>()
            .Include(item => item.Items)
            .SingleOrDefaultAsync(item => item.Id == batchId, cancellationToken)
            .ConfigureAwait(false);
        if (existingBatch is not null)
        {
            if (existingBatch.State is not FirstTermAutoEnrollmentBatchState.Complete
                || existingBatch.TotalStudents != firstTermStudents.Length
                || existingBatch.AcceptedStudents != firstTermStudents.Length)
            {
                throw new InvalidOperationException(
                    "DEMO_FIRST_TERM_SEED_MISMATCH: Reset the local Development database and retry.");
            }

            return;
        }

        var term = await dbContext.Set<AcademicTerm>()
            .AsNoTracking()
            .SingleAsync(item => item.Id == termId, cancellationToken)
            .ConfigureAwait(false);
        var policy = await dbContext.Set<PolicySet>()
            .AsNoTracking()
            .SingleAsync(item => item.TermId == termId
                && item.ScopeCode == ScopeCode
                && item.State == PolicySetState.Published, cancellationToken)
            .ConfigureAwait(false);
        var requiredCourses = await (
                from roadmap in dbContext.Set<CurriculumCourse>().AsNoTracking()
                join program in dbContext.Set<StudentRegistration.Academics.Domain.Program>().AsNoTracking()
                    on new { roadmap.CatalogueVersionId, roadmap.ProgramId }
                    equals new { program.CatalogueVersionId, ProgramId = program.Id }
                join course in dbContext.Set<Course>().AsNoTracking()
                    on new { roadmap.CatalogueVersionId, roadmap.CourseId }
                    equals new { course.CatalogueVersionId, CourseId = course.Id }
                where roadmap.CatalogueVersionId == catalogueVersionId
                    && roadmap.RecommendedTerm == 1
                    && roadmap.IsRequired
                    && course.IsActive
                orderby course.Code
                select new
                {
                    Course = course,
                    ProgramCode = program.Code,
                    roadmap.CohortScope,
                })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        if (requiredCourses.Length == 0)
        {
            throw new InvalidOperationException(
                "DEMO_FIRST_TERM_COURSES_MISSING: The published roadmap has no required first-term subjects.");
        }

        var requiredCourseIds = requiredCourses.Select(item => item.Course.Id).ToArray();
        if (await dbContext.Set<CoursePrerequisite>().AsNoTracking().AnyAsync(
                item => item.CatalogueVersionId == catalogueVersionId
                    && requiredCourseIds.Contains(item.CourseId),
                cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException(
                "DEMO_FIRST_TERM_PREREQUISITE_INVALID: First-term subjects cannot have prerequisites.");
        }

        var offeredGroups = await (
                from offering in dbContext.Set<CourseOffering>()
                join section in dbContext.Set<SectionGroup>() on offering.Id equals section.OfferingId
                where offering.TermId == termId
                    && requiredCourseIds.Contains(offering.CourseId)
                    && offering.State == CourseOfferingState.Published
                    && section.State == SectionGroupState.Published
                orderby section.GroupCode
                select new { Offering = offering, Group = section })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var groupByCourse = offeredGroups
            .GroupBy(item => item.Offering.CourseId)
            .ToDictionary(group => group.Key, group => group.First());
        if (requiredCourseIds.Any(courseId => !groupByCourse.ContainsKey(courseId)))
        {
            throw new InvalidOperationException(
                "DEMO_FIRST_TERM_OFFERING_MISSING: Every first-term subject requires a published group.");
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var batch = new FirstTermAutoEnrollmentBatch(
            batchId,
            termId,
            catalogueVersionId,
            ScopeCode,
            purpose,
            now);
        foreach (var studentContext in firstTermStudents)
        {
            var clientRequestId = StableGuid(
                $"first-term-request:{seedProfileVersion}:{termId:D}:{studentContext.Student.Id:D}");
            batch.AddItem(new FirstTermAutoEnrollmentItem(
                StableGuid($"first-term-item:{batchId:D}:{studentContext.Student.Id:D}"),
                batch.Id,
                studentContext.Student.Id,
                clientRequestId));
        }

        batch.Start("development-demo-seed", now, now.AddMinutes(5));
        foreach (var item in batch.Items)
        {
            item.Start(now);
            var student = firstTermStudents.Single(candidate => candidate.Student.Id == item.StudentId);
            var courses = requiredCourses.Where(candidate =>
                    (candidate.ProgramCode == student.Student.ProgramCode
                        || candidate.ProgramCode.StartsWith(student.Student.ProgramCode + "-", StringComparison.Ordinal))
                    && (candidate.CohortScope is null
                        || candidate.CohortScope == student.Student.Cohort))
                .ToArray();
            if (courses.Length == 0)
            {
                item.Fail("FIRST_TERM_CURRICULUM_NOT_FOUND", now);
                continue;
            }

            var submissionId = StableGuid($"first-term-submission:{item.ClientRequestId:D}");
            var submission = new RegistrationSubmission(
                submissionId,
                item.StudentId,
                termId,
                item.ClientRequestId,
                $"seed:{item.ClientRequestId:N}",
                now,
                RegistrationSubmissionOrigin.FirstTermAutomatic,
                courses.Sum(course => course.Course.Credits));
            var receiptGroups = new List<RegistrationGroupSnapshotDto>();
            var groupVersions = new Dictionary<Guid, string>();
            foreach (var course in courses)
            {
                var selected = groupByCourse[course.Course.Id];
                selected.Group.AllocateSeat();
                dbContext.Add(new Enrollment(
                    StableGuid($"first-term-enrollment:{submissionId:D}:{selected.Offering.Id:D}"),
                    item.StudentId,
                    selected.Offering.Id,
                    selected.Group.Id,
                    submissionId,
                    EnrollmentState.Active,
                    now));
                receiptGroups.Add(new RegistrationGroupSnapshotDto(
                    selected.Offering.Id,
                    course.Course.Code,
                    course.Course.Title,
                    selected.Group.Id,
                    selected.Group.GroupCode,
                    course.Course.Credits,
                    await BuildMeetingSnapshotsAsync(
                        selected.Group.Id,
                        cancellationToken).ConfigureAwait(false)));
                groupVersions[selected.Group.Id] = Convert.ToBase64String(selected.Group.Version);
            }

            var receipt = new RegistrationReceiptSnapshotDto(
                new(term.Id, term.Code, term.DisplayName, term.TimeZoneId),
                receiptGroups,
                courses.Sum(course => course.Course.Credits),
                policy.Id,
                policy.VersionCode,
                now);
            var decision = new DecisionSnapshot(
                policy.VersionCode,
                Convert.ToBase64String(student.State.Version),
                "first-term-automatic",
                groupVersions,
                "REGISTERED");
            submission.CompleteAccepted(
                "REGISTERED",
                $"AUTO-{submissionId.ToString("N")[..12].ToUpperInvariant()}",
                JsonSerializer.Serialize(receipt),
                JsonSerializer.Serialize(decision),
                now);
            dbContext.Add(submission);
            item.Accept(submissionId, now);
        }

        batch.Complete(now);
        dbContext.Add(batch);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        dbContext.ChangeTracker.Clear();
    }

    private async Task<IReadOnlyList<RegistrationMeetingSnapshotDto>> BuildMeetingSnapshotsAsync(
        Guid groupId,
        CancellationToken cancellationToken)
    {
        var meetings = await (
                from meeting in dbContext.Set<MeetingSlot>().AsNoTracking()
                join room in dbContext.Set<Room>().AsNoTracking() on meeting.RoomId equals room.Id
                where meeting.GroupId == groupId
                orderby meeting.DayOfWeek, meeting.StartLocal, meeting.Id
                select new
                {
                    Meeting = meeting,
                    room.Code,
                    room.Location,
                })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var staff = await (
                from assignment in dbContext.Set<GroupStaffAssignment>().AsNoTracking()
                join member in dbContext.Set<Staff>().AsNoTracking() on assignment.StaffId equals member.Id
                where assignment.GroupId == groupId && member.IsActive
                orderby assignment.MeetingSlotId, assignment.TeachingRole, member.DisplayName
                select new
                {
                    assignment.MeetingSlotId,
                    Role = assignment.TeachingRole == TeachingRole.Lecturer
                        ? "Lecturer"
                        : "TeachingAssistant",
                    member.DisplayName,
                })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        return meetings.Select(item => new RegistrationMeetingSnapshotDto(
            item.Meeting.Id,
            item.Meeting.ActivityType.ToString(),
            (int)item.Meeting.DayOfWeek,
            item.Meeting.StartLocal.ToString("HH:mm"),
            item.Meeting.EndLocal.ToString("HH:mm"),
            item.Code,
            item.Location,
            staff.Where(member => member.MeetingSlotId == item.Meeting.Id)
                .Select(member => new RegistrationMeetingStaffSnapshotDto(
                    member.Role,
                    member.DisplayName))
                .ToArray())).ToArray();
    }

    private static bool HasRequiredPolicyValues(
        IReadOnlyDictionary<string, string> rules) =>
        rules.GetValueOrDefault("PROBATION_MAX_CREDITS") == "12"
        && rules.GetValueOrDefault("NORMAL_MAX_CREDITS") == "18"
        && rules.GetValueOrDefault("OVERLOAD_MAX_CREDITS") == "21"
        && rules.GetValueOrDefault("OVERLOAD_MIN_GPA") == "3.0";

    private static Guid StableGuid(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return new Guid(hash.AsSpan(0, 16));
    }

    private sealed record RoomDefinition(
        Guid Id,
        string Code,
        string Location,
        int Capacity);

    private sealed record SeedResources(
        Guid LectureRoomId,
        Guid TutorialRoomId,
        Guid LecturerId,
        Guid TeachingAssistantId);
}
