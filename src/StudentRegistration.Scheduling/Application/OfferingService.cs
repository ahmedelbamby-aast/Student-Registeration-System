using StudentRegistration.Scheduling.Application.Ports;

namespace StudentRegistration.Scheduling.Application;

public enum OfferingOutcome
{
    Created,
    Updated,
    Found,
    NotFound,
    ValidationError,
}

public sealed record CreateOfferingGroup(string Code, int Capacity);

public sealed record CreateOfferingCommand(
    Guid TermId,
    Guid CourseId,
    IReadOnlyList<CreateOfferingGroup> Groups,
    string Reason);

public sealed record CreateOfferingStoreCommand(
    Guid TermId,
    Guid CourseId,
    IReadOnlyList<CreateOfferingGroup> Groups,
    string Reason);

public sealed record UpdateMeetingInput(
    Guid Id,
    Guid RoomId,
    string ActivityType,
    int DayOfWeek,
    TimeOnly StartLocal,
    TimeOnly EndLocal);

public sealed record UpdateStaffAssignmentInput(
    Guid MeetingSlotId,
    Guid StaffId,
    string Role);

public sealed record UpdateGroupCommand(
    Guid GroupId,
    byte[] ExpectedOfferingRowVersion,
    byte[] ExpectedGroupRowVersion,
    string GroupCode,
    int Capacity,
    bool RegistrationPaused,
    IReadOnlyList<UpdateMeetingInput> Meetings,
    IReadOnlyList<UpdateStaffAssignmentInput> StaffAssignments,
    string ActorId,
    string Reason);

public sealed record UpdateGroupStoreCommand(
    Guid GroupId,
    byte[] ExpectedOfferingRowVersion,
    byte[] ExpectedGroupRowVersion,
    string GroupCode,
    int Capacity,
    bool RegistrationPaused,
    IReadOnlyList<UpdateMeetingInput> Meetings,
    IReadOnlyList<UpdateStaffAssignmentInput> StaffAssignments,
    string ActorId,
    string Reason);

public sealed record OfferingStaffSnapshot(Guid Id, string Role, string Name);

public sealed record OfferingMeetingSnapshot(
    Guid Id,
    string ActivityType,
    int DayOfWeek,
    TimeOnly StartLocal,
    TimeOnly EndLocal,
    string RoomCode,
    string Location,
    IReadOnlyList<OfferingStaffSnapshot> Staff);

public sealed record OfferingGroupSnapshot(
    Guid Id,
    string GroupCode,
    int Capacity,
    int EnrolledCount,
    bool RegistrationPaused,
    string State,
    byte[] RowVersion,
    IReadOnlyList<OfferingMeetingSnapshot> Meetings);

public sealed record OfferingSnapshot(
    Guid Id,
    string State,
    IReadOnlyList<OfferingGroupSnapshot> Groups);

public sealed record AdminOfferingQuery(
    Guid? TermId,
    string? State,
    string? Query,
    int Page,
    int PageSize,
    string? Sort);

public sealed record AdminOfferingPage(
    IReadOnlyList<OfferingSnapshot> Items,
    int Page,
    int PageSize,
    int TotalCount,
    string Sort);

public sealed record AdminOfferingListResult(
    OfferingOutcome Outcome,
    AdminOfferingPage? Page = null,
    string? ErrorCode = null);

public sealed record StudentOfferingMeeting(
    Guid Id,
    string ActivityType,
    string DisplayLabel,
    int DayOfWeek,
    TimeOnly StartLocal,
    TimeOnly EndLocal,
    string RoomCode,
    string Location,
    IReadOnlyList<OfferingStaffSnapshot> Staff);

public sealed record StudentOfferingGroup(
    Guid Id,
    string GroupCode,
    int Capacity,
    int EnrolledCount,
    bool RegistrationPaused,
    string State,
    byte[] RowVersion,
    IReadOnlyList<StudentOfferingMeeting> Meetings);

public sealed record StudentOfferingDetail(
    Guid Id,
    string State,
    IReadOnlyList<StudentOfferingGroup> Groups);

public sealed record OfferingBundleValidationResult(
    bool Valid,
    IReadOnlyList<string> ReasonCodes);

public sealed record OfferingResult(
    OfferingOutcome Outcome,
    OfferingSnapshot? Offering = null,
    string? ErrorCode = null,
    byte[]? GroupRowVersion = null);

public sealed class OfferingService(IOfferingStore store)
{
    private static readonly HashSet<string> AdminOfferingSorts =
        new(StringComparer.Ordinal)
        {
            "courseCode,id",
            "courseCode-desc,id",
            "state,id",
            "state-desc,id",
        };

    public async Task<OfferingResult> CreateAsync(
        CreateOfferingCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.Groups is null || command.Groups.Count == 0)
        {
            return new(OfferingOutcome.ValidationError, ErrorCode: "GROUP_REQUIRED");
        }

