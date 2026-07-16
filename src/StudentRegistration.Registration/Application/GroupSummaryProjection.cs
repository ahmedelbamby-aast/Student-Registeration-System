using StudentRegistration.Registration.Application.Ports;
using StudentRegistration.Registration.Domain;
using StudentRegistration.Scheduling.Application.Ports;

namespace StudentRegistration.Registration.Application;

public sealed class GroupSummaryProjection
{
    public GroupSummary Project(
        EligibilityGroupSnapshot source,
        IReadOnlyList<CurrentPlanMeetingSnapshot> currentPlanMeetings)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(currentPlanMeetings);

        var reasons = new List<GroupNonSelectableReason>();
        var state = Required(source.State).ToLowerInvariant();
        AddLifecycleReason(reasons, state);
        AddReason(
            reasons,
            source.RegistrationPaused,
            "REGISTRATION_PAUSED",
            "Registration is paused for this group.");
        AddReason(
            reasons,
            source.EnrolledCount >= source.Capacity,
            "GROUP_FULL",
            "No seats currently remain in this group.");
        AddReason(
            reasons,
            source.Meetings.Any(meeting => !meeting.RoomAvailable),
            "ROOM_UNAVAILABLE",
            "A scheduled room is currently unavailable.");
        AddReason(
            reasons,
            !HasCompleteStaffing(source.Meetings),
            "GROUP_INCOMPLETE",
            "The group does not have the required meeting and staffing bundle.");
        AddReason(
            reasons,
            HasConflict(source.Meetings, currentPlanMeetings),
            "MEETING_CONFLICT",
            "This group overlaps the student's current plan.");

        var meetings = source.Meetings
            .OrderBy(meeting => meeting.DayOfWeek)
            .ThenBy(meeting => meeting.StartLocal)
            .ThenBy(meeting => meeting.MeetingId)
            .Select(meeting => new GroupMeetingSummary(
                meeting.MeetingId,
                CanonicalActivity(meeting.Activity),
                meeting.DayOfWeek,
                meeting.StartLocal,
                meeting.EndLocal,
                meeting.RoomCode,
                meeting.Location,
                meeting.Staff
                    .OrderBy(staff => staff.Role, StringComparer.Ordinal)
                    .ThenBy(staff => staff.Name, StringComparer.Ordinal)
                    .ThenBy(staff => staff.StaffId)
                    .Select(staff => new GroupMeetingStaffSummary(
                        CanonicalRole(staff.Role),
                        staff.Name))
                    .ToArray()))
            .ToArray();

        return new GroupSummary(
            source.GroupId,
            source.GroupCode,
            state,
            reasons.Count == 0,
            source.Capacity,
            source.EnrolledCount,
            reasons,
            meetings,
            Convert.ToBase64String(source.RowVersion));
    }

    private static void AddLifecycleReason(
        List<GroupNonSelectableReason> reasons,
        string state)
    {
        switch (state)
        {
            case "published":
                return;
            case "draft":
                reasons.Add(new(
                    "GROUP_UNPUBLISHED",
                    "This group is not published."));
                return;
            case "closed":
                reasons.Add(new("GROUP_CLOSED", "This group is closed."));
                return;
            case "cancelled":
                reasons.Add(new("GROUP_CANCELLED", "This group is cancelled."));
                return;
            default:
                reasons.Add(new(
                    "GROUP_STATE_UNAVAILABLE",
                    "The group lifecycle is unavailable."));
                return;
        }
    }

    private static bool HasCompleteStaffing(
        IReadOnlyList<EligibilityMeetingSnapshot> meetings)
    {
        if (meetings.Count == 0)
        {
            return false;
        }

        var lectures = meetings.Where(meeting => Is(meeting.Activity, "Lecture")).ToArray();
        var tutorials = meetings.Where(meeting => Is(meeting.Activity, "Tutorial")).ToArray();
        var laboratories = meetings.Where(meeting => Is(meeting.Activity, "Laboratory")).ToArray();
        return lectures.Length > 0
            && lectures.All(meeting => meeting.Staff.Any(staff => Is(staff.Role, "Lecturer")))
            && tutorials.Concat(laboratories).Any()
            && tutorials.All(meeting =>
                meeting.Staff.Any(staff => Is(staff.Role, "TeachingAssistant")))
            && laboratories.All(meeting =>
                meeting.Staff.Any(staff => Is(staff.Role, "TeachingAssistant")));
    }

    private static bool HasConflict(
        IReadOnlyList<EligibilityMeetingSnapshot> candidate,
        IReadOnlyList<CurrentPlanMeetingSnapshot> currentPlan) =>
        candidate.Any(left => currentPlan.Any(right =>
            left.DayOfWeek == right.DayOfWeek
            && left.StartLocal < right.EndLocal
            && right.StartLocal < left.EndLocal));

    private static string CanonicalActivity(string value) =>
        value.Trim().ToLowerInvariant() switch
        {
            "lecture" => "Lecture",
            "tutorial" => "Tutorial",
            "laboratory" => "Laboratory",
            _ => Required(value)
        };

    private static string CanonicalRole(string value) =>
        value.Trim().Replace("-", string.Empty, StringComparison.Ordinal)
            .ToLowerInvariant() switch
        {
            "lecturer" => "Lecturer",
            "teachingassistant" => "TeachingAssistant",
            _ => Required(value)
        };

    private static void AddReason(
        List<GroupNonSelectableReason> reasons,
        bool condition,
        string code,
        string message)
    {
        if (condition)
        {
            reasons.Add(new(code, message));
        }
    }

    private static bool Is(string value, string expected) =>
        string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);

    private static string Required(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? "Unavailable"
            : value.Trim();
}
