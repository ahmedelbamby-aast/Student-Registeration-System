using System.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Infrastructure.SqlServer.Audit;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Scheduling.Application;
using StudentRegistration.Scheduling.Application.Ports;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.Infrastructure.SqlServer.Scheduling;

public sealed class SqlStaffAvailabilityPort(
    StudentRegistrationDbContext dbContext,
    TimeProvider timeProvider) : IStaffAvailabilityPort
{
    public async Task<StaffAvailabilityPortResult> GetOwnAsync(
        Guid staffId,
        Guid termId,
        CancellationToken cancellationToken = default)
    {
        var now = UtcNow();
        if (staffId == Guid.Empty || termId == Guid.Empty)
        {
            return StaffAvailabilityPortResult.Invalid(null, now);
        }

        var current = await QueryAvailability(staffId, termId)
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);
        return current is null
            ? StaffAvailabilityPortResult.NotFound(now)
            : StaffAvailabilityPortResult.Success(Snapshot(current), [], now);
    }

    public async Task<StaffAvailabilityPortResult> ReplaceOwnAsync(
        ReplaceOwnStaffAvailability command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        var now = UtcNow();
        try
        {
            // Read and hold the published schedule boundary before locking the
            // staff-term root, matching publication's group-before-staff order.
            var publishedMeetings = await LoadPublishedMeetings(
                command.StaffId,
                command.TermId,
                cancellationToken);
            var current = await dbContext.Set<StaffTermAvailability>()
                .FromSqlInterpolated($$"""
                    SELECT *
                    FROM [scheduling].[StaffTermAvailabilities] WITH (UPDLOCK, HOLDLOCK)
                    WHERE [StaffId] = {{command.StaffId}} AND [TermId] = {{command.TermId}}
                    """)
                .Include(availability => availability.Ranges)
                .SingleOrDefaultAsync(cancellationToken);
            if (current is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return StaffAvailabilityPortResult.NotFound(now);
            }

            if (command.ExpectedStaffTermVersion is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return StaffAvailabilityPortResult.Invalid(Snapshot(current), now);
            }

            if (!command.ExpectedStaffTermVersion.SequenceEqual(current.Version))
            {
                // Canonical STALE_VERSION response includes the authorized current aggregate.
                await transaction.RollbackAsync(cancellationToken);
                return StaffAvailabilityPortResult.Stale(Snapshot(current), now);
            }

            var oldRanges = current.Ranges.ToArray();
            var decision = StaffAvailabilityTransactionRules.Apply(
                current,
                command,
                now,
                publishedMeetings);
            if (decision.Outcome is not StaffAvailabilityPortOutcome.Success)
            {
                await transaction.RollbackAsync(cancellationToken);
                var unchanged = Snapshot(current, oldRanges);
                return decision.Outcome switch
                {
                    StaffAvailabilityPortOutcome.StaleVersion =>
                        StaffAvailabilityPortResult.Stale(unchanged, now),
                    StaffAvailabilityPortOutcome.DeadlinePassed =>
                        StaffAvailabilityPortResult.DeadlinePassed(unchanged, now),
                    _ => StaffAvailabilityPortResult.Invalid(unchanged, now),
                };
            }

            dbContext.RemoveRange(oldRanges);
            await dbContext.SaveChangesAsync(cancellationToken);

            var alertIds = await AddMissingImpactAlerts(
                current,
                decision.ImpactedMeetings,
                now,
                cancellationToken);
            dbContext.AuditEvents.Add(new AuditEvent(
                Guid.NewGuid(),
                command.StaffId.ToString("D"),
                command.StaffId.ToString("D"),
                "staff-availability-replaced",
                nameof(StaffTermAvailability),
                current.Id.ToString("D"),
                command.Reason.Trim(),
                JsonSerializer.Serialize(new { rangeCount = oldRanges.Length }),
                JsonSerializer.Serialize(new
                {
                    rangeCount = current.Ranges.Count,
                    impactAlertCount = alertIds.Count,
                }),
                command.CorrelationId.Trim(),
                now));
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return StaffAvailabilityPortResult.Success(
                Snapshot(current),
                alertIds,
                now);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();
            var current = await QueryAvailability(command.StaffId, command.TermId)
                .AsNoTracking()
                .SingleOrDefaultAsync(cancellationToken);
            return current is null
                ? StaffAvailabilityPortResult.NotFound(now)
                : StaffAvailabilityPortResult.Stale(Snapshot(current), now);
        }
    }

    private IQueryable<StaffTermAvailability> QueryAvailability(
        Guid staffId,
        Guid termId) =>
        dbContext.Set<StaffTermAvailability>()
            .Include(availability => availability.Ranges)
            .Where(availability =>
                availability.StaffId == staffId
                && availability.TermId == termId);

    private async Task<IReadOnlyList<PublishedStaffMeeting>> LoadPublishedMeetings(
        Guid staffId,
        Guid termId,
        CancellationToken cancellationToken)
    {
        var query =
            from assignment in dbContext.Set<GroupStaffAssignment>()
            join meeting in dbContext.Set<MeetingSlot>()
                on new { assignment.MeetingSlotId, assignment.GroupId }
                equals new { MeetingSlotId = meeting.Id, meeting.GroupId }
            join sectionGroup in dbContext.Set<SectionGroup>()
                on assignment.GroupId equals sectionGroup.Id
            join offering in dbContext.Set<CourseOffering>()
                on sectionGroup.OfferingId equals offering.Id
            where assignment.StaffId == staffId
                && offering.TermId == termId
                && offering.State == CourseOfferingState.Published
                && sectionGroup.State == SectionGroupState.Published
            select new PublishedStaffMeeting(
                sectionGroup.Id,
                sectionGroup.Version,
                meeting.DayOfWeek,
                meeting.StartLocal,
                meeting.EndLocal);

        return await query
            .OrderBy(meeting => meeting.GroupId)
            .ToArrayAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<Guid>> AddMissingImpactAlerts(
        StaffTermAvailability availability,
        IReadOnlyList<PublishedStaffMeeting> impactedMeetings,
        DateTime detectedAtUtc,
        CancellationToken cancellationToken)
    {
        var ids = new List<Guid>();
        foreach (var meeting in impactedMeetings)
        {
            var exists = await dbContext.Set<ScheduleImpactAlert>().AnyAsync(
                alert =>
                    alert.GroupId == meeting.GroupId
                    && alert.StaffTermAvailabilityId == availability.Id
                    && alert.State != ScheduleImpactAlertState.Resolved
                    && alert.DetectedGroupVersion == meeting.GroupVersion
                    && alert.DetectedResourceVersion == availability.Version,
                cancellationToken);
            if (exists)
            {
                continue;
            }

            var id = Guid.NewGuid();
            dbContext.Add(new ScheduleImpactAlert(
                id,
                meeting.GroupId,
                availability.Id,
                "STAFF_UNAVAILABLE",
                meeting.GroupVersion,
                availability.Version,
                detectedAtUtc));
            ids.Add(id);
        }

        return ids;
    }

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;

    private static StaffTermAvailabilitySnapshot Snapshot(
        StaffTermAvailability availability,
        IReadOnlyList<StaffAvailability>? ranges = null) =>
        new(
            availability.Id,
            availability.StaffId,
            availability.TermId,
            availability.DeadlineUtc,
            availability.Version.ToArray(),
            (ranges ?? availability.Ranges)
                .OrderBy(range => range.DayOfWeek)
                .ThenBy(range => range.StartLocal)
                .ThenBy(range => range.Id)
                .Select(range => new StaffAvailabilityRangeSnapshot(
                    range.Id,
                    range.DayOfWeek,
                    range.StartLocal,
                    range.EndLocal,
                    range.Kind))
                .ToArray());
}
