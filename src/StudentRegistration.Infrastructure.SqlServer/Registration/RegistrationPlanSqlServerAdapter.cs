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

public sealed class RegistrationPlanSqlServerAdapter(
    StudentRegistrationDbContext dbContext) :
    IRegistrationPlanOwnerReader,
    IRegistrationPlanStore,
    IRegistrationPlanContextReader
{
    public async Task<Guid?> ResolveStudentIdAsync(
        Guid applicationUserId,
        Guid termId,
        CancellationToken cancellationToken = default)
    {
        if (applicationUserId == Guid.Empty || termId == Guid.Empty)
        {
            return null;
        }

        return await (
                from student in dbContext.Set<Student>().AsNoTracking()
                join state in dbContext.Set<StudentTermAcademicState>().AsNoTracking()
                    on new { StudentId = student.Id, TermId = termId }
                    equals new { state.StudentId, state.TermId }
                join term in dbContext.Set<AcademicTerm>().AsNoTracking()
                    on state.TermId equals term.Id
                where student.ApplicationUserId == applicationUserId && student.IsActive
                select (Guid?)student.Id)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<RegistrationPlan?> ReadAsync(
        Guid studentId,
        Guid termId,
        CancellationToken cancellationToken = default) =>
        PlanQuery(studentId, termId)
            .AsNoTrackingWithIdentityResolution()
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<RegistrationPlanStoreResult> ReplaceAsync(
        RegistrationPlanStoreCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        try
        {
            var plan = await PlanQuery(command.StudentId, command.TermId)
                .AsTracking()
                .SingleOrDefaultAsync(cancellationToken);
            var currentVersion = plan is null || plan.Version.Length == 0
                ? RegistrationPlanService.InitialEmptyVersion
                : Convert.ToBase64String(plan.Version);
            if (!string.Equals(
                command.ExpectedRowVersion,
                currentVersion,
                StringComparison.Ordinal))
            {
                await transaction.RollbackAsync(cancellationToken);
                dbContext.ChangeTracker.Clear();
                return new(
                    RegistrationPlanStoreOutcome.StaleVersion,
                    await ReadAsync(command.StudentId, command.TermId, cancellationToken));
            }

            plan ??= new RegistrationPlan(
                Guid.NewGuid(),
                command.StudentId,
                command.TermId,
                0m,
                RegistrationPlanState.Draft);
            if (dbContext.Entry(plan).State is EntityState.Detached)
            {
                dbContext.Add(plan);
            }

            plan.ReplaceSelections(
                command.Selections,
                command.TotalCredits,
                command.State,
                command.Conflicts,
                command.Validation);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new(RegistrationPlanStoreOutcome.Updated, plan);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();
            return new(
                RegistrationPlanStoreOutcome.StaleVersion,
                await ReadAsync(command.StudentId, command.TermId, cancellationToken));
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();
            var current = await ReadAsync(
                command.StudentId,
                command.TermId,
                cancellationToken);
            return current is null
                ? new(RegistrationPlanStoreOutcome.StorageUnavailable)
                : new(RegistrationPlanStoreOutcome.StaleVersion, current);
        }
    }

    public async Task<RegistrationPlanContextSnapshot?> ReadAsync(
        Guid studentId,
        Guid termId,
        IReadOnlyCollection<Guid> groupIds,
        DateTime evaluatedAtUtc,
        CancellationToken cancellationToken = default)
    {
        if (studentId == Guid.Empty ||
            termId == Guid.Empty ||
            groupIds is null ||
            evaluatedAtUtc.Kind is not DateTimeKind.Utc)
        {
            return null;
        }

        var requestedGroupIds = groupIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .Take(100)
            .ToArray();
        await using var transaction = dbContext.Database.CurrentTransaction is null
            ? await dbContext.Database.BeginTransactionAsync(
                IsolationLevel.RepeatableRead,
                cancellationToken)
            : null;

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
                    state.Version,
                    term.TimeZoneId
                })
            .SingleOrDefaultAsync(cancellationToken);
        if (academic is null)
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(cancellationToken);
            }
            return null;
        }

        var policy = await (
                from policySet in dbContext.Set<PolicySet>().AsNoTracking()
                join rule in dbContext.Set<PolicyRule>().AsNoTracking()
                    on policySet.Id equals rule.PolicySetId
                where policySet.TermId == termId &&
                    policySet.State == PolicySetState.Published &&
                    policySet.EffectiveFromUtc <= evaluatedAtUtc &&
                    (policySet.EffectiveToUtc == null ||
                     evaluatedAtUtc < policySet.EffectiveToUtc) &&
                    rule.Code == "NORMAL_MAX_CREDITS" &&
                    rule.Value == "18"
                orderby policySet.EffectiveFromUtc descending, policySet.Id
                select new
                {
                    policySet.Id,
                    policySet.VersionCode,
                    policySet.ScopeCode,
                    rule.SourceReference
                })
            .FirstOrDefaultAsync(cancellationToken);
        if (policy is null)
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(cancellationToken);
            }
            return null;
        }

        var catalogueVersion = await (
                from program in dbContext.Set<CatalogueProgram>().AsNoTracking()
                join version in dbContext.Set<CatalogueVersion>().AsNoTracking()
                    on program.CatalogueVersionId equals version.Id
                where program.Code == academic.ProgramCode &&
                    program.IsActive &&
                    version.State == CatalogueVersionState.Published &&
                    version.EffectiveFromUtc <= evaluatedAtUtc
                orderby version.EffectiveFromUtc descending, version.Id
                select new
                {
                    version.VersionCode,
                    version.ScopeCode
                })
            .FirstOrDefaultAsync(cancellationToken);
        if (catalogueVersion is null)
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(cancellationToken);
            }
            return null;
        }

        var rows = await (
                from sectionGroup in dbContext.Set<SectionGroup>().AsNoTracking()
                join offering in dbContext.Set<CourseOffering>().AsNoTracking()
                    on sectionGroup.OfferingId equals offering.Id
                join course in dbContext.Set<Course>().AsNoTracking()
                    on offering.CourseId equals course.Id
                join meeting in dbContext.Set<MeetingSlot>().AsNoTracking()
                    on sectionGroup.Id equals meeting.GroupId into meetings
                from meeting in meetings.DefaultIfEmpty()
                where requestedGroupIds.Contains(sectionGroup.Id) &&
                    offering.TermId == termId
                orderby offering.Id, sectionGroup.Id, meeting.DayOfWeek,
                    meeting.StartLocal, meeting.EndLocal, meeting.Id
                select new
                {
                    OfferingId = offering.Id,
                    GroupId = sectionGroup.Id,
                    sectionGroup.GroupCode,
                    course.Code,
                    course.Title,
                    course.Credits,
                    sectionGroup.State,
                    sectionGroup.RegistrationPaused,
                    sectionGroup.Capacity,
                    sectionGroup.EnrolledCount,
                    OfferingVersion = offering.Version,
                    GroupVersion = sectionGroup.Version,
                    MeetingId = meeting == null ? (Guid?)null : meeting.Id,
                    ActivityType = meeting == null
                        ? (ActivityType?)null
                        : meeting.ActivityType,
                    RoomId = meeting == null ? (Guid?)null : meeting.RoomId,
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
        var meetingIds = rows
            .Where(row => row.MeetingId.HasValue)
            .Select(row => row.MeetingId!.Value)
            .Distinct()
            .ToArray();
        var roomIds = rows
            .Where(row => row.RoomId.HasValue)
            .Select(row => row.RoomId!.Value)
            .Distinct()
            .ToArray();
        var rooms = await dbContext.Set<Room>()
            .AsNoTracking()
            .Where(room => roomIds.Contains(room.Id))
            .ToDictionaryAsync(room => room.Id, cancellationToken);
        var staffRows = await (
                from assignment in dbContext.Set<GroupStaffAssignment>().AsNoTracking()
                join staff in dbContext.Set<Staff>().AsNoTracking()
                    on assignment.StaffId equals staff.Id
                where meetingIds.Contains(assignment.MeetingSlotId) && staff.IsActive
                orderby assignment.MeetingSlotId, assignment.TeachingRole, staff.DisplayName
                select new
                {
                    assignment.MeetingSlotId,
                    assignment.TeachingRole,
                    staff.DisplayName
                })
            .ToArrayAsync(cancellationToken);
        if (transaction is not null)
        {
            await transaction.CommitAsync(cancellationToken);
        }

        var groups = rows
            .GroupBy(row => row.GroupId)
            .Select(groupRows =>
            {
                var first = groupRows.First();
                return new RegistrationPlanGroupSnapshot(
                    first.OfferingId,
                    first.GroupId,
                    first.GroupCode,
                    first.Code,
                    first.Title,
                    first.Credits,
                    first.State.ToString().ToLowerInvariant(),
                    first.RegistrationPaused,
                    first.Capacity,
                    first.EnrolledCount,
                    Convert.ToBase64String(first.OfferingVersion),
                    Convert.ToBase64String(first.GroupVersion),
                    groupRows
                        .Where(row =>
                            row.MeetingId.HasValue &&
                            row.ActivityType.HasValue &&
                            row.RoomId.HasValue &&
                            row.DayOfWeek.HasValue &&
                            row.StartLocal.HasValue &&
                            row.EndLocal.HasValue)
                        .Select(row =>
                        {
                            rooms.TryGetValue(row.RoomId!.Value, out var room);
                            var meetingStaff = staffRows
                                .Where(staff =>
                                    staff.MeetingSlotId == row.MeetingId!.Value)
                                .ToArray();
                            return new RegistrationPlanMeetingSnapshot(
                                row.MeetingId!.Value,
                                row.DayOfWeek!.Value,
                                row.StartLocal!.Value,
                                row.EndLocal!.Value,
                                row.ActivityType!.Value.ToString(),
                                room?.Code ?? "Unavailable",
                                room?.Location ?? "Unavailable",
                                meetingStaff
                                    .FirstOrDefault(staff =>
                                        staff.TeachingRole == TeachingRole.Lecturer)
                                    ?.DisplayName,
                                meetingStaff
                                    .Where(staff =>
                                        staff.TeachingRole ==
                                            TeachingRole.TeachingAssistant)
                                    .Select(staff => staff.DisplayName)
                                    .ToArray(),
                                academic.TimeZoneId);
                        })
                        .ToArray());
            })
            .ToArray();
        return new(
            Convert.ToBase64String(academic.Version),
            policy.Id,
            policy.VersionCode,
            policy.SourceReference,
            catalogueVersion.VersionCode,
            groups,
            catalogueVersion.ScopeCode,
            policy.ScopeCode);
    }

    private IQueryable<RegistrationPlan> PlanQuery(Guid studentId, Guid termId) =>
        dbContext.Set<RegistrationPlan>()
            .Include(plan => plan.Items)
            .Where(plan => plan.StudentId == studentId && plan.TermId == termId);
}

public static class RegistrationPlanSqlServerRegistration
{
    public static IServiceCollection AddStudentRegistrationPlansSqlServer(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddScoped<RegistrationPlanSqlServerAdapter>();
        services.TryAddScoped<IRegistrationPlanOwnerReader>(Resolve);
        services.TryAddScoped<IRegistrationPlanStore>(Resolve);
        services.TryAddScoped<IRegistrationPlanContextReader>(Resolve);
        return services;
    }

    private static RegistrationPlanSqlServerAdapter Resolve(
        IServiceProvider services) =>
        services.GetRequiredService<RegistrationPlanSqlServerAdapter>();
}
