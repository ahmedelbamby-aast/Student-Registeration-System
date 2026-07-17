using StudentRegistration.Client.Components.Scheduling;
using StudentRegistration.Client.Features.Academics;
using StudentRegistration.Client.Features.Frontend.Models;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Scheduling;
using StudentRegistration.Contracts.Staff;

namespace StudentRegistration.Client.Features.Staff;

public static class StaffPageSupport
{
    public static bool HasStaffRole(AppContextDto context) =>
        context.AuthorizedRoles.Any(IsStaffRole);

    public static bool HasActiveStaffContext(AppContextDto context) =>
        context.ActiveRole is not null && IsStaffRole(context.ActiveRole) &&
        context.AuthorizedRoles.Contains(context.ActiveRole, StringComparer.Ordinal);

    public static bool IsStaffRole(string role) =>
        string.Equals(role, StaffWorkspaceContract.LecturerRole, StringComparison.Ordinal) ||
        string.Equals(
            role,
            StaffWorkspaceContract.TeachingAssistantRole,
            StringComparison.Ordinal);

    public static FrontendAppContextView ToShellContext(AppContextDto context) =>
        AcademicContextViewMapper.ToView(context);

    public static IReadOnlyList<ScheduleCalendar.ScheduleMeetingItem> ToMeetingItems(
        IReadOnlyList<StaffAssignmentDto> assignments,
        string timeZoneId) =>
        assignments
            .SelectMany(assignment => assignment.Group.Meetings.Select(meeting => new
            {
                Assignment = assignment,
                Meeting = meeting
            }))
            .OrderBy(item => item.Meeting.DayOfWeek)
            .ThenBy(item => item.Meeting.StartLocal)
            .ThenBy(item => item.Meeting.EndLocal)
            .ThenBy(item => item.Assignment.SubjectCode, StringComparer.Ordinal)
            .ThenBy(item => item.Assignment.Group.GroupCode, StringComparer.Ordinal)
            .ThenBy(item => item.Meeting.Id)
            .Select(item => ToMeetingItem(
                item.Assignment,
                item.Meeting,
                timeZoneId))
            .ToArray();

    public static IReadOnlyList<ScheduleCalendar.ScheduleMeetingItem> ToAvailabilityItems(
        IReadOnlyList<AvailabilityRangeDto> ranges,
        string timeZoneId) =>
        ranges
            .OrderBy(range => range.DayOfWeek)
            .ThenBy(range => range.StartLocal)
            .ThenBy(range => range.EndLocal)
            .ThenBy(range => range.Id)
            .Select(range => new ScheduleCalendar.ScheduleMeetingItem(
                range.Id.ToString("D"),
                "Availability",
                range.Kind,
                "Owned range",
                range.Kind,
                null,
                [],
                "No room",
                range.DayOfWeek.ToString(),
                range.StartLocal.ToString("HH:mm"),
                range.EndLocal.ToString("HH:mm"),
                timeZoneId,
                "No conflict",
                "/staff/availability",
                $"Review {range.Kind} range on {range.DayOfWeek}"))
            .ToArray();

    private static ScheduleCalendar.ScheduleMeetingItem ToMeetingItem(
        StaffAssignmentDto assignment,
        MeetingDto meeting,
        string timeZoneId)
    {
        var staff = assignment.Group.Staff
            .Where(item => item.MeetingSlotId == meeting.Id)
            .ToArray();
        var lecturer = staff.FirstOrDefault(item =>
            string.Equals(item.Role, StaffWorkspaceContract.LecturerRole,
                StringComparison.OrdinalIgnoreCase))?.Name;
        var teachingAssistants = staff
            .Where(item => string.Equals(
                item.Role,
                StaffWorkspaceContract.TeachingAssistantRole,
                StringComparison.OrdinalIgnoreCase))
            .Select(item => item.Name)
            .ToArray();

        return new ScheduleCalendar.ScheduleMeetingItem(
            meeting.Id.ToString("D"),
            assignment.SubjectCode,
            assignment.SubjectTitle,
            assignment.Group.GroupCode,
            meeting.ActivityType,
            lecturer,
            teachingAssistants,
            $"{meeting.RoomCode} {meeting.Location}".Trim(),
            meeting.DayOfWeek.ToString(),
            meeting.StartLocal.ToString("HH:mm"),
            meeting.EndLocal.ToString("HH:mm"),
            timeZoneId,
            "Assigned",
            $"/staff/groups/{assignment.Group.Id:D}/roster",
            $"Open {assignment.SubjectCode} {assignment.Group.GroupCode} roster");
    }
}
