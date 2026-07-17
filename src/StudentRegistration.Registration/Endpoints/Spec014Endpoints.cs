using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using StudentRegistration.Contracts;
using StudentRegistration.Registration.Application;

namespace StudentRegistration.Registration.Endpoints;

public static class Spec014Endpoints
{
    private const string Student = "Student";
    private const string RegistrationSubmitOwn = "Registration.SubmitOwn";

    public static IEndpointRouteBuilder MapSpec014Endpoints(
        this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapPost(
                "/api/student/terms/{termId}/registrations",
                SubmitRegistration)
            .RequireAuthorization(RegistrationSubmitOwn)
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .Produces<RegistrationFinalResult>(StatusCodes.Status201Created)
            .Produces<RegistrationFinalResult>(StatusCodes.Status200OK)
            .Produces<RegistrationInProgressResponse>(StatusCodes.Status202Accepted)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status401Unauthorized)
            .Produces<ApiError>(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status409Conflict)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable);

        endpoints.MapGet(
                "/api/student/terms/{termId}/registrations/by-request/{clientRequestId}",
                GetRegistrationByRequest)
            .RequireAuthorization(RegistrationSubmitOwn)
            .Produces<RegistrationFinalResult>(StatusCodes.Status200OK)
            .Produces<RegistrationInProgressResponse>(StatusCodes.Status202Accepted)
            .Produces<ApiError>(StatusCodes.Status401Unauthorized)
            .Produces<ApiError>(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable);

        return endpoints;
    }

    private static async Task<IResult> SubmitRegistration(
        [FromRoute] Guid termId,
        [FromBody] SubmitRegistrationRequest request,
        HttpContext context,
        [FromServices] RegistrationEndpointService service,
        [FromServices] RegistrationTransactionCoordinator coordinator,
        CancellationToken cancellationToken)
    {
        if (!TryApplicationUserId(context, out _))
        {
            return Error(context, StatusCodes.Status401Unauthorized,
                "UNAUTHORIZED", "Authentication is required.");
        }

        var result = await service.SubmitAsync(
            context.User,
            termId,
            request,
            coordinator,
            cancellationToken);
        return ToPostResult(context, result);
    }

    private static async Task<IResult> GetRegistrationByRequest(
        [FromRoute] Guid termId,
        [FromRoute] Guid clientRequestId,
        HttpContext context,
        [FromServices] RegistrationEndpointService service,
        CancellationToken cancellationToken)
    {
        // Authorization middleware runs before this handler, so lookup cannot
        // disclose result, owner, term, submission, or version information.
        if (!TryApplicationUserId(context, out var applicationUserId))
        {
            return Error(context, StatusCodes.Status401Unauthorized,
                "UNAUTHORIZED", "Authentication is required.");
        }

        if (termId == Guid.Empty || clientRequestId == Guid.Empty)
        {
            return RequestNotFound(context);
        }

        var result = await service.LookupAsync(
            applicationUserId,
            termId,
            clientRequestId,
            cancellationToken);
        return result.Outcome switch
        {
            RegistrationEndpointOutcome.Replayed when result.FinalResult is not null =>
                Results.Ok(result.FinalResult),
            RegistrationEndpointOutcome.Processing when result.InProgress is not null =>
                Results.Json(result.InProgress,
                    statusCode: StatusCodes.Status202Accepted),
            RegistrationEndpointOutcome.Unavailable => Error(
                context, StatusCodes.Status503ServiceUnavailable,
                "REGISTRATION_UNAVAILABLE",
                "Registration is temporarily unavailable."),
            _ => RequestNotFound(context),
        };
    }

    private static IResult ToPostResult(
        HttpContext context,
        RegistrationEndpointResult result) => result.Outcome switch
        {
            RegistrationEndpointOutcome.Created when result.FinalResult is not null =>
                Results.Json(result.FinalResult,
                    statusCode: StatusCodes.Status201Created),
            RegistrationEndpointOutcome.Replayed when result.FinalResult is not null =>
                Results.Json(result.FinalResult,
                    statusCode: StatusCodes.Status200OK),
            RegistrationEndpointOutcome.Processing when result.InProgress is not null =>
                Results.Json(result.InProgress,
                    statusCode: StatusCodes.Status202Accepted),
            RegistrationEndpointOutcome.Invalid => Error(
                context, StatusCodes.Status400BadRequest,
                result.ErrorCode ?? "VALIDATION_ERROR",
                "The registration request is invalid."),
            RegistrationEndpointOutcome.Unauthorized => Error(
                context, StatusCodes.Status401Unauthorized,
                "UNAUTHORIZED", "Authentication is required."),
            RegistrationEndpointOutcome.Conflict => Error(
                context, StatusCodes.Status409Conflict,
                result.ErrorCode ?? "REGISTRATION_CONFLICT",
                "The registration could not be committed.",
                result.CurrentVersion),
            RegistrationEndpointOutcome.NotFound => Error(
                context, StatusCodes.Status409Conflict,
                result.ErrorCode ?? "REGISTRATION_CONTEXT_NOT_FOUND",
                "The registration context is unavailable."),
            _ => Error(
                context, StatusCodes.Status503ServiceUnavailable,
                "REGISTRATION_UNAVAILABLE",
                "Registration is temporarily unavailable."),
        };

    private static bool TryApplicationUserId(
        HttpContext context,
        out Guid applicationUserId)
    {
        applicationUserId = Guid.Empty;
        return context.User.Identity?.IsAuthenticated == true &&
            Guid.TryParse(
                context.User.FindFirstValue(ClaimTypes.NameIdentifier),
                out applicationUserId) &&
            applicationUserId != Guid.Empty;
    }

    private static IResult RequestNotFound(HttpContext context) =>
        Error(context, StatusCodes.Status404NotFound,
            "REQUEST_NOT_FOUND", "The registration request was not found.");

    private static IResult Error(
        HttpContext context,
        int status,
        string code,
        string message,
        string? currentVersion = null) =>
        Results.Json(
            new ApiError(
                code,
                message,
                CorrelationId(context),
                currentVersion: currentVersion),
            statusCode: status);

    private static string CorrelationId(HttpContext context) =>
        string.IsNullOrWhiteSpace(context.TraceIdentifier)
            ? context.TraceIdentifier = Guid.NewGuid().ToString("N")
            : context.TraceIdentifier;

    // Stable middleware/application outcomes used by the contract:
    // ANTIFORGERY_INVALID, IDEMPOTENCY_KEY_REUSED, GROUP_FULL,
    // PLAN_CHANGED, POLICY_CHANGED. Student + Registration.SubmitOwn.
}
