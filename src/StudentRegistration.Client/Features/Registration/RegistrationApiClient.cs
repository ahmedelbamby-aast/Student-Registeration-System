using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Registration;

namespace StudentRegistration.Client.Features.Registration;

/// <summary>
/// Client for registration discovery and the server-authoritative current plan.
/// Eligibility, capacity, conflict detection, and validation remain on the server.
/// </summary>
public sealed class RegistrationApiClient
{
    private const string AntiforgeryHeader = "X-XSRF-TOKEN";
    private const string AntiforgeryInterop =
        "StudentRegistration.antiforgery.getRequestToken";

    private static readonly JsonSerializerOptions ResponseJsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            RespectNullableAnnotations = true,
            RespectRequiredConstructorParameters = true
        };

    private readonly HttpClient _httpClient;
    private readonly IJSRuntime? _javascript;

    public RegistrationApiClient(HttpClient httpClient)
        : this(httpClient, null)
    {
    }

    public RegistrationApiClient(HttpClient httpClient, IJSRuntime? javascript)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _javascript = javascript;
    }

    public Task<RegistrationApiResult<Page<OfferingEligibilityDto>>> ListOfferingsAsync(
        Guid termId,
        OfferingDiscoveryQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        return GetAsync<Page<OfferingEligibilityDto>>(
            Query(
                $"/api/student/terms/{RequiredId(termId, nameof(termId)):D}/offerings",
                ("q", query.Search),
                ("eligibility", query.Eligibility),
                ("credits", query.Credits),
                ("day", query.Day),
                ("availability", query.Availability),
                ("sort", query.Sort),
                ("page", query.Page.ToString()),
                ("pageSize", query.PageSize.ToString())),
            cancellationToken);
    }

    public Task<RegistrationApiResult<OfferingEligibilityDto>> GetEligibilityAsync(
        Guid offeringId,
        CancellationToken cancellationToken = default) =>
        GetAsync<OfferingEligibilityDto>(
            $"/api/student/offerings/{RequiredId(offeringId, nameof(offeringId)):D}/eligibility",
            cancellationToken);

    public async Task<RegistrationPlanApiResult> GetRegistrationPlanAsync(
        Guid termId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            PlanPath(termId),
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        return await ReadPlanAsync(response, cancellationToken);
    }

    public Task<RegistrationPlanApiResult> ReplaceRegistrationPlanAsync(
        Guid termId,
        RegistrationPlanMutationRequest request,
        CancellationToken cancellationToken = default) =>
        SendPlanAsync(
            HttpMethod.Put,
            PlanPath(termId),
            request,
            cancellationToken);

    public Task<RegistrationPlanApiResult> ValidateRegistrationPlanAsync(
        Guid termId,
        CancellationToken cancellationToken = default) =>
        SendPlanAsync<object?>(
            HttpMethod.Post,
            $"{PlanPath(termId)}/validate",
            null,
            cancellationToken);

    public Task<RegistrationApiResult<OptimizationResultDto>>
        RecommendScheduleAsync(
            Guid termId,
            RecommendScheduleRequest request,
            CancellationToken cancellationToken = default) =>
        SendAsync<RecommendScheduleRequest, OptimizationResultDto>(
            HttpMethod.Post,
            $"{PlanPath(termId)}/recommendations",
            request,
            cancellationToken);

    public Task<RegistrationPlanApiResult> ApplyRecommendedOptionAsync(
        Guid termId,
        ApplyScheduleOptionRequest request,
        CancellationToken cancellationToken = default) =>
        SendPlanAsync(
            HttpMethod.Put,
            $"{PlanPath(termId)}/recommended-option",
            request,
            cancellationToken);

    private async Task<RegistrationApiResult<TResponse>> SendAsync<TRequest, TResponse>(
        HttpMethod method,
        string path,
        TRequest request,
        CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(method, path)
        {
            Content = JsonContent.Create(request)
        };
        await AddAntiforgeryAsync(message, cancellationToken);
        using var response = await _httpClient.SendAsync(
            message,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            try
            {
                var value = await response.Content.ReadFromJsonAsync<TResponse>(
                    ResponseJsonOptions,
                    cancellationToken);
                return value is null
                    ? RegistrationApiResult<TResponse>.Failure(
                        null,
                        response.StatusCode)
                    : RegistrationApiResult<TResponse>.Success(
                        value,
                        response.StatusCode,
                        ReadMetadata(response));
            }
            catch (Exception exception) when (
                exception is HttpRequestException or NotSupportedException or
                    JsonException or ArgumentException)
            {
                return RegistrationApiResult<TResponse>.Failure(
                    null,
                    response.StatusCode);
            }
        }

        return RegistrationApiResult<TResponse>.Failure(
            await ReadErrorAsync(response, cancellationToken),
            response.StatusCode);
    }

    private async Task<RegistrationPlanApiResult> SendPlanAsync<TRequest>(
        HttpMethod method,
        string path,
        TRequest request,
        CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(method, path);
        if (request is not null)
        {
            message.Content = JsonContent.Create(request);
        }

        await AddAntiforgeryAsync(message, cancellationToken);

        using var response = await _httpClient.SendAsync(
            message,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        return await ReadPlanAsync(response, cancellationToken);
    }

    private async Task AddAntiforgeryAsync(
        HttpRequestMessage message,
        CancellationToken cancellationToken)
    {
        if (_javascript is null)
        {
            return;
        }

        var token = await _javascript.InvokeAsync<string>(
            AntiforgeryInterop,
            cancellationToken);
        if (!string.IsNullOrWhiteSpace(token))
        {
            message.Headers.TryAddWithoutValidation(AntiforgeryHeader, token);
        }
    }

    private static async Task<RegistrationPlanApiResult> ReadPlanAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            try
            {
                var plan = await response.Content.ReadFromJsonAsync<RegistrationPlanDto>(
                    ResponseJsonOptions,
                    cancellationToken);
                return plan is null
                    ? RegistrationPlanApiResult.Failure(null, response.StatusCode)
                    : RegistrationPlanApiResult.Success(plan, response.StatusCode);
            }
            catch (Exception exception) when (
                exception is HttpRequestException
                    or NotSupportedException
                    or JsonException
                    or ArgumentException)
            {
                return RegistrationPlanApiResult.Failure(null, response.StatusCode);
            }
        }

        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            try
            {
                var stale = await response.Content
                    .ReadFromJsonAsync<StaleRegistrationPlanResponse>(
                        ResponseJsonOptions,
                        cancellationToken);
                if (stale is not null)
                {
                    return RegistrationPlanApiResult.Stale(
                        stale.Error,
                        stale.CurrentPlan,
                        response.StatusCode);
                }
            }
            catch (Exception exception) when (
                exception is HttpRequestException
                    or NotSupportedException
                    or JsonException
                    or ArgumentException)
            {
                // Fall through to the canonical safe error reader.
            }
        }

        return RegistrationPlanApiResult.Failure(
            await ReadErrorAsync(response, cancellationToken),
            response.StatusCode);
    }

    private async Task<RegistrationApiResult<T>> GetAsync<T>(
        string path,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(
            path,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            try
            {
                var value = await response.Content.ReadFromJsonAsync<T>(
                    ResponseJsonOptions,
                    cancellationToken);
                return value is null
                    ? RegistrationApiResult<T>.Failure(null, response.StatusCode)
                    : RegistrationApiResult<T>.Success(
                        value,
                        response.StatusCode,
                        ReadMetadata(response));
            }
            catch (Exception exception) when (
                exception is HttpRequestException
                    or NotSupportedException
                    or JsonException
                    or ArgumentException)
            {
                return RegistrationApiResult<T>.Failure(null, response.StatusCode);
            }
        }

        return RegistrationApiResult<T>.Failure(
            await ReadErrorAsync(response, cancellationToken),
            response.StatusCode);
    }

    private static RegistrationResponseMetadata ReadMetadata(HttpResponseMessage response) =>
        new(
            Header(response, "X-Eligibility-Policy-Version"),
            Header(response, "X-Eligibility-Reason-Codes"),
            Header(response, "X-Registration-Window-State"),
            Header(response, "X-Support-Reference-Path"));

    private static string? Header(HttpResponseMessage response, string name) =>
        response.Headers.TryGetValues(name, out var values)
            ? string.Join(", ", values.Where(value => !string.IsNullOrWhiteSpace(value)))
            : null;

    private static async Task<ApiError?> ReadErrorAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<ApiError>(
                ResponseJsonOptions,
                cancellationToken);
        }
        catch (Exception exception) when (
            exception is HttpRequestException
                or NotSupportedException
                or JsonException
                or ArgumentException)
        {
            return null;
        }
    }

    private static string Query(string path, params (string Name, string? Value)[] values)
    {
        var query = values
            .Where(value => !string.IsNullOrWhiteSpace(value.Value))
            .Select(value =>
                $"{Uri.EscapeDataString(value.Name)}={Uri.EscapeDataString(value.Value!)}");
        return $"{path}?{string.Join("&", query)}";
    }

    private static Guid RequiredId(Guid value, string parameterName) =>
        value == Guid.Empty
            ? throw new ArgumentException("A non-empty identifier is required.", parameterName)
            : value;

    private static string PlanPath(Guid termId) =>
        $"/api/student/terms/{RequiredId(termId, nameof(termId)):D}/registration-plan";
}

