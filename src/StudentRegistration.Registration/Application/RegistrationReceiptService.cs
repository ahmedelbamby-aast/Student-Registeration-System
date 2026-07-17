using System.Text.Json;
using StudentRegistration.Contracts.Registration;
using StudentRegistration.Registration.Application.Ports;
using ContractRegistrationTermSnapshotDto = StudentRegistration.Contracts.Registration.RegistrationTermSnapshotDto;

namespace StudentRegistration.Registration.Application;

public sealed class RegistrationRecordProjectionException : Exception
{
    public RegistrationRecordProjectionException(string message, Exception? inner = null)
        : base(message, inner) { }
}

/// <summary>Projects only the immutable JSON committed by SPEC-014.</summary>
public sealed class RegistrationReceiptService
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public RegistrationDetailDto ProjectDetail(RegistrationSubmissionRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        return string.Equals(record.State, "accepted", StringComparison.OrdinalIgnoreCase)
            ? new("accepted", ProjectAccepted(record), null)
            : new("rejected", null, ProjectRejected(record));
    }

    public RegistrationHistoryRowDto ProjectHistory(RegistrationSubmissionRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        if (!string.Equals(record.State, "accepted", StringComparison.OrdinalIgnoreCase))
        {
            return new(record.SubmissionId, null, record.FallbackTerm, "rejected",
                record.SubmittedAtUtc, 0, 0m, record.TermState);
        }

        var receipt = ProjectAccepted(record);
        return new(record.SubmissionId, receipt.Reference, receipt.Term, "accepted",
            receipt.SubmittedAtUtc, receipt.Groups.Count, receipt.TotalCredits,
            record.TermState);
    }

    public RegistrationTimetableDto ProjectTimetable(
        RegistrationCurrentTimetableSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        var groups = snapshot.AcceptedSubmissions
            .Select(ProjectAccepted)
            .SelectMany(receipt => receipt.Groups)
            .GroupBy(group => group.GroupId)
            .Select(group => group.First())
            .OrderBy(group => group.CourseCode, StringComparer.Ordinal)
            .ThenBy(group => group.GroupCode, StringComparer.Ordinal)
            .ToArray();
        var discoveryPath = string.Equals(
                snapshot.RegistrationWindowState, "open", StringComparison.Ordinal)
            ? snapshot.SubjectDiscoveryPath
            : null;
        return new(snapshot.Term, snapshot.TermState,
            snapshot.RegistrationWindowState, groups, discoveryPath);
    }

    private static RegistrationReceiptDto ProjectAccepted(
        RegistrationSubmissionRecord record)
    {
        if (string.IsNullOrWhiteSpace(record.Reference) ||
            string.IsNullOrWhiteSpace(record.ReceiptSnapshotJson))
        {
            throw new RegistrationRecordProjectionException(
                "An accepted registration has no canonical receipt snapshot.");
        }

        try
        {
            var snapshot = JsonSerializer.Deserialize<PersistedReceipt>(
                record.ReceiptSnapshotJson, Json);
            var decision = JsonSerializer.Deserialize<PersistedDecision>(
                record.DecisionSnapshotJson, Json);
            if (snapshot is null || decision is null || snapshot.Term is null ||
                snapshot.Groups is null || snapshot.Groups.Count == 0 ||
                string.IsNullOrWhiteSpace(snapshot.PolicyVersion) ||
                string.IsNullOrWhiteSpace(decision.PolicyVersion) ||
                !string.Equals(snapshot.PolicyVersion, decision.PolicyVersion,
                    StringComparison.Ordinal) ||
                snapshot.SubmittedAtUtc.Kind is not DateTimeKind.Utc)
            {
                throw new RegistrationRecordProjectionException(
                    "The canonical receipt snapshot is incomplete.");
            }

            var term = new ContractRegistrationTermSnapshotDto(
                RequiredId(snapshot.Term.Id), Required(snapshot.Term.Code),
                Required(snapshot.Term.DisplayName), Required(snapshot.Term.TimeZoneId));
            var groups = snapshot.Groups.Select(group => new RegistrationRecordGroupDto(
                RequiredId(group.OfferingId), Required(group.CourseCode),
                Required(group.SubjectTitle), RequiredId(group.GroupId),
                Required(group.GroupCode), group.Credits,
                (group.Meetings ?? throw new RegistrationRecordProjectionException(
                    "A receipt group has no Meetings collection."))
                .Select(meeting => new RegistrationRecordMeetingDto(
                    RequiredId(meeting.MeetingId), Required(meeting.ActivityType),
                    meeting.DayOfWeek, Required(meeting.StartLocal),
                    Required(meeting.EndLocal), Required(meeting.RoomCode),
                    Required(meeting.Location),
                    (meeting.Staff ?? []).Select(staff =>
                        new RegistrationRecordMeetingStaffDto(
                            Required(staff.Role), Required(staff.DisplayName))).ToArray()))
                .ToArray())).ToArray();

            return new(record.SubmissionId, record.Reference, term,
                snapshot.SubmittedAtUtc, record.ResultCode,
                snapshot.PolicyVersion, groups, snapshot.TotalCredits);
        }
        catch (RegistrationRecordProjectionException) { throw; }
        catch (Exception exception) when (exception is JsonException or ArgumentException)
        {
            throw new RegistrationRecordProjectionException(
                "The canonical receipt snapshot could not be read.", exception);
        }
    }

    private static RegistrationRejectedResultDto ProjectRejected(
        RegistrationSubmissionRecord record) => new(
            record.SubmissionId,
            "rejected",
            record.ResultCode,
            SafeRejectionMessage(record.ResultCode),
            record.SubmittedAtUtc,
            record.CompletedAtUtc,
            NoPartialRegistration: true);

    private static string SafeRejectionMessage(string resultCode) => resultCode switch
    {
        "GROUP_FULL" => "The selected group became full. Choose another group and try again. No subjects were partially registered.",
        "SCHEDULE_CONFLICT" => "The schedule is no longer valid. Review the plan and try again. No subjects were partially registered.",
        "WINDOW_CLOSED" or "WINDOW_CHANGED" => "The registration window changed. Refresh before trying again. No subjects were partially registered.",
        _ => "The registration was rejected. Review the current plan before trying again. No subjects were partially registered."
    };

    private static string Required(string? value) =>
        !string.IsNullOrWhiteSpace(value) ? value : throw new RegistrationRecordProjectionException(
            "A required historical value is missing.");

    private static Guid RequiredId(Guid value) => value != Guid.Empty
        ? value
        : throw new RegistrationRecordProjectionException(
            "A required historical identifier is missing.");

    private sealed record PersistedReceipt(
        PersistedTerm Term,
        IReadOnlyList<PersistedGroup> Groups,
        decimal TotalCredits,
        Guid PolicySetId,
        string PolicyVersion,
        DateTime SubmittedAtUtc);
    private sealed record PersistedTerm(Guid Id, string Code, string DisplayName, string TimeZoneId);
    private sealed record PersistedGroup(Guid OfferingId, string CourseCode,
        string SubjectTitle, Guid GroupId, string GroupCode, decimal Credits,
        IReadOnlyList<PersistedMeeting> Meetings);
    private sealed record PersistedMeeting(Guid MeetingId, string ActivityType,
        int DayOfWeek, string StartLocal, string EndLocal, string RoomCode,
        string Location, IReadOnlyList<PersistedStaff> Staff);
    private sealed record PersistedStaff(string Role, string DisplayName);
    private sealed record PersistedDecision(string PolicyVersion);
}
