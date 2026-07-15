using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Academics;

namespace StudentRegistration.Academics.Endpoints;

public static class Spec008Endpoints
{
    private const string ContextRead = "Context.Read";
    private const string StudentRole = "Student";
    private const string AcademicProfileReadOwn = "AcademicProfile.ReadOwn";
    private const string AcademicTermsManage = "AcademicTerms.Manage";
    private const string AcademicProfilesManage = "AcademicProfiles.Manage";
    private const int DefaultPageSize = 20;

    public static IEndpointRouteBuilder MapSpec008Endpoints(
        this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapGet("/api/public/context", GetPublicContextAsync)
            .AllowAnonymous()
            .Produces<PublicContextDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable)
            .Produces<ApiError>(StatusCodes.Status500InternalServerError);

        endpoints.MapGet("/api/context", GetAuthenticatedContextAsync)
            .RequireAuthorization(ContextRead)
            .Produces<AppContextDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable)
            .Produces<ApiError>(StatusCodes.Status500InternalServerError);

        endpoints.MapGet(
                "/api/students/me/academic-context",
                GetOwnAcademicContextAsync)
            .RequireAuthorization(AcademicProfileReadOwn)
            .Produces<StudentAcademicContextDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status409Conflict)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable)
            .Produces<ApiError>(StatusCodes.Status500InternalServerError);

        endpoints.MapGet("/api/admin/terms", GetAdminTermsAsync)
            .RequireAuthorization(AcademicTermsManage)
            .Produces<Page<AdminTermDto>>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable)
            .Produces<ApiError>(StatusCodes.Status500InternalServerError);

        endpoints.MapPost("/api/admin/terms", CreateAdminTermAsync)
            .RequireAuthorization(AcademicTermsManage)
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .Produces<AdminTermDto>(StatusCodes.Status201Created)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status409Conflict)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable)
            .Produces<ApiError>(StatusCodes.Status500InternalServerError);

        endpoints.MapPut("/api/admin/terms/{termId}", UpdateAdminTermAsync)
            .RequireAuthorization(AcademicTermsManage)
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .Produces<AdminTermDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status409Conflict)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable)
            .Produces<ApiError>(StatusCodes.Status500InternalServerError);

        endpoints.MapPost(
                "/api/admin/terms/{termId}/registration-windows/{windowId}/publish",
                PublishAdminRegistrationWindowAsync)
            .RequireAuthorization(AcademicTermsManage)
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .Produces<AdminTermDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status409Conflict)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable)
            .Produces<ApiError>(StatusCodes.Status500InternalServerError);

        endpoints.MapGet("/api/admin/students", GetAdminStudentsAsync)
            .RequireAuthorization(AcademicProfilesManage)
            .Produces<Page<AdminStudentLocatorDto>>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable)
            .Produces<ApiError>(StatusCodes.Status500InternalServerError);

        endpoints.MapGet(
                "/api/admin/students/{studentId}/academic-context",
                GetAdminStudentAcademicContextAsync)
            .RequireAuthorization(AcademicProfilesManage)
            .Produces<AdminStudentAcademicContextDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status409Conflict)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable)
            .Produces<ApiError>(StatusCodes.Status500InternalServerError);

        endpoints.MapPatch(
                "/api/admin/students/{studentId}/academic-profile",
                CorrectAdminStudentAcademicProfileAsync)
            .RequireAuthorization(AcademicProfilesManage)
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .Produces<AdminStudentAcademicContextDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status409Conflict)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable)
            .Produces<ApiError>(StatusCodes.Status500InternalServerError);

        return endpoints;
    }

    private static async Task<IResult> GetPublicContextAsync(
        [FromServices] AcademicContextResolver resolver,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            return Results.Ok(await resolver.ResolvePublicAsync(cancellationToken));
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (AcademicContextUnavailableException)
        {
            return Error(
                context,
                StatusCodes.Status503ServiceUnavailable,
                AcademicContextUnavailableException.ErrorCode,
                "Academic context is temporarily unavailable.");
        }
        catch (Exception)
        {
            return InternalError(context);
        }
    }

    private static async Task<IResult> GetAuthenticatedContextAsync(
        [FromServices] AcademicContextResolver resolver,
        [FromServices] IAcademicStudentScopeReader studentScopeReader,
        [FromServices] IAcademicSessionContextAdapter sessionAdapter,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryGetApplicationUserId(context.User, out var applicationUserId))
        {
            return Results.Unauthorized();
        }

        try
        {
            var activeRole = context.User.FindFirstValue(ClaimTypes.Role);
            AcademicStudentScope? studentScope = null;
            if (string.Equals(activeRole, StudentRole, StringComparison.Ordinal))
            {
                studentScope = await studentScopeReader.FindByApplicationUserIdAsync(
                    applicationUserId,
                    cancellationToken);
                if (studentScope is null)
                {
                    return Error(
                        context,
                        StatusCodes.Status503ServiceUnavailable,
                        AcademicContextUnavailableException.ErrorCode,
                        "Academic context is temporarily unavailable.");
                }
            }

            var academicContext = await resolver.ResolveAsync(
                studentScope,
                cancellationToken);
            var appContext = await sessionAdapter.ComposeAsync(
                applicationUserId,
                activeRole,
                academicContext,
                cancellationToken);
            return appContext is null
                ? Results.Unauthorized()
                : Results.Ok(appContext);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (AcademicContextUnavailableException)
        {
            return Error(
                context,
                StatusCodes.Status503ServiceUnavailable,
                AcademicContextUnavailableException.ErrorCode,
                "Academic context is temporarily unavailable.");
        }
        catch (Exception)
        {
            return InternalError(context);
        }
    }

    private static async Task<IResult> GetOwnAcademicContextAsync(
        [FromQuery] int? transcriptPage,
        [FromQuery] int? transcriptPageSize,
        [FromQuery] int? provenancePage,
        [FromQuery] int? provenancePageSize,
        [FromServices] StudentAcademicProfileService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryGetApplicationUserId(context.User, out var applicationUserId))
        {
            return Results.Unauthorized();
        }

        try
        {
            var result = await service.ReadOwnAsync(
                applicationUserId,
                transcriptPage ?? 1,
                transcriptPageSize ?? DefaultPageSize,
                provenancePage ?? 1,
                provenancePageSize ?? DefaultPageSize,
                cancellationToken);
            return ProfileResult(result, context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return InternalError(context);
        }
    }

    private static async Task<IResult> GetAdminTermsAsync(
        [FromQuery] string? query,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] TermState? state,
        [FromQuery] string? sort,
        [FromServices] AdminAcademicManagementService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.ListTermsAsync(
                query,
                page ?? 1,
                pageSize ?? DefaultPageSize,
                state,
                sort,
                cancellationToken);
            return QueryResult(result, context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return InternalError(context);
        }
    }

    private static async Task<IResult> CreateAdminTermAsync(
        CreateTermRequest request,
        [FromServices] AdminAcademicManagementService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryCreateCommandContext(context, out var commandContext))
        {
            return Results.Unauthorized();
        }

        try
        {
            var result = await service.CreateTermAsync(
                request,
                commandContext,
                cancellationToken);
            return TermCommandResult(
                result,
                context,
                StatusCodes.Status201Created,
                allowNotFound: false);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (ArgumentException)
        {
            return ValidationError(context);
        }
        catch (Exception)
        {
            return InternalError(context);
        }
    }

    private static async Task<IResult> UpdateAdminTermAsync(
        [FromRoute] string termId,
        UpdateTermRequest request,
        [FromServices] AdminAcademicManagementService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryRequiredId(termId, out var parsedTermId))
        {
            return ValidationError(context);
        }

        if (!TryCreateCommandContext(context, out var commandContext))
        {
            return Results.Unauthorized();
        }

        try
        {
            var result = await service.UpdateTermAsync(
                parsedTermId,
                request,
                commandContext,
                cancellationToken);
            return TermCommandResult(
                result,
                context,
                StatusCodes.Status200OK,
                allowNotFound: true);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (ArgumentException)
        {
            return ValidationError(context);
        }
        catch (Exception)
        {
            return InternalError(context);
        }
    }

    private static async Task<IResult> PublishAdminRegistrationWindowAsync(
        [FromRoute] string termId,
        [FromRoute] string windowId,
        PublishRegistrationWindowRequest request,
        [FromServices] AdminAcademicManagementService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryRequiredId(termId, out var parsedTermId) ||
            !TryRequiredId(windowId, out var parsedWindowId))
        {
            return ValidationError(context);
        }

        if (!TryCreateCommandContext(context, out var commandContext))
        {
            return Results.Unauthorized();
        }

        try
        {
            var result = await service.PublishTermRegistrationWindowAsync(
                parsedTermId,
                parsedWindowId,
                request,
                commandContext,
                cancellationToken);
            return TermCommandResult(
                result,
                context,
                StatusCodes.Status200OK,
                allowNotFound: true);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (ArgumentException)
        {
            return ValidationError(context);
        }
        catch (Exception)
        {
            return InternalError(context);
        }
    }

    private static async Task<IResult> GetAdminStudentsAsync(
        [FromQuery] string? termId,
        [FromQuery] string? query,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? sort,
        [FromServices] AdminAcademicManagementService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryRequiredId(termId, out var parsedTermId))
        {
            return ValidationError(context);
        }

        try
        {
            var result = await service.ListStudentsAsync(
                parsedTermId,
                query,
                page ?? 1,
                pageSize ?? DefaultPageSize,
                sort,
                cancellationToken);
            return QueryResult(result, context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return InternalError(context);
        }
    }

    private static async Task<IResult> GetAdminStudentAcademicContextAsync(
        [FromRoute] string studentId,
        [FromQuery] string? termId,
        [FromQuery] int? transcriptPage,
        [FromQuery] int? transcriptPageSize,
        [FromQuery] int? provenancePage,
        [FromQuery] int? provenancePageSize,
        [FromServices] StudentAcademicProfileService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryRequiredId(studentId, out var parsedStudentId) ||
            !TryRequiredId(termId, out var parsedTermId))
        {
            return Error(
                context,
                StatusCodes.Status400BadRequest,
                "VALIDATION_ERROR",
                "A valid student and academic term are required.");
        }

        try
        {
            var result = await service.ReadAdminAsync(
                parsedStudentId,
                parsedTermId,
                transcriptPage ?? 1,
                transcriptPageSize ?? DefaultPageSize,
                provenancePage ?? 1,
                provenancePageSize ?? DefaultPageSize,
                cancellationToken);
            return ProfileResult(result, context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return InternalError(context);
        }
    }

    private static async Task<IResult> CorrectAdminStudentAcademicProfileAsync(
        [FromRoute] string studentId,
        AcademicProfileCorrectionRequest request,
        [FromServices] AdminAcademicManagementService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryRequiredId(studentId, out var parsedStudentId))
        {
            return ValidationError(context);
        }

        if (!TryCreateCommandContext(context, out var commandContext))
        {
            return Results.Unauthorized();
        }

        try
        {
            var result = await service.CorrectStudentProfileAsync(
                parsedStudentId,
                request,
                commandContext,
                cancellationToken);
            return ProfileResult(result, context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (ArgumentException)
        {
            return ValidationError(context);
        }
        catch (Exception)
        {
            return InternalError(context);
        }
    }

    private static IResult QueryResult<T>(
        AdminAcademicQueryResult<T> result,
        HttpContext context)
        where T : class =>
        result.Outcome switch
        {
            AdminAcademicQueryOutcome.Succeeded when result.Page is not null =>
                Results.Ok(result.Page),
            AdminAcademicQueryOutcome.PageSizeInvalid => Error(
                context,
                StatusCodes.Status400BadRequest,
                "PAGE_SIZE_INVALID",
                "Page and page-size values are outside the supported range."),
            AdminAcademicQueryOutcome.ValidationError => ValidationError(context),
            AdminAcademicQueryOutcome.StorageUnavailable => Error(
                context,
                StatusCodes.Status503ServiceUnavailable,
                "CONTEXT_UNAVAILABLE",
                "Academic administration data is temporarily unavailable."),
            _ => InternalError(context)
        };

    private static IResult TermCommandResult(
        AcademicTermCommandResult result,
        HttpContext context,
        int successStatusCode,
        bool allowNotFound) =>
        result.Outcome switch
        {
            AcademicTermCommandOutcome.Succeeded or AcademicTermCommandOutcome.Replayed
                when result.Term is not null =>
                Results.Json(result.Term, statusCode: successStatusCode),
            AcademicTermCommandOutcome.NotFound when allowNotFound => Error(
                context,
                StatusCodes.Status404NotFound,
                "NOT_FOUND",
                "The academic term or registration window was not found."),
            AcademicTermCommandOutcome.StaleVersion => Error(
                context,
                StatusCodes.Status409Conflict,
                "STALE_VERSION",
                "The academic record changed and must be refreshed.",
                result.CurrentVersion),
            AcademicTermCommandOutcome.WindowOverlap => Error(
                context,
                StatusCodes.Status409Conflict,
                "WINDOW_OVERLAP",
                "The registration window overlaps another published window."),
            AcademicTermCommandOutcome.TermCodeExists => Error(
                context,
                StatusCodes.Status409Conflict,
                "TERM_CODE_EXISTS",
                "An academic term already uses this code."),
            AcademicTermCommandOutcome.TermStateConflict => Error(
                context,
                StatusCodes.Status409Conflict,
                "TERM_STATE_CONFLICT",
                "The requested academic-term state transition is not allowed."),
            AcademicTermCommandOutcome.IdempotencyKeyReused => Error(
                context,
                StatusCodes.Status409Conflict,
                "IDEMPOTENCY_KEY_REUSED",
                "The client request ID was already used for a different payload."),
            AcademicTermCommandOutcome.StorageUnavailable => Error(
                context,
                StatusCodes.Status503ServiceUnavailable,
                "CONTEXT_UNAVAILABLE",
                "Academic administration data is temporarily unavailable."),
            _ => InternalError(context)
        };

    private static IResult ProfileResult<TProfile>(
        AcademicProfileResult<TProfile> result,
        HttpContext context)
        where TProfile : class =>
        result.Outcome switch
        {
            AcademicProfileOutcome.Succeeded when result.Profile is not null =>
                Results.Ok(result.Profile),
            AcademicProfileOutcome.PageSizeInvalid => Error(
                context,
                StatusCodes.Status400BadRequest,
                "PAGE_SIZE_INVALID",
                "Page and page-size values are outside the supported range."),
            AcademicProfileOutcome.ValidationError => ValidationError(context),
            AcademicProfileOutcome.NotFound => Error(
                context,
                StatusCodes.Status404NotFound,
                "PROFILE_NOT_FOUND",
                "The academic profile was not found."),
            AcademicProfileOutcome.ProfileNotReady => Error(
                context,
                StatusCodes.Status409Conflict,
                "PROFILE_NOT_READY",
                "The complete academic profile is not ready."),
            AcademicProfileOutcome.StaleVersion => Error(
                context,
                StatusCodes.Status409Conflict,
                "STALE_VERSION",
                "The academic profile changed and must be refreshed.",
                result.CurrentVersion),
            AcademicProfileOutcome.InvalidSupersession => Error(
                context,
                StatusCodes.Status409Conflict,
                "INVALID_SUPERSESSION",
                "The transcript correction history is no longer current."),
            AcademicProfileOutcome.StorageUnavailable => Error(
                context,
                StatusCodes.Status503ServiceUnavailable,
                "CONTEXT_UNAVAILABLE",
                "Academic profile data is temporarily unavailable."),
            _ => InternalError(context)
        };

    private static IResult InternalError(HttpContext context) =>
        Error(
            context,
            StatusCodes.Status500InternalServerError,
            "INTERNAL_ERROR",
            "An unexpected error occurred. Use the reference when contacting support.");

    private static IResult ValidationError(HttpContext context) =>
        Error(
            context,
            StatusCodes.Status400BadRequest,
            "VALIDATION_ERROR",
            "The request contains invalid or incomplete academic data.");

    private static IResult Error(
        HttpContext context,
        int statusCode,
        string code,
        string message,
        string? currentVersion = null)
    {
        if (string.IsNullOrWhiteSpace(context.TraceIdentifier))
        {
            context.TraceIdentifier = Guid.NewGuid().ToString("N");
        }

        return Results.Json(
            new ApiError(
                code,
                message,
                context.TraceIdentifier,
                currentVersion: currentVersion),
            statusCode: statusCode);
    }

    private static bool TryRequiredId(string? value, out Guid id) =>
        Guid.TryParse(value, out id) && id != Guid.Empty;

    private static bool TryCreateCommandContext(
        HttpContext context,
        out AcademicCommandContext commandContext)
    {
        if (!TryGetApplicationUserId(context.User, out var applicationUserId))
        {
            commandContext = null!;
            return false;
        }

        if (string.IsNullOrWhiteSpace(context.TraceIdentifier))
        {
            context.TraceIdentifier = Guid.NewGuid().ToString("N");
        }

        commandContext = new AcademicCommandContext(
            applicationUserId.ToString("D"),
            context.TraceIdentifier);
        return true;
    }

    private static bool TryGetApplicationUserId(
        ClaimsPrincipal principal,
        out Guid applicationUserId) =>
        Guid.TryParse(
            principal.FindFirstValue(ClaimTypes.NameIdentifier),
            out applicationUserId) &&
        applicationUserId != Guid.Empty;
}
