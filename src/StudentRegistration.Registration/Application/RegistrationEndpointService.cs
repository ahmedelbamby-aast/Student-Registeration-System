using System.Security.Claims;

namespace StudentRegistration.Registration.Application;

public sealed record RegistrationMeetingStaffSnapshotDto(
    string Role,
    string DisplayName);

public sealed record RegistrationMeetingSnapshotDto(
    Guid MeetingId,
    string ActivityType,
    int DayOfWeek,
    string StartLocal,
    string EndLocal,
    string RoomCode,
    string Location,
    IReadOnlyList<RegistrationMeetingStaffSnapshotDto> Staff);

public sealed record RegistrationGroupSnapshotDto(
    Guid OfferingId,
    string CourseCode,
    string SubjectTitle,
    Guid GroupId,
    string GroupCode,
    decimal Credits,
    IReadOnlyList<RegistrationMeetingSnapshotDto> Meetings);

public sealed record RegistrationTermSnapshotDto(
    Guid Id,
    string Code,
    string DisplayName,
    string TimeZoneId);

public sealed record RegistrationReceiptSnapshotDto(
    RegistrationTermSnapshotDto Term,
    IReadOnlyList<RegistrationGroupSnapshotDto> Groups,
    decimal TotalCredits,
    Guid PolicySetId,
    string PolicyVersion,
    DateTime SubmittedAtUtc);

public sealed record RegistrationFinalResult(
    Guid SubmissionId,
    string Status,
    string ResultCode,
    IReadOnlyList<RegistrationGroupSnapshotDto> RegisteredGroups,
    DateTime ReceivedAtUtc,
    DateTime CompletedAtUtc,
    Guid PolicySetId,
    string PolicyVersion,
    string PlanRowVersion,
    string? Reference,
    RegistrationReceiptSnapshotDto? ReceiptSnapshot);

public sealed record RegistrationInProgressResponse(
    Guid ClientRequestId,
    string Status,
    int RetryAfterSeconds,
    string ResultUrl);

public enum RegistrationEndpointOutcome
{
    Created,
    Replayed,
    Processing,
    Invalid,
    Unauthorized,
    NotFound,
    Conflict,
    Unavailable,
}

public sealed record RegistrationEndpointResult(
    RegistrationEndpointOutcome Outcome,
    RegistrationFinalResult? FinalResult = null,
    RegistrationInProgressResponse? InProgress = null,
    string? ErrorCode = null,
    string? CurrentVersion = null);

public sealed record RegistrationCommandContext(
    ResolvedRegistrationContext ResolvedContext,
    string StudentTermStateRowVersion);

public interface IRegistrationEndpointStore
{
    Task<RegistrationCommandContext?> ResolveCommandContextAsync(
        Guid applicationUserId,
        Guid termId,
        CancellationToken cancellationToken = default);

    Task<RegistrationEndpointResult> SubmitAsync(
        RegistrationCommand command,
        RegistrationTransactionCoordinator coordinator,
        CancellationToken cancellationToken = default);

    Task<RegistrationEndpointResult> LookupAsync(
        Guid applicationUserId,
        Guid termId,
        Guid clientRequestId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Keeps authenticated command construction and endpoint result semantics in
/// the Registration application boundary. SQL transaction ownership remains
/// in the upstream SPEC-008 boundary consumed by the supplied coordinator.
/// </summary>
public sealed class RegistrationEndpointService(
    RegistrationCommandFactory commandFactory,
    IRegistrationEndpointStore store,
    TimeProvider timeProvider)
{
    public async Task<RegistrationEndpointResult> SubmitAsync(
        ClaimsPrincipal principal,
        Guid termId,
        SubmitRegistrationRequest request,
        RegistrationTransactionCoordinator coordinator,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(coordinator);

        if (!TryApplicationUserId(principal, out var applicationUserId))
        {
            return new(RegistrationEndpointOutcome.Unauthorized, ErrorCode: "UNAUTHORIZED");
        }

        // Capture once before the first I/O. Scheduled boundaries must use
        // authenticated ingress time, even if context resolution crosses a cutoff.
        var receivedAtUtc = timeProvider.GetUtcNow().UtcDateTime;

        var context = await store.ResolveCommandContextAsync(
            applicationUserId,
            termId,
            cancellationToken).ConfigureAwait(false);
        if (context is null)
        {
            return new(RegistrationEndpointOutcome.NotFound,
                ErrorCode: "REGISTRATION_CONTEXT_NOT_FOUND");
        }

        var creation = commandFactory.Create(
            principal,
            termId,
            request,
            context.ResolvedContext,
            receivedAtUtc);
        if (creation.Outcome is not RegistrationCommandCreationOutcome.Created ||
            creation.Command is null)
        {
            return new(
                creation.Outcome is RegistrationCommandCreationOutcome.Unauthorized
                    ? RegistrationEndpointOutcome.Unauthorized
                    : creation.Outcome is RegistrationCommandCreationOutcome.InvalidRequest
                        ? RegistrationEndpointOutcome.Invalid
                        : RegistrationEndpointOutcome.Conflict,
                ErrorCode: creation.ErrorCode);
        }

        return await store.SubmitAsync(
            creation.Command,
            coordinator,
            cancellationToken).ConfigureAwait(false);
    }

    public Task<RegistrationEndpointResult> LookupAsync(
        Guid applicationUserId,
        Guid termId,
        Guid clientRequestId,
        CancellationToken cancellationToken = default) =>
        store.LookupAsync(
            applicationUserId,
            termId,
            clientRequestId,
            cancellationToken);

    private static bool TryApplicationUserId(
        ClaimsPrincipal principal,
        out Guid applicationUserId)
    {
        applicationUserId = Guid.Empty;
        return principal.Identity?.IsAuthenticated == true &&
            Guid.TryParse(
                principal.FindFirstValue(ClaimTypes.NameIdentifier),
                out applicationUserId) &&
            applicationUserId != Guid.Empty;
    }
}
