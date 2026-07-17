using StudentRegistration.Scheduling.Application.Ports;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.Scheduling.Application;

public sealed record PublishedStaffMeeting(
    Guid GroupId,
    byte[] GroupVersion,
    DayOfWeek DayOfWeek,
    TimeOnly StartLocal,
    TimeOnly EndLocal);

public sealed record StaffAvailabilityRuleDecision(
    StaffAvailabilityPortOutcome Outcome,
    IReadOnlyList<PublishedStaffMeeting> ImpactedMeetings,
    string? ErrorCode = null);

public static class StaffAvailabilityTransactionRules
{
    public static StaffAvailabilityRuleDecision Apply(
        StaffTermAvailability current,
        ReplaceOwnStaffAvailability command,
        DateTime serverTimeUtc,
        IReadOnlyList<PublishedStaffMeeting> publishedMeetings)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(publishedMeetings);

        if (serverTimeUtc.Kind is not DateTimeKind.Utc
            || command.StaffId == Guid.Empty
            || command.TermId == Guid.Empty
            || command.ExpectedStaffTermVersion is null
            || command.Ranges is null
            || string.IsNullOrWhiteSpace(command.Reason)
            || string.IsNullOrWhiteSpace(command.CorrelationId)
            || current.StaffId != command.StaffId
            || current.TermId != command.TermId)
        {
            return new(
                StaffAvailabilityPortOutcome.ValidationFailed,
                [],
                "VALIDATION_ERROR");
        }

        if (!command.ExpectedStaffTermVersion.SequenceEqual(current.Version))
        {
            return new(
                StaffAvailabilityPortOutcome.StaleVersion,
                [],
                "STALE_VERSION");
        }

        if (serverTimeUtc > current.DeadlineUtc)
        {
            return new(
                StaffAvailabilityPortOutcome.DeadlinePassed,
                [],
                "AVAILABILITY_DEADLINE_PASSED");
        }

        if (!ValidCompleteRanges(command.Ranges))
        {
            return new(
                StaffAvailabilityPortOutcome.ValidationFailed,
                [],
                "VALIDATION_ERROR");
        }

        var replacements = command.Ranges
            .Select(range => new StaffAvailability(
                range.Id,
                current.Id,
                range.DayOfWeek,
                range.StartLocal,
                range.EndLocal,
                range.Kind))
            .ToArray();
        current.ReplaceRanges(replacements, serverTimeUtc);

        var impacted = publishedMeetings
            .Where(meeting => IsUnavailable(meeting, command.Ranges))
            .GroupBy(meeting => meeting.GroupId)
            .Select(group => group.First())
            .OrderBy(meeting => meeting.GroupId)
            .ToArray();

        return new(StaffAvailabilityPortOutcome.Success, impacted);
    }

    private static bool ValidCompleteRanges(
        IReadOnlyList<StaffAvailabilityRangeInput> ranges)
    {
        if (ranges.Count == 0
            || ranges.Any(range =>
                range.Id == Guid.Empty
                || !Enum.IsDefined(range.DayOfWeek)
                || !Enum.IsDefined(range.Kind)
                || range.EndLocal <= range.StartLocal)
            || ranges.Select(range => range.Id).Distinct().Count() != ranges.Count)
        {
            return false;
        }

        return !ranges
            .GroupBy(range => range.DayOfWeek)
            .Any(day => day
                .OrderBy(range => range.StartLocal)
                .Zip(day.OrderBy(range => range.StartLocal).Skip(1))
                .Any(pair => pair.First.EndLocal > pair.Second.StartLocal));
    }

    private static bool IsUnavailable(
        PublishedStaffMeeting meeting,
        IReadOnlyList<StaffAvailabilityRangeInput> ranges)
    {
        var sameDay = ranges
            .Where(range => range.DayOfWeek == meeting.DayOfWeek)
            .ToArray();
        var explicitlyUnavailable = sameDay.Any(range =>
            range.Kind is AvailabilityKind.Unavailable
            && range.StartLocal < meeting.EndLocal
            && meeting.StartLocal < range.EndLocal);
        var coveredByAvailableRange = sameDay.Any(range =>
            range.Kind is AvailabilityKind.Available
            && range.StartLocal <= meeting.StartLocal
            && range.EndLocal >= meeting.EndLocal);

        return explicitlyUnavailable || !coveredByAvailableRange;
    }
}
