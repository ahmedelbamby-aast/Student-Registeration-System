namespace StudentRegistration.Contracts.Scheduling;

public sealed record ValidationReasonDto(
    string Code,
    string Message,
    IReadOnlyList<Guid> ResourceIds,
    TimeOnly? OverlapStartLocal = null,
    TimeOnly? OverlapEndLocal = null);

public sealed record MeetingDto(
    Guid Id,
    string ActivityType,
    DayOfWeek DayOfWeek,
    TimeOnly StartLocal,
    TimeOnly EndLocal,
    Guid RoomId,
    string RoomCode,
    string Location);

public sealed record GroupStaffDto(
    Guid MeetingSlotId,
    string ActivityType,
    Guid StaffId,
    string Role,
    string Name);

public sealed record GroupDto(
    Guid Id,
    Guid OfferingId,
    string GroupCode,
    int Capacity,
    int EnrolledCount,
    bool RegistrationPaused,
    string State,
    bool Selectable,
    IReadOnlyList<ValidationReasonDto> NonSelectableReasons,
    IReadOnlyList<GroupStaffDto> Staff,
    IReadOnlyList<MeetingDto> Meetings,
    string RowVersion);

public sealed record CourseOfferingDto(
    Guid Id,
    Guid TermId,
    Guid CourseId,
    string CourseCode,
    string CourseTitle,
    string State,
    IReadOnlyList<GroupDto> Groups,
    string RowVersion);

public sealed record CourseOfferingSummaryDto(
    Guid Id,
    Guid TermId,
    Guid CourseId,
    string CourseCode,
    string CourseTitle,
    string State,
    int GroupCount,
    string RowVersion);

public sealed record CreateGroupInput(
    string GroupCode,
    int Capacity);

public sealed record CreateOfferingRequest(
    Guid TermId,
    Guid CourseId,
    IReadOnlyList<CreateGroupInput> Groups,
    Guid ClientRequestId);

public sealed record MeetingMutation(
    string RequestMeetingKey,
    Guid? Id,
    string ActivityType,
    DayOfWeek DayOfWeek,
    TimeOnly StartLocal,
    TimeOnly EndLocal,
    Guid RoomId);

public sealed record StaffAssignmentMutation(
    string RequestMeetingKey,
    Guid StaffId,
    string Role);

public sealed record UpdateGroupRequest(
    string ExpectedOfferingRowVersion,
    string ExpectedGroupRowVersion,
    IReadOnlyDictionary<string, string> ExpectedRoomRowVersions,
    IReadOnlyDictionary<string, string> ExpectedStaffTermAvailabilityRowVersions,
    string GroupCode,
    int Capacity,
    bool RegistrationPaused,
    string? State,
    IReadOnlyList<MeetingMutation> Meetings,
    IReadOnlyList<StaffAssignmentMutation> StaffAssignments,
    string Reason);

public sealed record DependencyVersions(
    string Offering,
    IReadOnlyDictionary<string, string> Groups,
    IReadOnlyDictionary<string, string> Rooms,
    IReadOnlyDictionary<string, string> StaffTermAvailability);

public sealed record ValidateOfferingRequest(
    string ExpectedOfferingRowVersion,
    IReadOnlyDictionary<string, string> ExpectedGroupRowVersions,
    IReadOnlyDictionary<string, string> ExpectedRoomRowVersions,
    IReadOnlyDictionary<string, string> ExpectedStaffTermAvailabilityRowVersions);

public sealed record OfferingValidationResult(
    bool Valid,
    IReadOnlyList<ValidationReasonDto> Reasons,
    string? PreviewToken,
    DependencyVersions DependencyVersions);

public sealed record ScheduleImpactValidationSnapshotDto(
    bool Valid,
    IReadOnlyList<ValidationReasonDto> Reasons,
    string GroupRowVersion,
    IReadOnlyDictionary<string, string> RoomRowVersions,
    IReadOnlyDictionary<string, string> StaffTermAvailabilityRowVersions,
    DateTime ValidatedAtUtc);

public sealed record PublishOfferingRequest(
    string ExpectedOfferingRowVersion,
    IReadOnlyDictionary<string, string> ExpectedGroupRowVersions,
    IReadOnlyDictionary<string, string> ExpectedRoomRowVersions,
    IReadOnlyDictionary<string, string> ExpectedStaffTermAvailabilityRowVersions,
    string PreviewToken,
    Guid ClientRequestId,
    string Reason);

public sealed record RoomDto(
    Guid Id,
    string Code,
    string Location,
    int Capacity,
    string State,
    string RowVersion);

public sealed record CreateRoomRequest(
    string Code,
    string Location,
    int Capacity,
    string State,
    Guid ClientRequestId);

public sealed record UpdateRoomRequest(
    string ExpectedRowVersion,
    string Code,
    string Location,
    int Capacity,
    string State,
    string Reason);

public sealed record StaffAvailabilityRangeDto(
    DayOfWeek DayOfWeek,
    TimeOnly StartLocal,
    TimeOnly EndLocal,
    string Kind);

public sealed record StaffTermAvailabilityDto(
    Guid Id,
    Guid StaffId,
    string StaffName,
    Guid TermId,
    DateTime DeadlineUtc,
    string RowVersion,
    IReadOnlyList<StaffAvailabilityRangeDto> Ranges);

public sealed record ScheduleImpactAlertDto(
    Guid Id,
    Guid GroupId,
    Guid? StaffTermAvailabilityId,
    Guid? RoomId,
    string ReasonCode,
    string State,
    string DetectedGroupRowVersion,
    string? DetectedRoomRowVersion,
    string? DetectedStaffTermAvailabilityRowVersion,
    ScheduleImpactValidationSnapshotDto? LastValidation,
    DateTime DetectedAtUtc,
    DateTime? RevalidatedAtUtc,
    DateTime? ResolvedAtUtc,
    string RowVersion);

public sealed record RevalidateScheduleImpactAlertRequest(
    string ExpectedAlertRowVersion,
    string ExpectedGroupRowVersion,
    IReadOnlyDictionary<string, string> ExpectedRoomRowVersions,
    IReadOnlyDictionary<string, string> ExpectedStaffTermAvailabilityRowVersions);

public sealed record ResolveScheduleImpactAlertRequest(
    string ExpectedAlertRowVersion,
    string Reason);
