using System.Security.Claims;
using StudentRegistration.Registration.Domain;

namespace StudentRegistration.Registration.Application;

public sealed record SubmitRegistrationRequest(
    Guid PlanId,
    string ExpectedPlanRowVersion,
    Guid ClientRequestId);

public sealed record ResolvedRegistrationContext(
    Guid ApplicationUserId,
    Guid StudentId,
    Guid TermId,
    string Version,
    DateTime ScheduledOpenUtc,
    DateTime ScheduledCloseUtc,
    bool EmergencyClosed);

public sealed record RegistrationIdempotencyScope(
    Guid StudentId,
    Guid TermId,
    Guid ClientRequestId);

public sealed record RegistrationCommand(
    Guid ApplicationUserId,
    Guid StudentId,
    Guid TermId,
    Guid PlanId,
    string ExpectedPlanRowVersion,
    Guid ClientRequestId,
    RegistrationIdempotencyScope IdempotencyScope,
    string ExpectedRegistrationContextVersion,
    DateTime ReceivedAtUtc,
    RegistrationSubmissionOrigin Origin = RegistrationSubmissionOrigin.StudentSelfService);

public enum RegistrationCommandCreationOutcome
{
    Created,
    Unauthorized,
    RegistrationContextNotFound,
    InvalidRequest,
    WindowClosed,
    WindowChanged
}

public sealed record RegistrationCommandCreationResult(
    RegistrationCommandCreationOutcome Outcome,
    RegistrationCommand? Command = null,
    string? ErrorCode = null);

public sealed class RegistrationCommandFactory(TimeProvider timeProvider)
{
    public RegistrationCommandCreationResult Create(
        ClaimsPrincipal principal,
        Guid routeTermId,
        SubmitRegistrationRequest request,
        ResolvedRegistrationContext registrationContext) =>
        Create(
            principal,
            routeTermId,
            request,
            registrationContext,
            timeProvider.GetUtcNow().UtcDateTime);

    public RegistrationCommandCreationResult Create(
        ClaimsPrincipal principal,
        Guid routeTermId,
        SubmitRegistrationRequest request,
        ResolvedRegistrationContext registrationContext,
        DateTime receivedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(registrationContext);

        if (receivedAtUtc.Kind is not DateTimeKind.Utc)
        {
            return new(
                RegistrationCommandCreationOutcome.InvalidRequest,
                ErrorCode: "VALIDATION_ERROR");
        }

        if (principal.Identity?.IsAuthenticated != true ||
            !Guid.TryParse(
                principal.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                out var applicationUserId) ||
            applicationUserId == Guid.Empty)
        {
            return new(
                RegistrationCommandCreationOutcome.Unauthorized,
                ErrorCode: "UNAUTHORIZED");
        }

        if (applicationUserId != registrationContext.ApplicationUserId ||
            routeTermId == Guid.Empty ||
            routeTermId != registrationContext.TermId ||
            registrationContext.StudentId == Guid.Empty)
        {
            return new(
                RegistrationCommandCreationOutcome.RegistrationContextNotFound,
                ErrorCode: "REGISTRATION_CONTEXT_NOT_FOUND");
        }

        if (request.PlanId == Guid.Empty ||
            request.ClientRequestId == Guid.Empty ||
            string.IsNullOrWhiteSpace(request.ExpectedPlanRowVersion) ||
            string.IsNullOrWhiteSpace(registrationContext.Version) ||
            registrationContext.ScheduledOpenUtc.Kind != DateTimeKind.Utc ||
            registrationContext.ScheduledCloseUtc.Kind != DateTimeKind.Utc ||
            registrationContext.ScheduledCloseUtc <=
                registrationContext.ScheduledOpenUtc)
        {
            return new(
                RegistrationCommandCreationOutcome.InvalidRequest,
                ErrorCode: "VALIDATION_ERROR");
        }

        if (registrationContext.EmergencyClosed)
        {
            return new(
                RegistrationCommandCreationOutcome.WindowChanged,
                ErrorCode: "WINDOW_CHANGED");
        }

        if (receivedAtUtc < registrationContext.ScheduledOpenUtc ||
            receivedAtUtc >= registrationContext.ScheduledCloseUtc)
        {
            return new(
                RegistrationCommandCreationOutcome.WindowClosed,
                ErrorCode: "WINDOW_CLOSED");
        }

        var scope = new RegistrationIdempotencyScope(
            registrationContext.StudentId,
            routeTermId,
            request.ClientRequestId);
        return new(
            RegistrationCommandCreationOutcome.Created,
            new RegistrationCommand(
                applicationUserId,
                registrationContext.StudentId,
                routeTermId,
                request.PlanId,
                request.ExpectedPlanRowVersion.Trim(),
                request.ClientRequestId,
                scope,
                registrationContext.Version,
                receivedAtUtc));
    }
}
