namespace StudentRegistration.Contracts.Registration;

public sealed record RegistrationTermSnapshotDto(
    Guid Id,
    string Code,
    string DisplayName,
    string TimeZoneId);

public sealed record RegistrationRecordMeetingStaffDto(
    string Role,
    string DisplayName);

public sealed record RegistrationRecordMeetingDto(
    Guid MeetingId,
    string ActivityType,
    int DayOfWeek,
    string StartLocal,
    string EndLocal,
    string RoomCode,
    string Location,
    IReadOnlyList<RegistrationRecordMeetingStaffDto> Staff);

public sealed record RegistrationRecordGroupDto(
    Guid OfferingId,
    string CourseCode,
    string SubjectTitle,
    Guid GroupId,
    string GroupCode,
    decimal Credits,
    IReadOnlyList<RegistrationRecordMeetingDto> Meetings);

public sealed record RegistrationReceiptDto(
    Guid SubmissionId,
    string Reference,
    RegistrationTermSnapshotDto Term,
    DateTime SubmittedAtUtc,
    string ResultCode,
    string PolicyVersion,
    IReadOnlyList<RegistrationRecordGroupDto> Groups,
    decimal TotalCredits);

public sealed record RegistrationHistoryRowDto(
    Guid SubmissionId,
    string? Reference,
    RegistrationTermSnapshotDto Term,
    string Status,
    DateTime SubmittedAtUtc,
    int GroupCount,
    decimal TotalCredits,
    string TermState);

public sealed record RegistrationRejectedResultDto(
    Guid SubmissionId,
    string Status,
    string ResultCode,
    string SafeMessage,
    DateTime SubmittedAtUtc,
    DateTime CompletedAtUtc,
    bool NoPartialRegistration);

public sealed record RegistrationDetailDto(
    string Status,
    RegistrationReceiptDto? Receipt,
    RegistrationRejectedResultDto? Rejection);

public sealed record RegistrationTimetableDto(
    RegistrationTermSnapshotDto? Term,
    string TermState,
    string RegistrationWindowState,
    IReadOnlyList<RegistrationRecordGroupDto> Groups,
    string? SubjectDiscoveryPath);
