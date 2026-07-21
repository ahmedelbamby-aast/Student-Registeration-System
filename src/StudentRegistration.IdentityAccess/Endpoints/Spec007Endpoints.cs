using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Identity;
using StudentRegistration.IdentityAccess.Application;
using StudentRegistration.IdentityAccess.Application.Authorization;

namespace StudentRegistration.IdentityAccess.Endpoints;

public static class Spec007Endpoints
{
    public static IEndpointRouteBuilder MapSpec007Endpoints(
        this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapPost("/api/auth/student/login", LoginStudentAsync)
            .AllowAnonymous()
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .RequireRateLimiting(IdentityRateLimitPolicies.StudentLogin)
            .Produces<SessionDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status401Unauthorized);

        endpoints.MapPost("/api/auth/student/activate", ActivateStudentAsync)
            .AllowAnonymous()
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .RequireRateLimiting(IdentityRateLimitPolicies.StudentActivation)
            .Produces<SessionDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest);

        endpoints.MapPost("/api/auth/staff/login", LoginStaffAsync)
            .AllowAnonymous()
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .RequireRateLimiting(IdentityRateLimitPolicies.StaffLogin)
            .Produces<SessionDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status401Unauthorized);

        endpoints.MapPost("/api/auth/logout", LogoutAsync)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .Produces(StatusCodes.Status204NoContent);

        endpoints.MapPost("/api/auth/recovery/request", RequestRecoveryAsync)
            .AllowAnonymous()
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .RequireRateLimiting(IdentityRateLimitPolicies.RecoveryRequest)
            .Produces(StatusCodes.Status202Accepted);

        endpoints.MapPost("/api/auth/recovery/complete", CompleteRecoveryAsync)
            .AllowAnonymous()
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .RequireRateLimiting(IdentityRateLimitPolicies.RecoveryCompletion)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiError>(StatusCodes.Status400BadRequest);

        endpoints.MapPost("/api/auth/password/change", ChangePasswordAsync)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .RequireRateLimiting(IdentityRateLimitPolicies.PasswordChange)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiError>(StatusCodes.Status400BadRequest);

        endpoints.MapPost("/api/auth/sessions/revoke-all", RevokeAllAsync)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .Produces(StatusCodes.Status204NoContent);

        endpoints.MapGet("/api/auth/session", GetSessionAsync)
            .RequireAuthorization()
            .Produces<SessionDto>(StatusCodes.Status200OK);

        endpoints.MapGet("/api/admin/users", ListUsersAsync)
            .RequireAuthorization(RolePolicies.IdentityManagement)
            .Produces<Page<IdentityUserSummaryDto>>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest);

        endpoints.MapPost("/api/admin/users/imports", CreateImportAsync)
            .RequireAuthorization(RolePolicies.IdentityManagement)
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .Produces<IdentityImportBatchDto>(StatusCodes.Status202Accepted)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status409Conflict);

        endpoints.MapGet("/api/admin/users/imports/{importId}", GetImportAsync)
            .RequireAuthorization(RolePolicies.IdentityManagement)
            .Produces<IdentityImportBatchDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status404NotFound);

        endpoints.MapPost("/api/admin/users/imports/{importId}/publish", PublishImportAsync)
            .RequireAuthorization(RolePolicies.IdentityManagement)
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .Produces<IdentityImportBatchDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status409Conflict);

        endpoints.MapPatch("/api/admin/users/{userId}/status", ChangeUserStatusAsync)
            .RequireAuthorization(RolePolicies.IdentityManagement)
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .Produces<IdentityUserSummaryDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status409Conflict);

        endpoints.MapPut("/api/admin/users/{userId}/roles", ReplaceUserRolesAsync)
            .RequireAuthorization(RolePolicies.IdentityManagement)
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .Produces<IdentityUserSummaryDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status409Conflict);

        return endpoints;
    }

    private static async Task<IResult> LoginStudentAsync(
        StudentLoginRequest request,
        [FromServices] StudentAuthenticationService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var result = await service.AuthenticateAsync(
            request.UniversityId,
            request.Password,
            NetworkScope(context),
            cancellationToken);
        return await SignInOrFailAsync(result, context);
    }

    private static async Task<IResult> ActivateStudentAsync(
        ActivateStudentRequest request,
        [FromServices] StudentActivationService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var result = await service.ActivateAsync(
            request.UniversityId,
            request.InitialPassword,
            request.NewPassword,
            NetworkScope(context),
            cancellationToken);
        if (result.Outcome == AuthenticationOutcome.PasswordRejected)
        {
            return Error(context, StatusCodes.Status400BadRequest, "PASSWORD_REJECTED");
        }

        return await SignInOrFailAsync(
            result,
            context,
            "ACTIVATION_FAILED",
            StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> LoginStaffAsync(
        StaffLoginRequest request,
        [FromServices] StaffAuthenticationService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var result = await service.AuthenticateAsync(
            request.UserName,
            request.Password,
            NetworkScope(context),
            cancellationToken);
        return await SignInOrFailAsync(result, context);
    }

    private static async Task<IResult> LogoutAsync(
        HttpContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await context.SignOutAsync(IdentityAuthenticationDefaults.AuthenticationScheme);
        return Results.NoContent();
    }

    private static async Task<IResult> RequestRecoveryAsync(
        RecoveryRequest request,
        [FromServices] SessionLifecycleService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        await service.RequestRecoveryAsync(
            request.UniversityIdOrUserName,
            NetworkScope(context),
            cancellationToken);
        return Results.Accepted();
    }

    private static async Task<IResult> CompleteRecoveryAsync(
        RecoveryCompleteRequest request,
        [FromServices] SessionLifecycleService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var result = await service.CompleteRecoveryAsync(
            request.ChallengeToken,
            request.NewPassword,
            NetworkScope(context),
            cancellationToken);
        return result.Outcome switch
        {
            SessionLifecycleOutcome.Completed => Results.NoContent(),
            SessionLifecycleOutcome.PasswordRejected =>
                Error(context, StatusCodes.Status400BadRequest, "PASSWORD_REJECTED"),
            _ => Error(context, StatusCodes.Status400BadRequest, "CHALLENGE_INVALID")
        };
    }

    private static async Task<IResult> ChangePasswordAsync(
        ChangePasswordRequest request,
        [FromServices] SessionLifecycleService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(context.User, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await service.ChangePasswordAsync(
            userId,
            request.CurrentPassword,
            request.NewPassword,
            NetworkScope(context),
            cancellationToken);
        return result.Outcome switch
        {
            SessionLifecycleOutcome.Completed => Results.NoContent(),
            SessionLifecycleOutcome.PasswordRejected =>
                Error(context, StatusCodes.Status400BadRequest, "PASSWORD_REJECTED"),
            _ => Error(context, StatusCodes.Status400BadRequest, "CURRENT_PASSWORD_INVALID")
        };
    }

    private static async Task<IResult> RevokeAllAsync(
        [FromServices] SessionLifecycleService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(context.User, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await service.RevokeAllSessionsAsync(userId, cancellationToken);
        if (result.Outcome != SessionLifecycleOutcome.Completed)
        {
            return Results.Unauthorized();
        }

        await context.SignOutAsync(IdentityAuthenticationDefaults.AuthenticationScheme);
        return Results.NoContent();
    }

    private static async Task<IResult> GetSessionAsync(
        [FromServices] SessionLifecycleService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(context.User, out var userId))
        {
            return Results.Unauthorized();
        }

        var activeRole = context.User.FindFirstValue(
            IdentityAuthenticationDefaults.ActiveRoleClaim);
        var session = await service.GetSessionAsync(
            userId,
            activeRole,
            cancellationToken);
        return session.Succeeded ? Results.Ok(ToDto(session)) : Results.Unauthorized();
    }

    private static async Task<IResult> ListUsersAsync(
        [FromQuery] string? search,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? sort,
        [FromServices] AdminUserLifecycleService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var result = await service.ListUsersAsync(
            search,
            page ?? 1,
            pageSize ?? 20,
            sort,
            cancellationToken);
        return AdminResult(result, context);
    }

    private static async Task<IResult> CreateImportAsync(
        IdentityImportRequest request,
        [FromServices] AdminUserLifecycleService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(context.User, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await service.CreateImportAsync(
            userId,
            request,
            context.TraceIdentifier,
            cancellationToken);
        return AdminResult(result, context, StatusCodes.Status202Accepted);
    }

    private static async Task<IResult> GetImportAsync(
        Guid importId,
        [FromServices] AdminUserLifecycleService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(context.User, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await service.GetImportAsync(userId, importId, cancellationToken);
        return AdminResult(result, context);
    }

    private static async Task<IResult> PublishImportAsync(
        Guid importId,
        IdentityImportPublishRequest request,
        [FromServices] AdminUserLifecycleService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(context.User, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await service.PublishImportAsync(
            userId,
            importId,
            request,
            context.TraceIdentifier,
            cancellationToken);
        return AdminResult(result, context);
    }

    private static async Task<IResult> ChangeUserStatusAsync(
        Guid userId,
        UserStatusRequest request,
        [FromServices] AdminUserLifecycleService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(context.User, out var actorUserId))
        {
            return Results.Unauthorized();
        }

        var result = await service.ChangeStatusAsync(
            actorUserId,
            userId,
            request,
            context.TraceIdentifier,
            cancellationToken);
        return AdminResult(result, context);
    }

    private static async Task<IResult> ReplaceUserRolesAsync(
        Guid userId,
        UserRolesRequest request,
        [FromServices] AdminUserLifecycleService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(context.User, out var actorUserId))
        {
            return Results.Unauthorized();
        }

        var result = await service.ReplaceRolesAsync(
            actorUserId,
            userId,
            request,
            context.TraceIdentifier,
            cancellationToken);
        return AdminResult(result, context);
    }

    private static async Task<IResult> SignInOrFailAsync(
        AuthenticationResult result,
        HttpContext context,
        string errorCode = "AUTHENTICATION_FAILED",
        int errorStatus = StatusCodes.Status401Unauthorized)
    {
        if (!result.Succeeded
            || result.UserId is null
            || result.ExpiresAtUtc is null
            || string.IsNullOrWhiteSpace(result.DisplayName)
            || string.IsNullOrWhiteSpace(result.SecurityStamp))
        {
            return Error(context, errorStatus, errorCode);
        }

        if (result.ActiveRole is not null
            && !result.AuthorizedRoles.Contains(result.ActiveRole, StringComparer.Ordinal))
        {
            return Error(context, errorStatus, errorCode);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.UserId.Value.ToString()),
            new(ClaimTypes.Name, result.DisplayName),
            new(IdentityAuthenticationDefaults.SecurityStampClaim, result.SecurityStamp)
        };
        claims.AddRange(result.AuthorizedRoles.Select(role =>
            new Claim(RolePolicies.AvailableRoleClaimType, role)));
        if (result.ActiveRole is not null)
        {
            claims.Add(new Claim(ClaimTypes.Role, result.ActiveRole));
            foreach (var permission in RolePolicies.PermissionsForRole(result.ActiveRole))
            {
                claims.Add(new Claim(
                    RolePolicies.PermissionClaimType,
                    permission));
            }

            claims.Add(new Claim(IdentityAuthenticationDefaults.ActiveRoleClaim, result.ActiveRole));
        }

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
                claims,
                IdentityAuthenticationDefaults.AuthenticationScheme));
        await context.SignInAsync(
            IdentityAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                AllowRefresh = false,
                IsPersistent = false,
                ExpiresUtc = new DateTimeOffset(result.ExpiresAtUtc.Value, TimeSpan.Zero)
            });

        return Results.Ok(ToDto(result));
    }

    private static SessionDto ToDto(AuthenticationResult result) =>
        new(
            result.DisplayName ?? string.Empty,
            result.AuthorizedRoles,
            result.ActiveRole,
            "active",
            result.ExpiresAtUtc ?? DateTime.UnixEpoch);

    private static IResult AdminResult<T>(
        AdminUserLifecycleResult<T> result,
        HttpContext context,
        int successStatus = StatusCodes.Status200OK)
        where T : class =>
        result.Outcome switch
        {
            AdminUserLifecycleOutcome.Succeeded when result.Value is not null =>
                Results.Json(result.Value, statusCode: successStatus),
            AdminUserLifecycleOutcome.PageSizeInvalid =>
                Error(context, StatusCodes.Status400BadRequest, "PAGE_SIZE_INVALID"),
            AdminUserLifecycleOutcome.ImportInvalid =>
                Error(context, StatusCodes.Status400BadRequest, "IMPORT_INVALID"),
            AdminUserLifecycleOutcome.ImportNotValidated =>
                Error(context, StatusCodes.Status400BadRequest, "IMPORT_NOT_VALIDATED"),
            AdminUserLifecycleOutcome.ValidationFailed =>
                Error(context, StatusCodes.Status400BadRequest, "VALIDATION_FAILED"),
            AdminUserLifecycleOutcome.NotFound =>
                Error(context, StatusCodes.Status404NotFound, "NOT_FOUND"),
            AdminUserLifecycleOutcome.StaleVersion =>
                Error(
                    context,
                    StatusCodes.Status409Conflict,
                    "STALE_VERSION",
                    result.CurrentVersion),
            AdminUserLifecycleOutcome.FinalAdminRequired =>
                Error(context, StatusCodes.Status409Conflict, "FINAL_ADMIN_REQUIRED"),
            AdminUserLifecycleOutcome.IdempotencyKeyReused =>
                Error(context, StatusCodes.Status409Conflict, "IDEMPOTENCY_KEY_REUSED"),
            AdminUserLifecycleOutcome.ImportContentExists =>
                Error(context, StatusCodes.Status409Conflict, "IMPORT_CONTENT_EXISTS"),
            _ => Error(
                context,
                StatusCodes.Status500InternalServerError,
                "ADMIN_OPERATION_FAILED")
        };

    private static IResult Error(
        HttpContext context,
        int statusCode,
        string code,
        string? currentVersion = null) =>
        Results.Json(
            new ApiError(
                code,
                "The request could not be completed.",
                context.TraceIdentifier,
                currentVersion: currentVersion),
            statusCode: statusCode);

    private static bool TryGetUserId(ClaimsPrincipal principal, out Guid userId) =>
        Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out userId);

    private static string? NetworkScope(HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString();
}
