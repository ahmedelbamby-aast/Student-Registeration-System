using System.Security.Claims;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Registration;
using StudentRegistration.Registration.Application.Ports;

namespace StudentRegistration.Registration.Application;

public enum RegistrationRecordQueryOutcome
{
    Succeeded,
    Invalid,
    Unauthorized,
    NotFound,
    Conflict,
    Unavailable
}

public sealed record RegistrationRecordListResult(
    RegistrationRecordQueryOutcome Outcome,
    Page<RegistrationHistoryRowDto>? Page = null,
    string? ErrorCode = null);
public sealed record RegistrationRecordDetailResult(
    RegistrationRecordQueryOutcome Outcome,
    RegistrationDetailDto? Detail = null,
    string? ErrorCode = null);
public sealed record RegistrationTimetableResult(
    RegistrationRecordQueryOutcome Outcome,
    RegistrationTimetableDto? Timetable = null,
    string? ErrorCode = null);

public sealed class RegistrationRecordQueries(
    IRegistrationRecordReader reader,
    RegistrationReceiptService receipts)
{
    public const string StableSort = "submittedAtUtc:desc,submissionId:asc";

    public async Task<RegistrationRecordListResult> ListOwnAsync(
        ClaimsPrincipal principal, int page, int pageSize, Guid? termId,
        CancellationToken cancellationToken = default)
    {
        if (!TryUserId(principal, out var userId)) return UnauthorizedList();
        if (!ValidPage(page, pageSize)) return InvalidList("PAGE_SIZE_INVALID");
        if (termId == Guid.Empty) return InvalidList("TERM_ID_INVALID");
        var snapshot = await reader.ListOwnAsync(
            userId, termId, page, pageSize, cancellationToken);
        return snapshot is null ? NotFoundList() : ProjectPage(snapshot, page, pageSize);
    }

    public async Task<RegistrationRecordDetailResult> ReadOwnAsync(
        ClaimsPrincipal principal, Guid submissionId,
        CancellationToken cancellationToken = default)
    {
        if (!TryUserId(principal, out var userId)) return UnauthorizedDetail();
        if (submissionId == Guid.Empty) return NotFoundDetail();
        var record = await reader.ReadOwnAsync(userId, submissionId, cancellationToken);
        return record is null ? NotFoundDetail() : ProjectDetail(record);
    }

    public async Task<RegistrationTimetableResult> ReadCurrentAsync(
        ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        if (!TryUserId(principal, out var userId))
            return new(RegistrationRecordQueryOutcome.Unauthorized, ErrorCode: "UNAUTHORIZED");
        var snapshot = await reader.ReadCurrentAsync(userId, cancellationToken);
        if (snapshot is null)
            return new(RegistrationRecordQueryOutcome.NotFound, ErrorCode: "STUDENT_NOT_FOUND");
        if (!string.IsNullOrWhiteSpace(snapshot.ErrorCode))
            return new(RegistrationRecordQueryOutcome.Conflict, ErrorCode: snapshot.ErrorCode);
        try { return new(RegistrationRecordQueryOutcome.Succeeded, receipts.ProjectTimetable(snapshot)); }
        catch (RegistrationRecordProjectionException)
        { return new(RegistrationRecordQueryOutcome.Unavailable, ErrorCode: "REGISTRATION_RECORD_UNAVAILABLE"); }
    }

    public async Task<RegistrationRecordListResult> ListAdminAsync(
        ClaimsPrincipal principal, Guid studentId, Guid termId,
        int page, int pageSize, string correlationId,
        CancellationToken cancellationToken = default)
    {
        if (!TryUserId(principal, out var actorId)) return UnauthorizedList();
        if (studentId == Guid.Empty || termId == Guid.Empty) return NotFoundList();
        if (!ValidPage(page, pageSize)) return InvalidList("PAGE_SIZE_INVALID");
        var snapshot = await reader.ListAdminAsync(actorId, studentId, termId,
            page, pageSize, RequiredCorrelation(correlationId), cancellationToken);
        return snapshot is null ? NotFoundList() : ProjectPage(snapshot, page, pageSize);
    }

    public async Task<RegistrationRecordDetailResult> ReadAdminAsync(
        ClaimsPrincipal principal, Guid studentId, Guid termId, Guid submissionId,
        string correlationId, CancellationToken cancellationToken = default)
    {
        if (!TryUserId(principal, out var actorId)) return UnauthorizedDetail();
        if (studentId == Guid.Empty || termId == Guid.Empty || submissionId == Guid.Empty)
            return NotFoundDetail();
        var record = await reader.ReadAdminAsync(actorId, studentId, termId,
            submissionId, RequiredCorrelation(correlationId), cancellationToken);
        return record is null ? NotFoundDetail() : ProjectDetail(record);
    }

    private RegistrationRecordListResult ProjectPage(
        RegistrationRecordPageSnapshot snapshot, int page, int pageSize)
    {
        try
        {
            return new(RegistrationRecordQueryOutcome.Succeeded,
                new Page<RegistrationHistoryRowDto>(
                    snapshot.Items.Select(receipts.ProjectHistory).ToArray(),
                    page, pageSize, snapshot.TotalCount, StableSort));
        }
        catch (RegistrationRecordProjectionException)
        { return new(RegistrationRecordQueryOutcome.Unavailable, ErrorCode: "REGISTRATION_RECORD_UNAVAILABLE"); }
    }

    private RegistrationRecordDetailResult ProjectDetail(RegistrationSubmissionRecord record)
    {
        try { return new(RegistrationRecordQueryOutcome.Succeeded, receipts.ProjectDetail(record)); }
        catch (RegistrationRecordProjectionException)
        { return new(RegistrationRecordQueryOutcome.Unavailable, ErrorCode: "REGISTRATION_RECORD_UNAVAILABLE"); }
    }

    private static bool TryUserId(ClaimsPrincipal principal, out Guid id)
    {
        id = Guid.Empty;
        return principal?.Identity?.IsAuthenticated == true &&
            Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out id) && id != Guid.Empty;
    }
    private static bool ValidPage(int page, int size) =>
        page >= 1 && size is >= 1 and <= 100 && page <= int.MaxValue / size;
    private static string RequiredCorrelation(string value) =>
        !string.IsNullOrWhiteSpace(value) ? value : Guid.NewGuid().ToString("N");
    private static RegistrationRecordListResult UnauthorizedList() => new(RegistrationRecordQueryOutcome.Unauthorized, ErrorCode: "UNAUTHORIZED");
    private static RegistrationRecordListResult InvalidList(string code) => new(RegistrationRecordQueryOutcome.Invalid, ErrorCode: code);
    private static RegistrationRecordListResult NotFoundList() => new(RegistrationRecordQueryOutcome.NotFound, ErrorCode: "REGISTRATION_NOT_FOUND");
    private static RegistrationRecordDetailResult UnauthorizedDetail() => new(RegistrationRecordQueryOutcome.Unauthorized, ErrorCode: "UNAUTHORIZED");
    private static RegistrationRecordDetailResult NotFoundDetail() => new(RegistrationRecordQueryOutcome.NotFound, ErrorCode: "REGISTRATION_NOT_FOUND");
}
