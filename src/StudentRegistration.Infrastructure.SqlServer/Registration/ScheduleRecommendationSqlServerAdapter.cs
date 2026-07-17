using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StudentRegistration.Academics.Domain;
using StudentRegistration.IdentityAccess.Domain;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Registration.Application;
using StudentRegistration.Registration.Application.Ports;
using StudentRegistration.Registration.Domain;
using StudentRegistration.Scheduling.Domain;
using CatalogueProgram = StudentRegistration.Academics.Domain.Program;

namespace StudentRegistration.Infrastructure.SqlServer.Registration;

public sealed class ScheduleRecommendationSqlServerAdapter(
    StudentRegistrationDbContext dbContext,
    TimeProvider timeProvider) :
    IRecommendationSnapshotReader,
    IRecommendationPlanWriter
{
    public async Task<RecommendationSnapshotResult> ReadAsync(
        Guid studentId,
        Guid termId,
        string expectedPlanRowVersion,
        DateTime evaluatedAtUtc,
        CancellationToken cancellationToken = default)
    {
        if (studentId == Guid.Empty ||
            termId == Guid.Empty ||
            string.IsNullOrWhiteSpace(expectedPlanRowVersion) ||
            evaluatedAtUtc.Kind is not DateTimeKind.Utc)
        {
            return new(RecommendationSnapshotOutcome.NotFound);
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.RepeatableRead,
            cancellationToken);
        try
        {
            var plan = await PlanQuery(studentId, termId)
                .AsNoTracking()
                .SingleOrDefaultAsync(cancellationToken);
            if (plan is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return new(RecommendationSnapshotOutcome.NotFound);
            }

            if (!string.Equals(
                    expectedPlanRowVersion.Trim(),
                    RowVersion(plan),
                    StringComparison.Ordinal))
            {
                await transaction.RollbackAsync(cancellationToken);
                return new(RecommendationSnapshotOutcome.PlanChanged);
            }

            var dependency = await ReadDependencyAsync(
                studentId,
                termId,
                evaluatedAtUtc,
                cancellationToken);
            if (dependency is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return new(RecommendationSnapshotOutcome.Unavailable);
            }

            var offeringIds = plan.Items
                .Select(item => item.OfferingId)
                .Distinct()
                .ToArray();
            var selectedCourseIds = await dbContext.Set<CourseOffering>()
                .AsNoTracking()
                .Where(offering =>
                    offeringIds.Contains(offering.Id) &&
                    offering.TermId == termId)
                .Select(offering => offering.CourseId)
                .Distinct()
                .Order()
                .ToArrayAsync(cancellationToken);
            var groups = await ReadGroupsAsync(
                offeringIds,
                termId,
                dependency.Timezone,
                cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var candidates = groups.Select(group =>
                new ScheduleCandidateGroup(
                    group.CourseId,
                    group.Snapshot.OfferingId,
                    group.Snapshot.GroupId,
                    group.Snapshot.Credits,
                    group.Snapshot.State,
                    IsViable(group.Snapshot),
                    group.Snapshot.Meetings.Select(meeting =>
                        new ScheduleCandidateMeeting(
                            meeting.DayOfWeek,
                            meeting.StartLocal,
                            meeting.EndLocal,
                            meeting.Location))
                        .ToArray()))
                .ToArray();
            var offeringVersions = groups
                .GroupBy(group => group.Snapshot.OfferingId)
                .ToDictionary(
                    group => group.Key.ToString("N"),
                    group => group.First().Snapshot.OfferingVersion,
                    StringComparer.Ordinal);
            var groupVersions = groups.ToDictionary(
                group => group.Snapshot.GroupId.ToString("N"),
                group => group.Snapshot.GroupVersion,
                StringComparer.Ordinal);

            return new(
                RecommendationSnapshotOutcome.Captured,
                new(
                    studentId,
                    termId,
                    plan.Id,
                    RowVersion(plan),
                    dependency.AcademicContextVersion,
                    dependency.CatalogueVersion,
                    dependency.PolicySetId,
                    dependency.PolicyVersion,
                    offeringVersions,
                    groupVersions,
                    RegistrationPlanService.DefaultTargetCredits,
                    selectedCourseIds,
                    offeringIds.Order().ToArray(),
                    candidates,
                    groups.Select(group => group.Snapshot).ToArray()));
        }
        catch (Exception exception)
            when (exception is DbUpdateException or InvalidOperationException)
        {
            await transaction.RollbackAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();
            return new(RecommendationSnapshotOutcome.Unavailable);
        }
    }

    public async Task<RecommendationPlanWriteResult> ReplaceAsync(
        RecommendationPlanReplacement replacement,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(replacement);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        try
        {
            var plan = await PlanQuery(replacement.StudentId, replacement.TermId)
                .AsTracking()
                .SingleOrDefaultAsync(cancellationToken);
            if (plan is null ||
                plan.Id != replacement.PlanId ||
                !string.Equals(
                    RowVersion(plan),
                    replacement.ExpectedPlanRowVersion,
                    StringComparison.Ordinal))
            {
                await transaction.RollbackAsync(cancellationToken);
                return new(RecommendationPlanWriteOutcome.PlanChanged);
            }

            var dependency = await ReadDependencyAsync(
                replacement.StudentId,
                replacement.TermId,
                timeProvider.GetUtcNow().UtcDateTime,
                cancellationToken);
            if (dependency is null ||
                !MatchesDependency(replacement, dependency))
            {
                await transaction.RollbackAsync(cancellationToken);
                return new(RecommendationPlanWriteOutcome.StaleInput);
            }

            var selectedGroupIds = replacement.Selections
                .Select(selection => selection.GroupId)
                .Distinct()
                .ToArray();
            if (selectedGroupIds.Length != replacement.Selections.Count)
            {
                await transaction.RollbackAsync(cancellationToken);
                return new(RecommendationPlanWriteOutcome.InvalidSelection);
            }

            var selected = await ReadGroupsByIdsAsync(
                selectedGroupIds,
                replacement.TermId,
                cancellationToken);
            var capturedGroups = await ReadVersionMapsAsync(
                replacement,
                cancellationToken);
            if (selected.Count != replacement.Selections.Count ||
                capturedGroups is null ||
                !selected.All(group => IsViable(group.Snapshot)) ||
                selected.Select(group => group.Snapshot.OfferingId).Distinct().Count() !=
                    selected.Count ||
                selected.Sum(group => group.Snapshot.Credits) !=
                    RegistrationPlanService.DefaultTargetCredits ||
                HasOverlap(selected.Select(group => group.Snapshot)) ||
                !SameSelectedCourseSet(plan, replacement, selected) ||
                !MapsMatch(replacement, capturedGroups))
            {
                await transaction.RollbackAsync(cancellationToken);
                return new(RecommendationPlanWriteOutcome.StaleInput);
            }

            var validation = new ValidationSnapshot(
                timeProvider.GetUtcNow().UtcDateTime,
                dependency.AcademicContextVersion,
                dependency.PolicyVersion,
                dependency.CatalogueVersion,
                selected.ToDictionary(
                    group => group.Snapshot.OfferingId,
                    group => group.Snapshot.OfferingVersion),
                selected.ToDictionary(
                    group => group.Snapshot.GroupId,
                    group => group.Snapshot.GroupVersion));
            var selections = selected.Select(group =>
                new RegistrationPlanSelection(
                    Guid.NewGuid(),
                    group.Snapshot.OfferingId,
                    group.Snapshot.GroupId,
                    group.Snapshot.OfferingVersion,
                    group.Snapshot.GroupVersion))
                .ToArray();
            plan.ReplaceSelections(
                selections,
                RegistrationPlanService.DefaultTargetCredits,
                RegistrationPlanState.Draft,
                [],
                validation);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            var view = new RegistrationPlanView(
                plan.Id,
                replacement.TermId,
                RowVersion(plan),
                selected.Select(ToSelectedGroup).ToArray(),
                RegistrationPlanService.DefaultTargetCredits,
                RegistrationPlanService.DefaultTargetCredits,
                RegistrationPlanService.MaximumAllowedCredits,
                [
                    new(
                        "LOAD_WITHIN_MAXIMUM",
                        false,
                        "The selected load is within the approved 18-credit maximum.",
                        "18",
                        "18",
                        dependency.PolicySetId,
                        dependency.PolicyVersion,
                        "SPEC-013")
                ],
                [],
                [],
                validation,
                false);
            return new(RecommendationPlanWriteOutcome.Updated, view);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();
            return new(RecommendationPlanWriteOutcome.PlanChanged);
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();
            return new(RecommendationPlanWriteOutcome.Unavailable);
        }
    }

    private async Task<DependencySnapshot?> ReadDependencyAsync(
        Guid studentId,
        Guid termId,
        DateTime evaluatedAtUtc,
        CancellationToken cancellationToken)
    {
        var academic = await (
                from student in dbContext.Set<Student>().AsNoTracking()
                join state in dbContext.Set<StudentTermAcademicState>().AsNoTracking()
                    on new { StudentId = student.Id, TermId = termId }
                    equals new { state.StudentId, state.TermId }
                join term in dbContext.Set<AcademicTerm>().AsNoTracking()
                    on state.TermId equals term.Id
                where student.Id == studentId && student.IsActive
                select new
                {
                    student.ProgramCode,
                    AcademicContextVersion = state.Version,
                    Timezone = term.TimeZoneId
                })
            .SingleOrDefaultAsync(cancellationToken);
        if (academic is null)
        {
            return null;
        }

        var policy = await dbContext.Set<PolicySet>()
            .AsNoTracking()
            .Where(policySet =>
                policySet.TermId == termId &&
                policySet.State == PolicySetState.Published &&
                policySet.EffectiveFromUtc <= evaluatedAtUtc &&
                (policySet.EffectiveToUtc == null ||
                 evaluatedAtUtc < policySet.EffectiveToUtc))
            .OrderByDescending(policySet => policySet.EffectiveFromUtc)
            .ThenBy(policySet => policySet.Id)
            .Select(policySet => new
            {
                policySet.Id,
                policySet.VersionCode
            })
            .FirstOrDefaultAsync(cancellationToken);
        var catalogueVersion = await (
                from program in dbContext.Set<CatalogueProgram>().AsNoTracking()
                join version in dbContext.Set<CatalogueVersion>().AsNoTracking()
                    on program.CatalogueVersionId equals version.Id
                where program.Code == academic.ProgramCode &&
                    program.IsActive &&
                    version.State == CatalogueVersionState.Published &&
                    version.EffectiveFromUtc <= evaluatedAtUtc
                orderby version.EffectiveFromUtc descending, version.Id
                select version.VersionCode)
            .FirstOrDefaultAsync(cancellationToken);
        return policy is null || catalogueVersion is null
            ? null
            : new(
                Convert.ToBase64String(academic.AcademicContextVersion),
                catalogueVersion,
                policy.Id,
                policy.VersionCode,
                academic.Timezone);
    }

    private async Task<IReadOnlyList<CandidateSnapshot>> ReadGroupsAsync(
        IReadOnlyCollection<Guid> offeringIds,
        Guid termId,
        string timezone,
        CancellationToken cancellationToken)
    {
        var groupIds = await (
                from sectionGroup in dbContext.Set<SectionGroup>().AsNoTracking()
                join offering in dbContext.Set<CourseOffering>().AsNoTracking()
                    on sectionGroup.OfferingId equals offering.Id
                where offeringIds.Contains(sectionGroup.OfferingId) &&
                    offering.TermId == termId
                select sectionGroup.Id)
            .ToArrayAsync(cancellationToken);
        return await ReadGroupsByIdsAsync(
            groupIds,
            termId,
            cancellationToken,
            timezone);
    }

    private async Task<IReadOnlyList<CandidateSnapshot>> ReadGroupsByIdsAsync(
        IReadOnlyCollection<Guid> groupIds,
        Guid termId,
        CancellationToken cancellationToken,
        string timezone = "UTC")
    {
        var rows = await (
                from sectionGroup in dbContext.Set<SectionGroup>().AsNoTracking()
                join offering in dbContext.Set<CourseOffering>().AsNoTracking()
                    on sectionGroup.OfferingId equals offering.Id
                join course in dbContext.Set<Course>().AsNoTracking()
                    on offering.CourseId equals course.Id
                join meeting in dbContext.Set<MeetingSlot>().AsNoTracking()
                    on sectionGroup.Id equals meeting.GroupId into meetings
                from meeting in meetings.DefaultIfEmpty()
                where groupIds.Contains(sectionGroup.Id) &&
                    offering.TermId == termId
                orderby offering.Id, sectionGroup.Id, meeting.DayOfWeek,
                    meeting.StartLocal, meeting.EndLocal, meeting.Id
                select new GroupRow(
                    course.Id,
                    offering.Id,
                    sectionGroup.Id,
                    sectionGroup.GroupCode,
                    course.Code,
                    course.Title,
                    course.Credits,
                    sectionGroup.State,
                    sectionGroup.RegistrationPaused,
                    sectionGroup.Capacity,
                    sectionGroup.EnrolledCount,
                    offering.Version,
                    sectionGroup.Version,
                    meeting == null ? null : meeting.Id,
                    meeting == null ? null : meeting.DayOfWeek,
                    meeting == null ? null : meeting.StartLocal,
                    meeting == null ? null : meeting.EndLocal))
            .ToArrayAsync(cancellationToken);

        return rows
            .GroupBy(row => row.GroupId)
            .Select(groupRows =>
            {
                var first = groupRows.First();
                return new CandidateSnapshot(
                    first.CourseId,
                    new(
                        first.OfferingId,
                        first.GroupId,
                        first.GroupCode,
                        first.CourseCode,
                        first.CourseTitle,
                        first.Credits,
                        first.GroupState.ToString().ToLowerInvariant(),
                        first.RegistrationPaused,
                        first.Capacity,
                        first.EnrolledCount,
                        Convert.ToBase64String(first.OfferingVersion),
                        Convert.ToBase64String(first.GroupVersion),
                        groupRows
                            .Where(row =>
                                row.MeetingId.HasValue &&
                                row.DayOfWeek.HasValue &&
                                row.StartLocal.HasValue &&
                                row.EndLocal.HasValue)
                            .Select(row => new RegistrationPlanMeetingSnapshot(
                                row.MeetingId!.Value,
                                row.DayOfWeek!.Value,
                                row.StartLocal!.Value,
                                row.EndLocal!.Value,
                                "Class",
                                "Unavailable",
                                "Unavailable",
                                null,
                                [],
                                timezone))
                            .ToArray()));
            })
            .ToArray();
    }

    private async Task<VersionMaps?> ReadVersionMapsAsync(
        RecommendationPlanReplacement replacement,
        CancellationToken cancellationToken)
    {
        if (!TryIds(replacement.OfferingVersions.Keys, out var offeringIds) ||
            !TryIds(replacement.GroupVersions.Keys, out var groupIds))
        {
            return null;
        }

        var offerings = await dbContext.Set<CourseOffering>()
            .AsNoTracking()
            .Where(offering => offeringIds.Contains(offering.Id))
            .ToDictionaryAsync(
                offering => offering.Id.ToString("N"),
                offering => Convert.ToBase64String(offering.Version),
                StringComparer.Ordinal,
                cancellationToken);
        var groups = await dbContext.Set<SectionGroup>()
            .AsNoTracking()
            .Where(group => groupIds.Contains(group.Id))
            .ToDictionaryAsync(
                group => group.Id.ToString("N"),
                group => Convert.ToBase64String(group.Version),
                StringComparer.Ordinal,
                cancellationToken);
        return new(offerings, groups);
    }

    private static bool TryIds(
        IEnumerable<string> values,
        out Guid[] identifiers)
    {
        var parsed = new List<Guid>();
        foreach (var value in values)
        {
            if (!Guid.TryParse(value, out var identifier) ||
                identifier == Guid.Empty)
            {
                identifiers = [];
                return false;
            }

            parsed.Add(identifier);
        }

        identifiers = parsed.Distinct().ToArray();
        return identifiers.Length == parsed.Count;
    }

    private static bool SameSelectedCourseSet(
        RegistrationPlan plan,
        RecommendationPlanReplacement replacement,
        IReadOnlyList<CandidateSnapshot> selected)
    {
        var existingOfferings = plan.Items
            .Select(item => item.OfferingId)
            .Order()
            .ToArray();
        var replacementOfferings = replacement.Selections
            .Select(selection => selection.OfferingId)
            .Order()
            .ToArray();
        return existingOfferings.SequenceEqual(replacementOfferings) &&
            replacement.Selections
                .OrderBy(selection => selection.OfferingId)
                .Select(selection => selection.CourseId)
                .SequenceEqual(
                    selected
                        .OrderBy(group => group.Snapshot.OfferingId)
                        .Select(group => group.CourseId));
    }

    private static bool MatchesDependency(
        RecommendationPlanReplacement replacement,
        DependencySnapshot dependency) =>
        string.Equals(
            replacement.AcademicContextVersion,
            dependency.AcademicContextVersion,
            StringComparison.Ordinal) &&
        string.Equals(
            replacement.CatalogueVersion,
            dependency.CatalogueVersion,
            StringComparison.Ordinal) &&
        replacement.PolicySetId == dependency.PolicySetId &&
        string.Equals(
            replacement.PolicyVersion,
            dependency.PolicyVersion,
            StringComparison.Ordinal) &&
        string.Equals(
            replacement.OptimizerConfigurationVersion,
            "1.0.0",
            StringComparison.Ordinal);

    private static bool MapsMatch(
        RecommendationPlanReplacement replacement,
        VersionMaps current) =>
        DictionaryEqual(replacement.OfferingVersions, current.OfferingVersions) &&
        DictionaryEqual(replacement.GroupVersions, current.GroupVersions);

    private static bool DictionaryEqual(
        IReadOnlyDictionary<string, string> expected,
        IReadOnlyDictionary<string, string> current) =>
        expected.Count == current.Count &&
        expected.All(pair =>
            current.TryGetValue(pair.Key, out var value) &&
            string.Equals(pair.Value, value, StringComparison.Ordinal));

    private static bool IsViable(RegistrationPlanGroupSnapshot group) =>
        string.Equals(group.State, "published", StringComparison.Ordinal) &&
        !group.RegistrationPaused &&
        group.EnrolledCount < group.Capacity;

    private static RegistrationPlanSelectedGroup ToSelectedGroup(
        CandidateSnapshot candidate) =>
        new(
            candidate.Snapshot.OfferingId,
            candidate.Snapshot.GroupId,
            candidate.Snapshot.GroupCode,
            candidate.Snapshot.CourseCode,
            candidate.Snapshot.SubjectTitle,
            candidate.Snapshot.Credits,
            candidate.Snapshot.Capacity,
            candidate.Snapshot.EnrolledCount,
            candidate.Snapshot.OfferingVersion,
            candidate.Snapshot.GroupVersion,
            candidate.Snapshot.Meetings);

    private static bool HasOverlap(
        IEnumerable<RegistrationPlanGroupSnapshot> groups)
    {
        var meetings = groups
            .SelectMany(group => group.Meetings.Select(meeting =>
                (group.GroupId, Meeting: meeting)))
            .ToArray();
        for (var first = 0; first < meetings.Length; first++)
        {
            for (var second = first + 1; second < meetings.Length; second++)
            {
                if (meetings[first].GroupId != meetings[second].GroupId &&
                    meetings[first].Meeting.DayOfWeek ==
                        meetings[second].Meeting.DayOfWeek &&
                    meetings[first].Meeting.StartLocal <
                        meetings[second].Meeting.EndLocal &&
                    meetings[second].Meeting.StartLocal <
                        meetings[first].Meeting.EndLocal)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private IQueryable<RegistrationPlan> PlanQuery(Guid studentId, Guid termId) =>
        dbContext.Set<RegistrationPlan>()
            .Include(plan => plan.Items)
            .Where(plan => plan.StudentId == studentId && plan.TermId == termId);

    private static string RowVersion(RegistrationPlan plan) =>
        plan.Version.Length == 0
            ? RegistrationPlanService.InitialEmptyVersion
            : Convert.ToBase64String(plan.Version);

    private sealed record DependencySnapshot(
        string AcademicContextVersion,
        string CatalogueVersion,
        Guid PolicySetId,
        string PolicyVersion,
        string Timezone);

    private sealed record CandidateSnapshot(
        Guid CourseId,
        RegistrationPlanGroupSnapshot Snapshot);

    private sealed record VersionMaps(
        IReadOnlyDictionary<string, string> OfferingVersions,
        IReadOnlyDictionary<string, string> GroupVersions);

    private sealed record GroupRow(
        Guid CourseId,
        Guid OfferingId,
        Guid GroupId,
        string GroupCode,
        string CourseCode,
        string CourseTitle,
        decimal Credits,
        SectionGroupState GroupState,
        bool RegistrationPaused,
        int Capacity,
        int EnrolledCount,
        byte[] OfferingVersion,
        byte[] GroupVersion,
        Guid? MeetingId,
        DayOfWeek? DayOfWeek,
        TimeOnly? StartLocal,
        TimeOnly? EndLocal);
}

public static class ScheduleRecommendationSqlServerRegistration
{
    public static IServiceCollection AddScheduleRecommendationsSqlServer(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddScoped<ScheduleRecommendationSqlServerAdapter>();
        services.TryAddScoped<IRecommendationSnapshotReader>(Resolve);
        services.TryAddScoped<IRecommendationPlanWriter>(Resolve);
        return services;
    }

    private static ScheduleRecommendationSqlServerAdapter Resolve(
        IServiceProvider services) =>
        services.GetRequiredService<ScheduleRecommendationSqlServerAdapter>();
}