public sealed record OfferingDiscoveryQuery(
    string? Search,
    string Eligibility,
    string? Credits,
    string? Day,
    string Availability,
    string Sort,
    int Page = 1,
    int PageSize = 20);

public sealed record RegistrationResponseMetadata(
    string? PolicyVersion,
    string? ReasonCodes,
    string? RegistrationWindowState,
    string? SupportReferencePath);

public sealed record RegistrationApiResult<T>(
    bool IsSuccess,
    T? Value,
    ApiError? Error,
    System.Net.HttpStatusCode StatusCode,
    RegistrationResponseMetadata? Metadata)
{
    public static RegistrationApiResult<T> Success(
        T value,
        System.Net.HttpStatusCode statusCode,
        RegistrationResponseMetadata metadata) =>
        new(true, value, null, statusCode, metadata);

    public static RegistrationApiResult<T> Failure(
        ApiError? error,
        System.Net.HttpStatusCode statusCode) =>
        new(false, default, error, statusCode, null);
}

public sealed record RegistrationPlanApiResult(
    bool IsSuccess,
    RegistrationPlanDto? Value,
    ApiError? Error,
    System.Net.HttpStatusCode StatusCode,
    RegistrationPlanDto? CurrentPlan)
{
    public static RegistrationPlanApiResult Success(
        RegistrationPlanDto value,
        System.Net.HttpStatusCode statusCode) =>
        new(true, value, null, statusCode, null);

    public static RegistrationPlanApiResult Failure(
        ApiError? error,
        System.Net.HttpStatusCode statusCode) =>
        new(false, null, error, statusCode, null);

    public static RegistrationPlanApiResult Stale(
        ApiError error,
        RegistrationPlanDto currentPlan,
        System.Net.HttpStatusCode statusCode) =>
        new(false, null, error, statusCode, currentPlan);
}
