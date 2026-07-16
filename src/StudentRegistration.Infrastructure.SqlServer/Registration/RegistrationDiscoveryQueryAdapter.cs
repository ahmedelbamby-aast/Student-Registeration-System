using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.Academics.Domain;
using StudentRegistration.IdentityAccess.Domain;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Registration.Application.Ports;
using StudentRegistration.Registration.Domain;
using StudentRegistration.Scheduling.Application.Ports;
using StudentRegistration.Scheduling.Domain;
using AcademicProgram = StudentRegistration.Academics.Domain.Program;

namespace StudentRegistration.Infrastructure.SqlServer.Registration;

public sealed class RegistrationDiscoveryQueryAdapter(
    StudentRegistrationDbContext dbContext) :
    IEligibilityAcademicReader,
    IEligibilityOfferingReader,
    ICurrentPlanReader
{
    private const string DemoApprovalReference = "DEMO-APPROVAL-2026.1";
    private const string DemoApprovedBy = "Ahmed ELbamby";

    private static readonly string[] RequiredPolicyCodes =
    [
        "REGISTRATION_WINDOW_OPEN",
        "ACADEMIC_STANDING_ALLOWED",
        "PREREQUISITES_REQUIRED",
        "NORMAL_MAX_CREDITS",
        "PROBATION_MAX_CREDITS",
        "CAPACITY_REQUIRED",
        "CONFLICT_BLOCKED",
    ];

    public async Task<EligibilityAcademicSnapshot?> ReadAsync(
        Guid applicationUserId,
        Guid termId,
        IReadOnlyCollection<Guid> courseIds,
        DateTime evaluatedAtUtc,
        CancellationToken cancellationToken = default)
    {
        if (applicationUserId == Guid.Empty ||
            termId == Guid.Empty ||
            evaluatedAtUtc.Kind is not DateTimeKind.Utc)
        {
            return null;
        }

        var requestedCourseIds = (courseIds ?? [])
            .Where(id => id != Guid.Empty)
            .Distinct()
            .Take(100)
            .ToArray();
        var student = await StudentForApplicationUserQuery(applicationUserId)
            .SingleOrDefaultAsync(cancellationToken);
        if (student is null)
        {
            return null;
        }

        var academicState = await dbContext.Set<StudentTermAcademicState>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                state => state.StudentId == student.Id && state.TermId == termId,
                cancellationToken);
        if (academicState is null)
        {
            return null;
        }

        var windows = await dbContext.Set<RegistrationWindow>()
            .AsNoTracking()
            .Where(window =>
                window.TermId == termId &&
                window.State == RegistrationWindowLifecycleState.Published)
            .OrderBy(window => window.Id)
            .ToArrayAsync(cancellationToken);
        var window = windows
            .Where(item => AppliesTo(item, student))
            .OrderByDescending(item => ScopePriority(item.ScopeType))
            .ThenBy(item => item.Id)
            .FirstOrDefault();

        var holds = await dbContext.Set<StudentHold>()
            .AsNoTracking()
            .Where(hold =>
                hold.StudentId == student.Id &&
                hold.TermId == termId &&
                hold.EffectiveFromUtc <= evaluatedAtUtc &&
                (hold.EffectiveToUtc == null ||
                    evaluatedAtUtc < hold.EffectiveToUtc))
            .OrderBy(hold => hold.Code)
            .ThenBy(hold => hold.Id)
            .Select(hold => new EligibilityHoldSnapshot(
                hold.Code,
                hold.Message,
                hold.BlocksRegistration,
                hold.SourceReference))
            .ToArrayAsync(cancellationToken);

        var attempts = await dbContext.Set<TranscriptAttempt>()
            .AsNoTracking()
            .Where(attempt => attempt.StudentId == student.Id)
            .OrderBy(attempt => attempt.CourseCode)
            .ThenBy(attempt => attempt.Id)
            .ToArrayAsync(cancellationToken);
        var superseded = attempts
            .Where(attempt => attempt.SupersedesAttemptId.HasValue)
            .Select(attempt => attempt.SupersedesAttemptId!.Value)
            .ToHashSet();
        var transcript = attempts
            .Where(attempt => !superseded.Contains(attempt.Id))
            .Select(attempt => new EligibilityTranscriptSnapshot(
                attempt.CourseCode,
                TranscriptStatus(attempt.Status),
                true))
            .ToArray();

        var catalogueContext = await FindCatalogueAsync(
            student,
            evaluatedAtUtc,
            cancellationToken);
        var policy = await FindPolicyAsync(
            termId,
            catalogueContext?.ProgramId,
            requestedCourseIds,
            evaluatedAtUtc,
            cancellationToken);
        var catalogue = catalogueContext is null
            ? null
            : await BuildCatalogueAsync(
                catalogueContext,
                requestedCourseIds,
                policy,
                student.Cohort,
                cancellationToken);

        return new EligibilityAcademicSnapshot(
            student.Id,
            termId,
            student.ProgramCode,
            student.Cohort,
            academicState.GpaAtStart,
            academicState.EarnedCreditsAtStart,
            academicState.StandingAtStart,
            academicState.Version,
            new EligibilityRegistrationWindowSnapshot(
                window is not null &&
                    window.OpensAtUtc <= evaluatedAtUtc &&
                    evaluatedAtUtc < window.ClosesAtUtc,
                window?.Version ?? []),
            holds,
            transcript,
            catalogue,
            policy);
    }

    public async Task<IReadOnlyList<EligibilityOfferingSnapshot>> ListForTermAsync(
        Guid termId,
        CancellationToken cancellationToken = default)
    {
        if (termId == Guid.Empty)
        {
            return [];
        }

        var offerings = await OfferingsForTermQuery(termId)
            .OrderBy(item => item.Id)
            .ToArrayAsync(cancellationToken);
        return await BuildOfferingSnapshotsAsync(offerings, cancellationToken);
    }

    public async Task<EligibilityOfferingSnapshot?> FindAsync(
        Guid offeringId,
        CancellationToken cancellationToken = default)
    {
        if (offeringId == Guid.Empty)
        {
            return null;
        }

        var offering = await dbContext.Set<CourseOffering>()
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == offeringId, cancellationToken);
        if (offering is null)
        {
            return null;
        }

        return (await BuildOfferingSnapshotsAsync(
            [offering],
            cancellationToken)).Single();
    }

    public async Task<CurrentPlanSnapshot> ReadAsync(
        Guid studentId,
        Guid termId,
        CancellationToken cancellationToken = default)
    {
        if (studentId == Guid.Empty || termId == Guid.Empty)
        {
            return CurrentPlanSnapshot.Empty;
        }

        var rows = await (
                from planRow in CurrentPlanForStudentTermQuery(studentId, termId)
                join item in dbContext.Set<RegistrationPlanItem>().AsNoTracking()
                    on planRow.Id equals item.PlanId into planItems
                from item in planItems.DefaultIfEmpty()
                join meeting in dbContext.Set<MeetingSlot>().AsNoTracking()
                    on item.SelectedGroupId equals meeting.GroupId into groupMeetings
                from meeting in groupMeetings.DefaultIfEmpty()
                orderby item.OfferingId,
                    item.SelectedGroupId,
                    meeting.DayOfWeek,
                    meeting.StartLocal,
                    meeting.EndLocal,
                    meeting.Id
                select new
                {
                    planRow.TotalCredits,
                    planRow.Version,
                    OfferingId = item == null ? (Guid?)null : item.OfferingId,
                    GroupId = item == null ? (Guid?)null : item.SelectedGroupId,
                    DayOfWeek = meeting == null
                        ? (DayOfWeek?)null
                        : meeting.DayOfWeek,
                    StartLocal = meeting == null
                        ? (TimeOnly?)null
                        : meeting.StartLocal,
                    EndLocal = meeting == null
                        ? (TimeOnly?)null
                        : meeting.EndLocal
                })
            .ToArrayAsync(cancellationToken);
        if (rows.Length == 0)
        {
            return CurrentPlanSnapshot.Empty;
        }

        var selections = rows
            .Where(row => row.OfferingId.HasValue && row.GroupId.HasValue)
            .GroupBy(row => new
            {
                OfferingId = row.OfferingId!.Value,
                GroupId = row.GroupId!.Value
            })
            .Select(selection => new CurrentPlanSelectionSnapshot(
                selection.Key.OfferingId,
                selection.Key.GroupId,
                selection
                    .Where(row =>
                        row.DayOfWeek.HasValue &&
                        row.StartLocal.HasValue &&
                        row.EndLocal.HasValue)
                    .Select(row => new CurrentPlanMeetingSnapshot(
                        row.DayOfWeek!.Value,
                        row.StartLocal!.Value,
                        row.EndLocal!.Value))
                    .ToArray()))
            .ToArray();
        var plan = rows[0];
        return new(
            plan.TotalCredits,
            selections,
            Convert.ToBase64String(plan.Version));
    }

    private IQueryable<RegistrationPlan> CurrentPlanForStudentTermQuery(
        Guid studentId,
        Guid termId) =>
        dbContext.Set<RegistrationPlan>()
            .AsNoTracking()
            .Where(plan => plan.StudentId == studentId && plan.TermId == termId);

    private IQueryable<CourseOffering> OfferingsForTermQuery(Guid termId) =>
        dbContext.Set<CourseOffering>()
            .AsNoTracking()
            .Where(item => item.TermId == termId);

    private IQueryable<Student> StudentForApplicationUserQuery(
        Guid applicationUserId) =>
        dbContext.Set<Student>()
            .AsNoTracking()
            .Where(item =>
                item.ApplicationUserId == applicationUserId &&
                item.IsActive);

    private async Task<CatalogueContext?> FindCatalogueAsync(
        Student student,
        DateTime evaluatedAtUtc,
        CancellationToken cancellationToken)
    {
        var candidates = await (
                from program in dbContext.Set<AcademicProgram>().AsNoTracking()
                join version in dbContext.Set<CatalogueVersion>().AsNoTracking()
                    on program.CatalogueVersionId equals version.Id
                where program.Code == student.ProgramCode &&
                    program.IsActive &&
                    version.State == CatalogueVersionState.Published &&
                    version.EffectiveFromUtc <= evaluatedAtUtc
                orderby version.EffectiveFromUtc descending, version.Id
                select new CatalogueContext(
                    version.Id,
                    version.VersionCode,
                    version.Version,
                    program.Id))
            .FirstOrDefaultAsync(cancellationToken);
        return candidates;
    }

    private async Task<EligibilityCatalogueSnapshot> BuildCatalogueAsync(
        CatalogueContext context,
        IReadOnlyCollection<Guid> requestedCourseIds,
        EligibilityPolicySnapshot? policy,
        string cohort,
        CancellationToken cancellationToken)
    {
        var curriculumCourseIds = await dbContext.Set<CurriculumCourse>()
            .AsNoTracking()
            .Where(item =>
                item.CatalogueVersionId == context.CatalogueVersionId &&
                item.ProgramId == context.ProgramId &&
                requestedCourseIds.Contains(item.CourseId) &&
                (item.CohortScope == null || item.CohortScope == cohort))
            .Select(item => item.CourseId)
            .Distinct()
            .ToArrayAsync(cancellationToken);
        var courses = await dbContext.Set<Course>()
            .AsNoTracking()
            .Where(item =>
                item.CatalogueVersionId == context.CatalogueVersionId &&
                curriculumCourseIds.Contains(item.Id) &&
                item.IsActive)
            .OrderBy(item => item.Code)
            .ThenBy(item => item.Id)
            .ToArrayAsync(cancellationToken);
        var courseIdSet = courses.Select(item => item.Id).ToArray();
        var prerequisites = await dbContext.Set<CoursePrerequisite>()
            .AsNoTracking()
            .Where(item =>
                item.CatalogueVersionId == context.CatalogueVersionId &&
                courseIdSet.Contains(item.CourseId))
            .OrderBy(item => item.CourseId)
            .ThenBy(item => item.RequiredCourseId)
            .ToArrayAsync(cancellationToken);
        var requiredIds = prerequisites
            .Select(item => item.RequiredCourseId)
            .Distinct()
            .ToArray();
        var requiredCodes = await dbContext.Set<Course>()
            .AsNoTracking()
            .Where(item =>
                item.CatalogueVersionId == context.CatalogueVersionId &&
                requiredIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, item => item.Code, cancellationToken);
        var policyValues = policy?.Rules
            .ToDictionary(item => item.TypeKey, item => item.Value, StringComparer.Ordinal)
            ?? [];

        var snapshots = courses.Select(course =>
        {
            var isProject = string.Equals(
                course.Code,
                "DS413",
                StringComparison.OrdinalIgnoreCase);
            return new EligibilityCourseSnapshot(
                course.Id,
                course.Code,
                course.Title,
                course.Credits,
                prerequisites
                    .Where(item => item.CourseId == course.Id)
                    .Select(item => requiredCodes.GetValueOrDefault(
                        item.RequiredCourseId,
                        item.RequiredCourseId.ToString("D")))
                    .Order(StringComparer.Ordinal)
                    .ToArray(),
                isProject
                    ? ParseDecimal(policyValues.GetValueOrDefault("MinimumGpa"))
                    : null,
                isProject
                    ? ParseDecimal(policyValues.GetValueOrDefault(
                        "MinimumEarnedCredits"))
                    : null,
                course.Provenance.SourceReference,
                course.Provenance.AccessedOn);
        }).ToArray();

        return new EligibilityCatalogueSnapshot(
            context.CatalogueVersionId,
            context.VersionCode,
            context.RowVersion,
            snapshots);
    }

    private async Task<EligibilityPolicySnapshot?> FindPolicyAsync(
        Guid termId,
        Guid? programId,
        IReadOnlyCollection<Guid> requestedCourseIds,
        DateTime evaluatedAtUtc,
        CancellationToken cancellationToken)
    {
        var policies = await dbContext.Set<PolicySet>()
            .AsNoTracking()
            .Where(item =>
                item.TermId == termId &&
                item.State == PolicySetState.Published &&
                item.EffectiveFromUtc <= evaluatedAtUtc &&
                (item.EffectiveToUtc == null ||
                    evaluatedAtUtc < item.EffectiveToUtc) &&
                (item.ProgramId == null || item.ProgramId == programId))
            .OrderByDescending(item => item.ProgramId.HasValue)
            .ThenByDescending(item => item.EffectiveFromUtc)
            .ThenBy(item => item.Id)
            .ToArrayAsync(cancellationToken);
        var policy = policies.FirstOrDefault();
        if (policy is null)
        {
            return null;
        }

        var rules = await dbContext.Set<PolicyRule>()
            .AsNoTracking()
            .Where(item => item.PolicySetId == policy.Id)
            .OrderBy(item => item.Code)
            .ThenBy(item => item.Id)
            .ToArrayAsync(cancellationToken);
        var byCode = rules.ToDictionary(item => item.Code, StringComparer.Ordinal);
        if (RequiredPolicyCodes.Any(code => !byCode.ContainsKey(code)))
        {
            return null;
        }

        var requestedCodes = requestedCourseIds.Count == 0
            ? []
            : await dbContext.Set<Course>()
                .AsNoTracking()
                .Where(item => requestedCourseIds.Contains(item.Id))
                .Select(item => item.Code)
                .ToArrayAsync(cancellationToken);
        var projectRequested = requestedCodes.Contains(
            "DS413",
            StringComparer.OrdinalIgnoreCase);
        if (projectRequested &&
            (!byCode.ContainsKey("DS413_MIN_GPA") ||
                !byCode.ContainsKey("DS413_MIN_EARNED_CREDITS")))
        {
            return null;
        }

        var accessedOn = DateOnly.FromDateTime(policy.EffectiveFromUtc);
        EligibilityPolicyRuleSnapshot From(
            string type,
            string code,
            string reasonCode) =>
            new(
                type,
                reasonCode,
                byCode[code].SourceReference,
                accessedOn,
                byCode[code].Value);
        EligibilityPolicyRuleSnapshot Governed(
            string type,
            string reasonCode,
            string value) =>
            new(
                type,
                reasonCode,
                DemoApprovalReference,
                accessedOn,
                value);

        var snapshots = new List<EligibilityPolicyRuleSnapshot>
        {
            From(
                "RegistrationWindow",
                "REGISTRATION_WINDOW_OPEN",
                "REGISTRATION_WINDOW_CLOSED"),
            From(
                "AcademicStanding",
                "ACADEMIC_STANDING_ALLOWED",
                "ACADEMIC_STANDING_UNAVAILABLE"),
            Governed("BlockingHold", "REGISTRATION_HOLD", "false"),
            From(
                "Prerequisite",
                "PREREQUISITES_REQUIRED",
                "PREREQUISITE_NOT_COMPLETED"),
            From(
                "CreditLoad",
                "NORMAL_MAX_CREDITS",
                "LOAD_ABOVE_NORMAL_MAXIMUM"),
            From(
                "ProbationLoad",
                "PROBATION_MAX_CREDITS",
                "PROBATION_LOAD_EXCEEDED"),
            Governed(
                "RepeatEligibility",
                "REPEAT_POLICY_UNAVAILABLE",
                "unavailable"),
            From("Capacity", "CAPACITY_REQUIRED", "GROUP_FULL"),
            From("MeetingConflict", "CONFLICT_BLOCKED", "MEETING_CONFLICT"),
        };
        if (byCode.TryGetValue("DS413_MIN_GPA", out var minimumGpa))
        {
            snapshots.Add(new(
                "MinimumGpa",
                minimumGpa.ReasonCode,
                minimumGpa.SourceReference,
                accessedOn,
                minimumGpa.Value));
        }

        if (byCode.TryGetValue(
                "DS413_MIN_EARNED_CREDITS",
                out var minimumEarnedCredits))
        {
            snapshots.Add(new(
                "MinimumEarnedCredits",
                minimumEarnedCredits.ReasonCode,
                minimumEarnedCredits.SourceReference,
                accessedOn,
                minimumEarnedCredits.Value));
        }

        return new EligibilityPolicySnapshot(
            policy.Id,
            policy.VersionCode,
            policy.EffectiveFromUtc,
            policy.EffectiveToUtc,
            DemoApprovedBy,
            DemoApprovalReference,
            policy.VersionToken,
            snapshots);
    }

    private async Task<IReadOnlyList<EligibilityOfferingSnapshot>>
        BuildOfferingSnapshotsAsync(
            IReadOnlyCollection<CourseOffering> offerings,
            CancellationToken cancellationToken)
    {
        var offeringIds = offerings.Select(item => item.Id).ToArray();
        var groups = await dbContext.Set<SectionGroup>()
            .AsNoTracking()
            .Where(item => offeringIds.Contains(item.OfferingId))
            .OrderBy(item => item.OfferingId)
            .ThenBy(item => item.GroupCode)
            .ThenBy(item => item.Id)
            .ToArrayAsync(cancellationToken);
        var groupIds = groups.Select(item => item.Id).ToArray();
        var meetings = await dbContext.Set<MeetingSlot>()
            .AsNoTracking()
            .Where(item => groupIds.Contains(item.GroupId))
            .OrderBy(item => item.GroupId)
            .ThenBy(item => item.DayOfWeek)
            .ThenBy(item => item.StartLocal)
            .ThenBy(item => item.Id)
            .ToArrayAsync(cancellationToken);
        var roomIds = meetings.Select(item => item.RoomId).Distinct().ToArray();
        var rooms = await dbContext.Set<Room>()
            .AsNoTracking()
            .Where(item => roomIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, cancellationToken);
        var meetingIds = meetings.Select(item => item.Id).ToArray();
        var assignments = await dbContext.Set<GroupStaffAssignment>()
            .AsNoTracking()
            .Where(item => meetingIds.Contains(item.MeetingSlotId))
            .OrderBy(item => item.MeetingSlotId)
            .ThenBy(item => item.TeachingRole)
            .ThenBy(item => item.StaffId)
            .ToArrayAsync(cancellationToken);
        var staffIds = assignments.Select(item => item.StaffId).Distinct().ToArray();
        var staff = await dbContext.Set<Staff>()
            .AsNoTracking()
            .Where(item => staffIds.Contains(item.Id) && item.IsActive)
            .ToDictionaryAsync(item => item.Id, cancellationToken);

        return offerings
            .OrderBy(item => item.Id)
            .Select(offering => new EligibilityOfferingSnapshot(
                offering.Id,
                offering.TermId,
                offering.CourseId,
                OfferingState(offering.State),
                offering.Version,
                groups
                    .Where(group => group.OfferingId == offering.Id)
                    .Select(group => new EligibilityGroupSnapshot(
                        group.Id,
                        group.GroupCode,
                        GroupState(group.State),
                        group.RegistrationPaused,
                        group.Capacity,
                        group.EnrolledCount,
                        group.Version,
                        meetings
                            .Where(meeting => meeting.GroupId == group.Id)
                            .Select(meeting =>
                            {
                                rooms.TryGetValue(meeting.RoomId, out var room);
                                return new EligibilityMeetingSnapshot(
                                    meeting.Id,
                                    meeting.ActivityType.ToString(),
                                    meeting.DayOfWeek,
                                    meeting.StartLocal,
                                    meeting.EndLocal,
                                    room?.Code ?? "Unavailable",
                                    room?.Location ?? "Unavailable",
                                    room?.AvailabilityState ==
                                        RoomAvailabilityState.Available,
                                    assignments
                                        .Where(item =>
                                            item.MeetingSlotId == meeting.Id &&
                                            staff.ContainsKey(item.StaffId))
                                        .Select(item =>
                                            new EligibilityMeetingStaffSnapshot(
                                                item.StaffId,
                                                item.TeachingRole.ToString(),
                                                staff[item.StaffId].DisplayName))
                                        .ToArray());
                            })
                            .ToArray()))
                    .ToArray()))
            .ToArray();
    }

    private static bool AppliesTo(RegistrationWindow window, Student student) =>
        window.ScopeType switch
        {
            RegistrationWindowScopeType.AllStudents => true,
            RegistrationWindowScopeType.Program => string.Equals(
                window.ScopeValue,
                student.ProgramCode,
                StringComparison.OrdinalIgnoreCase),
            RegistrationWindowScopeType.Cohort => string.Equals(
                window.ScopeValue,
                student.Cohort,
                StringComparison.OrdinalIgnoreCase),
            _ => false,
        };

    private static int ScopePriority(RegistrationWindowScopeType scope) =>
        scope switch
        {
            RegistrationWindowScopeType.Cohort => 3,
            RegistrationWindowScopeType.Program => 2,
            RegistrationWindowScopeType.AllStudents => 1,
            _ => 0,
        };

    private static decimal? ParseDecimal(string? value) =>
        decimal.TryParse(
            value,
            System.Globalization.NumberStyles.Number,
            System.Globalization.CultureInfo.InvariantCulture,
            out var parsed)
            ? parsed
            : null;

    private static string TranscriptStatus(TranscriptAttemptStatus state) =>
        state.ToString().ToLowerInvariant();

    private static string OfferingState(CourseOfferingState state) =>
        state.ToString().ToLowerInvariant();

    private static string GroupState(SectionGroupState state) =>
        state.ToString().ToLowerInvariant();

    private sealed record CatalogueContext(
        Guid CatalogueVersionId,
        string VersionCode,
        byte[] RowVersion,
        Guid ProgramId);
}

public static class RegistrationDiscoverySqlServerRegistration
{
    public static IServiceCollection AddStudentRegistrationDiscoverySqlServer(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<RegistrationDiscoveryQueryAdapter>();
        services.TryAddScoped<IEligibilityAcademicReader>(Resolve);
        services.TryAddScoped<IEligibilityOfferingReader>(Resolve);
        services.TryAddScoped<ICurrentPlanReader>(Resolve);
        return services;
    }

    private static RegistrationDiscoveryQueryAdapter Resolve(
        IServiceProvider services) =>
        services.GetRequiredService<RegistrationDiscoveryQueryAdapter>();
}
