using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;
using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Academics;

namespace StudentRegistration.Academics.Endpoints;

public static class Spec009Endpoints
{
    private const string CataloguePolicyManage = "CataloguePolicy.Manage";
    private const string AcademicProfileReadOwn = "AcademicProfile.ReadOwn";
    private const int DefaultPageSize = 20;
    private const int MaximumPageSize = 100;

    public static IEndpointRouteBuilder MapSpec009Endpoints(
        this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapGet("/api/admin/programs", GetPrograms)
            .RequireAuthorization(CataloguePolicyManage)
            .Produces<Page<ProgramAdminDto>>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable)
            .Produces<ApiError>(StatusCodes.Status500InternalServerError);

        endpoints.MapGet("/api/admin/catalogue/versions", GetVersions)
            .RequireAuthorization(CataloguePolicyManage)
            .Produces<Page<CatalogueVersionSummaryDto>>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable)
            .Produces<ApiError>(StatusCodes.Status500InternalServerError);

        endpoints.MapGet("/api/admin/catalogue/drafts/{draftId}", GetDraft)
            .RequireAuthorization(CataloguePolicyManage)
            .Produces<CatalogueDraftDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable)
            .Produces<ApiError>(StatusCodes.Status500InternalServerError);

        endpoints.MapPost("/api/admin/catalogue/drafts", CreateDraft)
            .RequireAuthorization(CataloguePolicyManage)
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .Produces<CatalogueDraftDto>(StatusCodes.Status201Created)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status409Conflict)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable);

        endpoints.MapGet("/api/students/me/roadmap", GetStudentRoadmap)
            .RequireAuthorization(AcademicProfileReadOwn)
            .Produces<StudentRoadmapDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable);

        endpoints.MapPut("/api/admin/catalogue/drafts/{draftId}", UpdateDraft)
            .RequireAuthorization(CataloguePolicyManage)
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .Produces<CatalogueDraftDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status409Conflict)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable)
            .Produces<ApiError>(StatusCodes.Status500InternalServerError);

        endpoints.MapPost("/api/admin/catalogue/imports", CreateImport)
            .RequireAuthorization(CataloguePolicyManage)
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .Produces<ImportBatchDto>(StatusCodes.Status201Created)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status409Conflict)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable)
            .Produces<ApiError>(StatusCodes.Status500InternalServerError);

        endpoints.MapGet("/api/admin/catalogue/imports/{importId}", GetImport)
            .RequireAuthorization(CataloguePolicyManage)
            .Produces<ImportBatchDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable)
            .Produces<ApiError>(StatusCodes.Status500InternalServerError);

        endpoints.MapPost(
                "/api/admin/catalogue/imports/{importId}/validate",
                ValidateImport)
            .RequireAuthorization(CataloguePolicyManage)
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .Produces<CatalogueValidationResult>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status409Conflict)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable)
            .Produces<ApiError>(StatusCodes.Status500InternalServerError);

        endpoints.MapPost(
                "/api/admin/catalogue/imports/{importId}/publish",
                PublishImport)
            .RequireAuthorization(CataloguePolicyManage)
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .Produces<CatalogueVersionSummaryDto>(StatusCodes.Status201Created)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status409Conflict)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable)
            .Produces<ApiError>(StatusCodes.Status500InternalServerError);

        endpoints.MapGet("/api/admin/policies", GetPolicies)
            .RequireAuthorization(CataloguePolicyManage)
            .Produces<Page<PolicySetAdminDto>>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable)
            .Produces<ApiError>(StatusCodes.Status500InternalServerError);

        endpoints.MapPost("/api/admin/policies", CreatePolicy)
            .RequireAuthorization(CataloguePolicyManage)
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .Produces<PolicySetAdminDto>(StatusCodes.Status201Created)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status409Conflict)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable)
            .Produces<ApiError>(StatusCodes.Status500InternalServerError);

        endpoints.MapPut("/api/admin/policies/{policySetId}", UpdatePolicy)
            .RequireAuthorization(CataloguePolicyManage)
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .Produces<PolicySetAdminDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status409Conflict)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable)
            .Produces<ApiError>(StatusCodes.Status500InternalServerError);

        endpoints.MapPost(
                "/api/admin/policies/{policySetId}/validate",
                ValidatePolicy)
            .RequireAuthorization(CataloguePolicyManage)
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .Produces<CatalogueValidationResult>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status409Conflict)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable)
            .Produces<ApiError>(StatusCodes.Status500InternalServerError);

        endpoints.MapPost(
                "/api/admin/policies/{policySetId}/simulate",
                SimulatePolicy)
            .RequireAuthorization(CataloguePolicyManage)
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .Produces<PolicySimulationResult>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status409Conflict)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable)
            .Produces<ApiError>(StatusCodes.Status500InternalServerError);

        endpoints.MapPost(
                "/api/admin/policies/{policySetId}/publish",
                PublishPolicy)
            .RequireAuthorization(CataloguePolicyManage)
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .Produces<PolicySetAdminDto>(StatusCodes.Status201Created)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status409Conflict)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable)
            .Produces<ApiError>(StatusCodes.Status500InternalServerError);

        return endpoints;
    }

    private static async Task<IResult> GetPrograms(
        [FromQuery] string? query,
        [FromQuery] bool? active,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? sort,
        HttpContext context,
        [FromServices] ICatalogueAdministrationStore store,
        CancellationToken cancellationToken)
    {
        var invalid = ValidateList(query, page, pageSize, context);
        if (invalid is not null)
        {
            return invalid;
        }

        return await ExecuteAsync(
            () => store.ListProgramsAsync(
                new(query?.Trim(), active, page ?? 1, pageSize ?? DefaultPageSize, sort),
                cancellationToken),
            context);
    }

    private static async Task<IResult> GetVersions(
        [FromQuery] string? scope,
        [FromQuery] string? state,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? sort,
        HttpContext context,
        [FromServices] ICatalogueAdministrationStore store,
        CancellationToken cancellationToken)
    {
        var invalid = ValidateList(scope, page, pageSize, context);
        if (invalid is not null)
        {
            return invalid;
        }

        return await ExecuteAsync(
            () => store.ListVersionsAsync(
                new(scope?.Trim(), state?.Trim(), page ?? 1, pageSize ?? DefaultPageSize, sort),
                cancellationToken),
            context);
    }

    private static async Task<IResult> GetDraft(
        [FromRoute] string draftId,
        HttpContext context,
        [FromServices] ICatalogueAdministrationStore store,
        CancellationToken cancellationToken)
    {
        var invalid = ValidateId(draftId, context);
        if (invalid is not null)
        {
            return invalid;
        }

        return await ExecuteNullableAsync(
            () => store.GetDraftAsync(Guid.Parse(draftId), cancellationToken),
            "DRAFT_NOT_FOUND",
            "The catalogue draft was not found.",
            context);
    }

    private static async Task<IResult> CreateDraft(
        CreateCatalogueDraftRequest request,
        HttpContext context,
        [FromServices] ICatalogueAdministrationStore store,
        CancellationToken cancellationToken)
    {
        if (request.BasedOnVersionId == Guid.Empty ||
            request.ClientRequestId == Guid.Empty ||
            string.IsNullOrWhiteSpace(request.Reason))
        {
            return Error(
                context,
                StatusCodes.Status400BadRequest,
                "VALIDATION_ERROR",
                "A base version, reason, and client request identifier are required.");
        }

        return await ExecuteAsync(
            async () => Results.Json(
                await store.CreateDraftAsync(request, cancellationToken),
                statusCode: StatusCodes.Status201Created),
            context,
            valueIsResult: true);
    }

    private static async Task<IResult> GetStudentRoadmap(
        HttpContext context,
        [FromServices] ICatalogueAdministrationStore store,
        CancellationToken cancellationToken)
    {
        if (context.User.Identity?.IsAuthenticated != true ||
            !Guid.TryParse(
                context.User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var applicationUserId) ||
            applicationUserId == Guid.Empty)
        {
            return Error(
                context,
                StatusCodes.Status401Unauthorized,
                "UNAUTHORIZED",
                "Authentication is required.");
        }

        return await ExecuteNullableAsync(
            () => store.GetStudentRoadmapAsync(applicationUserId, cancellationToken),
            "ROADMAP_NOT_FOUND",
            "The student roadmap is not available.",
            context);
    }

    private static async Task<IResult> UpdateDraft(
        [FromRoute] string draftId,
        CatalogueDraftMutationRequest request,
        HttpContext context,
        [FromServices] ICatalogueAdministrationStore store,
        CancellationToken cancellationToken)
    {
        var invalid = ValidateId(draftId, context)
            ?? ValidateRequired(request.ExpectedDraftRowVersion, context)
            ?? ValidateRequired(request.Reason, context)
            ?? ValidateRequired(request.Source, context)
            ?? ValidateOperations(request.Operations, context);
        if (invalid is not null)
        {
            return invalid;
        }

        return await ExecuteAsync(
            () => store.UpdateDraftAsync(Guid.Parse(draftId), request, cancellationToken),
            context);
    }

    private static async Task<IResult> CreateImport(
        CreateImportRequest request,
        HttpContext context,
        [FromServices] ICatalogueAdministrationStore store,
        CancellationToken cancellationToken)
    {
        var invalid = request.DraftId == Guid.Empty
            ? ValidationError(context)
            : ValidateRequired(request.Source, context)
              ?? ValidateRequired(request.ContentHash, context)
              ?? (request.ClientRequestId == Guid.Empty ? ValidationError(context) : null);
        if (invalid is not null)
        {
            return invalid;
        }

        return await ExecuteAsync(
            async () =>
            {
                var created = await store.CreateImportAsync(request, cancellationToken);
                return Results.Created($"/api/admin/catalogue/imports/{created.Id:D}", created);
            },
            context,
            valueIsResult: true);
    }

    private static async Task<IResult> GetImport(
        [FromRoute] string importId,
        HttpContext context,
        [FromServices] ICatalogueAdministrationStore store,
        CancellationToken cancellationToken)
    {
        var invalid = ValidateId(importId, context);
        if (invalid is not null)
        {
            return invalid;
        }

        return await ExecuteNullableAsync(
            () => store.GetImportAsync(Guid.Parse(importId), cancellationToken),
            "IMPORT_NOT_FOUND",
            "The catalogue import was not found.",
            context);
    }

    private static async Task<IResult> ValidateImport(
        [FromRoute] string importId,
        CatalogueValidationRequest request,
        HttpContext context,
        [FromServices] ICatalogueAdministrationStore store,
        CancellationToken cancellationToken)
    {
        var invalid = ValidateId(importId, context)
            ?? ValidateRequired(request.ExpectedImportRowVersion, context)
            ?? ValidateRequired(request.ExpectedDraftRowVersion, context);
        if (invalid is not null)
        {
            return invalid;
        }

        return await ExecuteAsync(
            () => store.ValidateImportAsync(
                new(
                    Guid.Parse(importId),
                    request.ExpectedImportRowVersion,
                    request.ExpectedDraftRowVersion,
                    Actor(context)),
                cancellationToken),
            context);
    }

    private static async Task<IResult> PublishImport(
        [FromRoute] string importId,
        PublishVersionRequest request,
        HttpContext context,
        [FromServices] ICatalogueAdministrationStore store,
        CancellationToken cancellationToken)
    {
        var invalid = ValidateId(importId, context)
            ?? ValidateRequired(request.ExpectedDraftRowVersion, context)
            ?? ValidateRequired(request.PreviewToken, context)
            ?? (request.ClientRequestId == Guid.Empty ? ValidationError(context) : null);
        if (invalid is not null)
        {
            return invalid;
        }

        return await ExecuteAsync(
            async () =>
            {
                var published = await store.PublishImportAsync(
                    new(
                        Guid.Parse(importId),
                        request.ExpectedDraftRowVersion,
                        request.PreviewToken,
                        request.ClientRequestId,
                        Actor(context)),
                    cancellationToken);
                return Results.Created(
                    $"/api/admin/catalogue/versions/{published.Id:D}",
                    published);
            },
            context,
            valueIsResult: true);
    }

    private static IResult GetPolicies(
        [FromQuery] string? scope,
        [FromQuery] string? state,
        [FromQuery] DateTime? effectiveAtUtc,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? sort,
        HttpContext context) =>
        ValidateList(scope, page, pageSize, context)
        ?? PolicyUnavailable(context);

    private static IResult CreatePolicy(
        CreatePolicySetRequest request,
        HttpContext context) =>
        ValidateRequired(request.Scope, context)
        ?? ValidateRequired(request.Version, context)
        ?? ValidateRequired(request.Reason, context)
        ?? ValidateOperations(request.Operations, context)
        ?? PolicyUnavailable(context);

    private static IResult UpdatePolicy(
        [FromRoute] string policySetId,
        PolicySetMutationRequest request,
        HttpContext context) =>
        ValidateId(policySetId, context)
        ?? ValidateRequired(request.ExpectedPolicySetRowVersion, context)
        ?? ValidateRequired(request.Reason, context)
        ?? ValidateOperations(request.Operations, context)
        ?? PolicyUnavailable(context);

    private static IResult ValidatePolicy(
        [FromRoute] string policySetId,
        PolicyValidationRequest request,
        HttpContext context) =>
        ValidateId(policySetId, context)
        ?? ValidateRequired(request.ExpectedPolicySetRowVersion, context)
        ?? PolicyUnavailable(context);

    private static IResult SimulatePolicy(
        [FromRoute] string policySetId,
        PolicySimulationRequest request,
        HttpContext context) =>
        ValidateId(policySetId, context)
        ?? (request.PolicySetId == Guid.Empty
            || !Guid.TryParse(policySetId, out var routeId)
            || routeId != request.PolicySetId
                ? ValidationError(context)
                : null)
        ?? ValidateRequired(request.StudentContextFixtureId, context)
        ?? PolicyUnavailable(context);

    private static IResult PublishPolicy(
        [FromRoute] string policySetId,
        PolicyPublishRequest request,
        HttpContext context) =>
        ValidateId(policySetId, context)
        ?? ValidateRequired(request.ExpectedPolicySetRowVersion, context)
        ?? ValidateRequired(request.PreviewToken, context)
        ?? ValidateRequired(request.ClientRequestId.ToString("D"), context)
        ?? PolicyUnavailable(context);

    private static async Task<IResult> ExecuteAsync<T>(
        Func<Task<T>> action,
        HttpContext context)
    {
        try
        {
            return Results.Ok(await action().ConfigureAwait(false));
        }
        catch (CatalogueStoreException exception)
        {
            return StoreError(exception, context);
        }
        catch (ArgumentException)
        {
            return ValidationError(context);
        }
        catch (InvalidOperationException)
        {
            return Error(
                context,
                StatusCodes.Status409Conflict,
                "CATALOGUE_STATE_INVALID",
                "The catalogue operation is not valid in its current state.");
        }
    }

    private static async Task<IResult> ExecuteAsync(
        Func<Task<IResult>> action,
        HttpContext context,
        bool valueIsResult)
    {
        _ = valueIsResult;
        try
        {
            return await action().ConfigureAwait(false);
        }
        catch (CatalogueStoreException exception)
        {
            return StoreError(exception, context);
        }
        catch (ArgumentException)
        {
            return ValidationError(context);
        }
        catch (InvalidOperationException)
        {
            return Error(
                context,
                StatusCodes.Status409Conflict,
                "CATALOGUE_STATE_INVALID",
                "The catalogue operation is not valid in its current state.");
        }
    }

    private static async Task<IResult> ExecuteNullableAsync<T>(
        Func<Task<T?>> action,
        string notFoundCode,
        string notFoundMessage,
        HttpContext context)
        where T : class
    {
        try
        {
            var value = await action().ConfigureAwait(false);
            return value is null
                ? Error(context, StatusCodes.Status404NotFound, notFoundCode, notFoundMessage)
                : Results.Ok(value);
        }
        catch (CatalogueStoreException exception)
        {
            return StoreError(exception, context);
        }
    }

    private static IResult StoreError(
        CatalogueStoreException exception,
        HttpContext context) =>
        Error(
            context,
            exception.Failure switch
            {
                CatalogueStoreFailure.Validation => StatusCodes.Status400BadRequest,
                CatalogueStoreFailure.NotFound => StatusCodes.Status404NotFound,
                CatalogueStoreFailure.Conflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError,
            },
            exception.Code,
            exception.Message);

    private static string Actor(HttpContext context) =>
        context.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? context.User.Identity?.Name
        ?? "authenticated-admin";

    private static IResult? ValidateList(
        string? filter,
        int? page,
        int? pageSize,
        HttpContext context)
    {
        var requestedPage = page ?? 1;
        var requestedSize = pageSize ?? DefaultPageSize;
        if (requestedPage < 1 || requestedSize is < 1 or > MaximumPageSize)
        {
            return Error(
                context,
                StatusCodes.Status400BadRequest,
                "PAGE_SIZE_INVALID",
                "Page and page-size values are outside the supported range.");
        }

        var trimmed = filter?.Trim();
        return trimmed is not null && trimmed.Length is < 3 or > 50
            ? ValidationError(context)
            : null;
    }

    private static IResult? ValidateId(string value, HttpContext context) =>
        Guid.TryParse(value, out var id) && id != Guid.Empty
            ? null
            : ValidationError(context);

    private static IResult? ValidateRequired(string? value, HttpContext context) =>
        string.IsNullOrWhiteSpace(value) ? ValidationError(context) : null;

    private static IResult? ValidateOperations<T>(
        IReadOnlyList<T>? operations,
        HttpContext context) =>
        operations is null || operations.Count == 0
            ? ValidationError(context)
            : null;

    private static IResult CatalogueUnavailable(HttpContext context) =>
        Error(
            context,
            StatusCodes.Status503ServiceUnavailable,
            "CATALOGUE_UNAVAILABLE",
            "Catalogue administration data is temporarily unavailable.");

    private static IResult PolicyUnavailable(HttpContext context) =>
        Error(
            context,
            StatusCodes.Status503ServiceUnavailable,
            "POLICY_UNAVAILABLE",
            "Policy administration data is temporarily unavailable.");

    private static IResult ValidationError(HttpContext context) =>
        Error(
            context,
            StatusCodes.Status400BadRequest,
            "VALIDATION_ERROR",
            "The request contains invalid or incomplete catalogue data.");

    private static IResult Error(
        HttpContext context,
        int statusCode,
        string code,
        string message)
    {
        if (string.IsNullOrWhiteSpace(context.TraceIdentifier))
        {
            context.TraceIdentifier = Guid.NewGuid().ToString("N");
        }

        return Results.Json(
            new ApiError(code, message, context.TraceIdentifier),
            statusCode: statusCode);
    }
}
