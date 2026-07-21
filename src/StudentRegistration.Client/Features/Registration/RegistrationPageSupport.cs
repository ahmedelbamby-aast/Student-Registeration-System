using StudentRegistration.Client.Features.Frontend.Models;
using StudentRegistration.Client.Components.Scheduling;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Registration;

namespace StudentRegistration.Client.Features.Registration;

public static class RegistrationPageSupport
{
    public static bool IsStudentContext(AppContextDto context) =>
        string.Equals(context.ActiveRole, "Student", StringComparison.Ordinal) &&
        context.AuthorizedRoles.Contains("Student", StringComparer.Ordinal);

    public static FrontendAppContextView ToShellContext(AppContextDto context) => new(
        new DateTimeOffset(context.ServerTimeUtc),
        context.TimeZoneId,
        context.TeachingTerm?.Label,
        context.RegistrationTerm?.Label,
        context.RegistrationWindowState,
        context.RegistrationWindow,
        context.DisplayName,
        context.AuthorizedRoles,
        context.ActiveRole,
        SessionStateToken(context.SessionState),
        new DateTimeOffset(context.ExpiresAtUtc),
        ServiceStateToken(context.ServiceState),
        context.SupportReferencePath);

    public static string DayName(int day) => day is >= 0 and <= 6
        ? ((DayOfWeek)day).ToString()
        : $"Day {day}";

    public static IReadOnlyList<ScheduleCalendar.ScheduleMeetingItem> ToMeetingItems(
        IReadOnlyList<RegistrationRecordGroupDto> groups,
        string timeZoneId) =>
        groups
            .SelectMany(group => group.Meetings.Select(meeting => new
            {
                Group = group,
                Meeting = meeting
            }))
            .OrderBy(item => item.Meeting.DayOfWeek)
            .ThenBy(item => item.Meeting.StartLocal, StringComparer.Ordinal)
            .ThenBy(item => item.Meeting.EndLocal, StringComparer.Ordinal)
            .ThenBy(item => item.Group.CourseCode, StringComparer.Ordinal)
            .ThenBy(item => item.Group.GroupCode, StringComparer.Ordinal)
            .ThenBy(item => item.Meeting.MeetingId)
            .Select(item => new ScheduleCalendar.ScheduleMeetingItem(
                item.Meeting.MeetingId.ToString("D"),
                item.Group.CourseCode,
                item.Group.SubjectTitle,
                item.Group.GroupCode,
                item.Meeting.ActivityType,
                item.Meeting.Staff.FirstOrDefault(staff =>
                    string.Equals(staff.Role, "Lecturer", StringComparison.OrdinalIgnoreCase))
                    ?.DisplayName,
                item.Meeting.Staff.Where(staff =>
                        string.Equals(
                            staff.Role,
                            "TeachingAssistant",
                            StringComparison.OrdinalIgnoreCase))
                    .Select(staff => staff.DisplayName)
                    .ToArray(),
                $"{item.Meeting.RoomCode} {item.Meeting.Location}".Trim(),
                DayName(item.Meeting.DayOfWeek),
                item.Meeting.StartLocal,
                item.Meeting.EndLocal,
                timeZoneId,
                "No conflict",
                $"/student/subjects/{item.Group.OfferingId:D}",
                $"Open {item.Group.CourseCode} {item.Group.GroupCode} details"))
            .ToArray();

    private static string SessionStateToken(SessionState state) => state switch
    {
        SessionState.Active => "active",
        SessionState.Expiring => "expiring",
        _ => "unknown"
    };

    private static string ServiceStateToken(ServiceState state) => state switch
    {
        ServiceState.Available => "available",
        ServiceState.Maintenance => "maintenance",
        ServiceState.Unavailable => "unavailable",
        _ => "unavailable"
    };
}
