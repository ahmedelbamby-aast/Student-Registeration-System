using StudentRegistration.Contracts.Scheduling;

namespace StudentRegistration.Contracts.Staff;

public static class StaffWorkspaceContract
{
    public const string LecturerRole = "Lecturer";
    public const string TeachingAssistantRole = "TeachingAssistant";
    public const string ActiveEnrollmentState = "active";
    public const int DefaultPageSize = 20;
    public const int MaximumPageSize = 100;
    public const string RosterSort = "displayName:asc,universityId:asc";
}

public sealed record StaffAssignmentDto(
    string SubjectCode,
    string SubjectTitle,
    GroupDto Group,
    string StaffRole,
    int RosterCount);

public sealed record StaffTimetableDto(
    string RoleContext,
    IReadOnlyList<StaffAssignmentDto> Assignments);

public sealed record RosterRowDto(
    string UniversityId,
    string DisplayName,
    string EnrollmentState);

public sealed record AvailabilityRangeDto(
    Guid Id,
    DayOfWeek DayOfWeek,
    TimeOnly StartLocal,
    TimeOnly EndLocal,
    string Kind);

public sealed record StaffTermAvailabilityDto(
    Guid Id,
    Guid StaffId,
    Guid TermId,
    DateTime DeadlineUtc,
    string RowVersion,
    IReadOnlyList<AvailabilityRangeDto> Ranges);

public sealed record ReplaceAvailabilityRequest(
    string ExpectedStaffTermRowVersion,
    IReadOnlyList<AvailabilityRangeDto> Ranges);

public sealed record AvailabilityUpdateResult(
    StaffTermAvailabilityDto Availability,
    IReadOnlyList<Guid> ImpactAlertIds);
