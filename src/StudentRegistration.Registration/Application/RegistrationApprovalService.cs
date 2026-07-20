using System.Security.Claims;
using StudentRegistration.IdentityAccess.Application.Authorization;
using StudentRegistration.Registration.Domain;

namespace StudentRegistration.Registration.Application;

public sealed record DecideRegistrationLineRequest(
    string Decision,
    string Reason,
    string ExpectedSubmissionRowVersion,
    string ExpectedLineRowVersion,
    Guid ClientRequestId);

public sealed record RegistrationApprovalActor(
    Guid ApplicationUserId,
    RegistrationApprovalActorRole Role,
    bool DecideAll);

public sealed record RegistrationApprovalQueueRowDto(
    Guid SubmissionId,
    RegistrationSubmissionLineDto Line,
    string StudentUniversityId,
    string StudentDisplayName,
    decimal RequestedCredits,
    decimal CurrentCgpa,
    bool Overload,
    DateTime SubmittedAtUtc,
    DateTime WindowClosesAtUtc,
    string SubmissionVersion);

public sealed record RegistrationApprovalPageDto(
    IReadOnlyList<RegistrationApprovalQueueRowDto> Items,
    int Page,
    int PageSize,
    int TotalCount);

public enum RegistrationApprovalOutcome
{
    Found,
    Updated,
    Invalid,
    Unauthorized,
    NotFound,
    Conflict,
    Unavailable,
}

public sealed record RegistrationApprovalResult(
    RegistrationApprovalOutcome Outcome,
    RegistrationApprovalPageDto? Page = null,
    RegistrationApprovalQueueRowDto? Row = null,
    RegistrationFinalResult? Registration = null,
    string? ErrorCode = null,
    string? CurrentVersion = null);

public interface IRegistrationApprovalStore
{
    Task<RegistrationApprovalResult> ListAsync(
        RegistrationApprovalActor actor,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<RegistrationApprovalResult> FindAsync(
        RegistrationApprovalActor actor,
        Guid submissionId,
        Guid lineId,
        CancellationToken cancellationToken = default);

    Task<RegistrationApprovalResult> DecideAsync(
        RegistrationApprovalActor actor,
        Guid submissionId,
        Guid lineId,
        DecideRegistrationLineRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class RegistrationApprovalService(IRegistrationApprovalStore store)
{
    public Task<RegistrationApprovalResult> ListAsync(
        ClaimsPrincipal principal,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (!TryActor(principal, out var actor))
        {
            return Unauthorized();
        }

        if (page < 1 || pageSize is < 1 or > 100)
        {
            return Invalid();
        }

        return store.ListAsync(actor, page, pageSize, cancellationToken);
    }

    public Task<RegistrationApprovalResult> FindAsync(
        ClaimsPrincipal principal,
        Guid submissionId,
        Guid lineId,
        CancellationToken cancellationToken = default)
    {
        if (!TryActor(principal, out var actor))
        {
            return Unauthorized();
        }

        if (submissionId == Guid.Empty || lineId == Guid.Empty)
        {
            return Invalid();
        }

        return store.FindAsync(actor, submissionId, lineId, cancellationToken);
    }

    public Task<RegistrationApprovalResult> DecideAsync(
        ClaimsPrincipal principal,
        Guid submissionId,
        Guid lineId,
        DecideRegistrationLineRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!TryActor(principal, out var actor))
        {
            return Unauthorized();
        }

        var decision = request.Decision?.Trim().ToLowerInvariant();
        if (submissionId == Guid.Empty ||
            lineId == Guid.Empty ||
            decision is not ("approve" or "reject") ||
            string.IsNullOrWhiteSpace(request.Reason) ||
            string.IsNullOrWhiteSpace(request.ExpectedSubmissionRowVersion) ||
            string.IsNullOrWhiteSpace(request.ExpectedLineRowVersion) ||
            request.ClientRequestId == Guid.Empty)
        {
            return Invalid();
        }

        return store.DecideAsync(actor, submissionId, lineId, request, cancellationToken);
    }

    private static bool TryActor(
        ClaimsPrincipal principal,
        out RegistrationApprovalActor actor)
    {
        actor = null!;
        if (principal.Identity?.IsAuthenticated != true ||
            !Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ||
            userId == Guid.Empty)
        {
            return false;
        }

        if (principal.IsInRole(RolePolicies.Admin) &&
            principal.HasClaim(
                RolePolicies.PermissionClaimType,
                RolePolicies.RegistrationApprovalDecideAll))
        {
            actor = new(userId, RegistrationApprovalActorRole.Admin, true);
            return true;
        }

        if (principal.IsInRole(RolePolicies.Lecturer) &&
            principal.HasClaim(
                RolePolicies.PermissionClaimType,
                RolePolicies.RegistrationApprovalDecideAssigned))
        {
            actor = new(userId, RegistrationApprovalActorRole.Lecturer, false);
            return true;
        }

        if (principal.IsInRole(RolePolicies.TeachingAssistant) &&
            principal.HasClaim(
                RolePolicies.PermissionClaimType,
                RolePolicies.RegistrationApprovalDecideAssigned))
        {
            actor = new(userId, RegistrationApprovalActorRole.TeachingAssistant, false);
            return true;
        }

        return false;
    }

    private static Task<RegistrationApprovalResult> Unauthorized() =>
        Task.FromResult(new RegistrationApprovalResult(
            RegistrationApprovalOutcome.Unauthorized,
            ErrorCode: "UNAUTHORIZED"));

    private static Task<RegistrationApprovalResult> Invalid() =>
        Task.FromResult(new RegistrationApprovalResult(
            RegistrationApprovalOutcome.Invalid,
            ErrorCode: "VALIDATION_ERROR"));
}