        var offering = await store.CreateAsync(
            new(
                command.TermId,
                command.CourseId,
                command.Groups,
                command.Reason),
            cancellationToken);
        return new(OfferingOutcome.Created, offering);
    }

    public async Task<OfferingBundleValidationResult> ValidateForPublicationAsync(
        Guid offeringId,
        CancellationToken cancellationToken = default)
    {
        var offering = await store.LoadAsync(offeringId, cancellationToken);
        if (offering is null)
        {
            return new(false, ["OFFERING_NOT_FOUND"]);
        }

        var reasons = new List<string>();
        foreach (var group in offering.Groups)
        {
            var lectureMeetings = group.Meetings
                .Where(meeting => Is(meeting.ActivityType, "Lecture"))
                .ToArray();
            var tutorialMeetings = group.Meetings
                .Where(meeting => Is(meeting.ActivityType, "Tutorial"))
                .ToArray();
            var laboratoryMeetings = group.Meetings
                .Where(meeting => Is(meeting.ActivityType, "Laboratory"))
                .ToArray();

            AddWhen(reasons, lectureMeetings.Length == 0, "MISSING_LECTURE");
            AddWhen(
                reasons,
                lectureMeetings.Length > 0
                    && lectureMeetings.All(meeting =>
                        meeting.Staff.All(staff => !Is(staff.Role, "Lecturer"))),
                "MISSING_LECTURER");
            AddWhen(
                reasons,
                tutorialMeetings.Length == 0 && laboratoryMeetings.Length == 0,
                "MISSING_TUTORIAL_OR_LABORATORY");
            AddWhen(
                reasons,
                tutorialMeetings.Any(meeting =>
                    meeting.Staff.All(staff =>
                        !Is(staff.Role, "TeachingAssistant"))),
                "MISSING_TEACHING_ASSISTANT");
            AddWhen(
                reasons,
                laboratoryMeetings.Any(meeting =>
                    meeting.Staff.All(staff =>
                        !Is(staff.Role, "TeachingAssistant"))),
                "MISSING_TEACHING_ASSISTANT");
        }

        return new(reasons.Count == 0, reasons);
    }

    public async Task<StudentOfferingDetail?> GetStudentDetailAsync(
        Guid offeringId,
        CancellationToken cancellationToken = default)
    {
        var offering = await store.LoadAsync(offeringId, cancellationToken);
        return offering is null
            ? null
            : new(
                offering.Id,
                offering.State,
                offering.Groups.Select(group =>
                    new StudentOfferingGroup(
                        group.Id,
                        group.GroupCode,
                        group.Capacity,
                        group.EnrolledCount,
                        group.RegistrationPaused,
                        group.State,
                        group.RowVersion,
                        group.Meetings.Select(meeting =>
                            new StudentOfferingMeeting(
                                meeting.Id,
                                meeting.ActivityType,
                                Is(meeting.ActivityType, "Tutorial")
                                    ? "Section"
                                    : meeting.ActivityType,
                                meeting.DayOfWeek,
                                meeting.StartLocal,
                                meeting.EndLocal,
                                meeting.RoomCode,
                                meeting.Location,
                                meeting.Staff)).ToArray())).ToArray());
    }

    public async Task<OfferingResult> UpdateGroupAsync(
        UpdateGroupCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var version = await store.UpdateGroupAsync(
            new(
                command.GroupId,
                command.ExpectedOfferingRowVersion,
                command.ExpectedGroupRowVersion,
                command.GroupCode,
                command.Capacity,
                command.RegistrationPaused,
                command.Meetings,
                command.StaffAssignments,
                command.ActorId,
                command.Reason),
            cancellationToken);
        return new(OfferingOutcome.Updated, GroupRowVersion: version);
    }

    public async Task<AdminOfferingListResult> ListAdminOfferingsAsync(
        AdminOfferingQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.PageSize is < 1 or > 100)
        {
            return new(
                OfferingOutcome.ValidationError,
                ErrorCode: "PAGE_SIZE_INVALID");
        }
        if (query.Page < 1)
        {
            return new(
                OfferingOutcome.ValidationError,
                ErrorCode: "PAGE_INVALID");
        }

        if (query.TermId == Guid.Empty)
        {
            return new(
                OfferingOutcome.ValidationError,
                ErrorCode: "VALIDATION_ERROR");
        }

        var normalizedQuery = query.Query?.Trim();
        if (normalizedQuery is not null
            && normalizedQuery.Length is < 3 or > 50)
        {
            return new(
                OfferingOutcome.ValidationError,
                ErrorCode: "VALIDATION_ERROR");
        }

        var normalizedState = query.State?.Trim().ToLowerInvariant();
        if (normalizedState is not null
            && normalizedState is not
                ("draft" or "published" or "closed" or "cancelled"))
        {
            return new(
                OfferingOutcome.ValidationError,
                ErrorCode: "VALIDATION_ERROR");
        }

        var normalizedSort = string.IsNullOrWhiteSpace(query.Sort)
            ? "courseCode,id"
            : query.Sort.Trim();
        if (!AdminOfferingSorts.Contains(normalizedSort))
        {
            return new(
                OfferingOutcome.ValidationError,
                ErrorCode: "VALIDATION_ERROR");
        }

        var normalized = query with
        {
            State = normalizedState,
            Query = normalizedQuery,
            Sort = normalizedSort,
        };
        var page = await store.ListAsync(normalized, cancellationToken);
        return new(OfferingOutcome.Found, page);
    }

    private static bool Is(string value, string expected) =>
        string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);

    private static void AddWhen(
        List<string> reasons,
        bool condition,
        string reason)
    {
        if (condition && !reasons.Contains(reason, StringComparer.Ordinal))
        {
            reasons.Add(reason);
        }
    }
}
