using StudentRegistration.Client.Components.Scheduling;
using StudentRegistration.Client.Features.Frontend.Models;

namespace StudentRegistration.Client.Features.Scheduling;

/// <summary>
/// Maps server-returned conflict details to the approved accessible client state.
/// It does not detect overlaps, authorize a plan, or infer a resolution.
/// </summary>
public static class ConflictStateMapper
{
    public const string MeetingOverlapCode = "MEETING_OVERLAP";
    public const string ConflictText = "Conflict";
    public const string ConflictIcon = "✕";

    public static AccessibleConflictState Map(
        IReadOnlyList<ServerScheduleConflictPresentation> conflicts,
        IReadOnlyList<ScheduleCalendar.ScheduleMeetingItem> canonicalMeetings)
    {
        ArgumentNullException.ThrowIfNull(conflicts);
        ArgumentNullException.ThrowIfNull(canonicalMeetings);

        if (conflicts.Any(conflict => conflict is null))
        {
            throw new ArgumentException("Conflicts cannot contain null entries.", nameof(conflicts));
        }

        if (canonicalMeetings.Any(meeting => meeting is null))
        {
            throw new ArgumentException(
                "The canonical schedule cannot contain null meetings.",
                nameof(canonicalMeetings));
        }

        var meetings = canonicalMeetings.ToArray();
        var mappedConflicts = conflicts.Select(MapConflict).ToArray();
        var blockingReasons = conflicts
            .Select(conflict => conflict.Message)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return new AccessibleConflictState(
            mappedConflicts.Length > 0,
            mappedConflicts.Length > 0 ? ConflictIcon : string.Empty,
            mappedConflicts.Length > 0 ? ConflictText : "No conflict",
            blockingReasons,
            mappedConflicts,
            meetings,
            meetings);
    }

    private static MappedScheduleConflictPresentation MapConflict(
        ServerScheduleConflictPresentation conflict)
    {
        if (!string.Equals(conflict.Code, MeetingOverlapCode, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Only server-returned meeting overlaps are enabled for the demo.",
                nameof(conflict));
        }

        ValidateGroup(conflict.First, nameof(conflict.First));
        ValidateGroup(conflict.Second, nameof(conflict.Second));
        if (string.Equals(conflict.First.GroupId, conflict.Second.GroupId, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "A conflict must identify two different groups.",
                nameof(conflict));
        }

        if (conflict.OverlapEndLocal <= conflict.OverlapStartLocal)
        {
            throw new ArgumentException(
                "The server overlap interval must end after it starts.",
                nameof(conflict));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(conflict.Message);
        ValidateActions(conflict);

        var first = new ConflictSubjectGroupView(
            conflict.First.CourseCode,
            conflict.First.SubjectTitle,
            conflict.First.GroupCode);
        var second = new ConflictSubjectGroupView(
            conflict.Second.CourseCode,
            conflict.Second.SubjectTitle,
            conflict.Second.GroupCode);

        var panel = new ConflictView(
            [first, second],
            [
                new ConflictOverlapSlotView(
                    conflict.Day,
                    conflict.OverlapStartLocal,
                    conflict.OverlapEndLocal,
                    [first.Reference, second.Reference])
            ],
            [],
            conflict.Actions
                .Select(action => new ConflictResolutionLinkView(
                    action.Label,
                    action.Route,
                    action.Action,
                    action.TargetGroupId))
                .ToArray());

        return new MappedScheduleConflictPresentation(conflict, panel);
    }

    private static void ValidateGroup(
        ServerConflictGroupPresentation group,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentException.ThrowIfNullOrWhiteSpace(group.GroupId);
        ArgumentException.ThrowIfNullOrWhiteSpace(group.GroupCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(group.CourseCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(group.SubjectTitle);
        if (group.EndLocal <= group.StartLocal)
        {
            throw new ArgumentException(
                "A server meeting interval must end after it starts.",
                parameterName);
        }
    }

    private static void ValidateActions(ServerScheduleConflictPresentation conflict)
    {
        ArgumentNullException.ThrowIfNull(conflict.Actions);
        var groupIds = new[] { conflict.First.GroupId, conflict.Second.GroupId };

        foreach (var action in conflict.Actions)
        {
            ArgumentNullException.ThrowIfNull(action);
            if (action.Action is not ("change-group" or "remove-group"))
            {
                throw new ArgumentException(
                    "Conflict actions must be change-group or remove-group.",
                    nameof(conflict));
            }

            if (!groupIds.Contains(action.TargetGroupId, StringComparer.Ordinal))
            {
                throw new ArgumentException(
                    "A conflict action must target an affected group.",
                    nameof(conflict));
            }

            ArgumentException.ThrowIfNullOrWhiteSpace(action.Label);
            ArgumentException.ThrowIfNullOrWhiteSpace(action.Route);
        }

        var hasCompleteActions = groupIds.All(groupId =>
            conflict.Actions.Any(action =>
                action.Action == "change-group"
                && action.TargetGroupId == groupId)
            && conflict.Actions.Any(action =>
                action.Action == "remove-group"
                && action.TargetGroupId == groupId));
        if (!hasCompleteActions)
        {
            throw new ArgumentException(
                "Each affected group requires change and remove actions.",
                nameof(conflict));
        }
    }
}

public sealed record ServerConflictGroupPresentation(
    string GroupId,
    string GroupCode,
    string CourseCode,
    string SubjectTitle,
    TimeOnly StartLocal,
    TimeOnly EndLocal);

public sealed record ServerConflictResolutionActionPresentation(
    string Action,
    string TargetGroupId,
    string Label,
    string Route);

/// <summary>
/// Client-side immutable shape for the approved non-amendment conflict fields.
/// Values are copied from the authenticated server response.
/// </summary>
public sealed record ServerScheduleConflictPresentation(
    string Code,
    ServerConflictGroupPresentation First,
    ServerConflictGroupPresentation Second,
    DayOfWeek Day,
    TimeOnly OverlapStartLocal,
    TimeOnly OverlapEndLocal,
    string Message,
    IReadOnlyList<ServerConflictResolutionActionPresentation> Actions);

public sealed record MappedScheduleConflictPresentation(
    ServerScheduleConflictPresentation Source,
    ConflictView Panel);

public sealed record AccessibleConflictState(
    bool ReviewBlocked,
    string IconText,
    string StatusText,
    IReadOnlyList<string> BlockingReasons,
    IReadOnlyList<MappedScheduleConflictPresentation> Conflicts,
    IReadOnlyList<ScheduleCalendar.ScheduleMeetingItem> CalendarMeetings,
    IReadOnlyList<ScheduleCalendar.ScheduleMeetingItem> ChronologicalMeetings);
