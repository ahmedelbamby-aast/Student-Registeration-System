using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Admin;

namespace StudentRegistration.Client.Features.Operations;

public sealed class AdminOperationsApiClient
{
    private const string MetricsPath = "/api/admin/operations/metrics";
    private const string AuditPath = "/api/admin/audit";
    private const string ExportsPath = "/api/admin/exports";
    private const string AntiforgeryHeader = "X-XSRF-TOKEN";
    private const string AntiforgeryInterop =
        "StudentRegistration.antiforgery.getRequestToken";

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            RespectNullableAnnotations = true,
            RespectRequiredConstructorParameters = true
        };

    private readonly HttpClient _httpClient;
    private readonly IJSRuntime? _javascript;

    public AdminOperationsApiClient(HttpClient httpClient)
        : this(httpClient, null)
    {
    }

    public AdminOperationsApiClient(HttpClient httpClient, IJSRuntime? javascript)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _javascript = javascript;
    }

    public Task<AdminOperationsApiResult<AdminOperationsMetricsDto>> GetMetricsAsync(
        Guid termId,
        Guid registrationWindowId,
        CancellationToken cancellationToken = default) =>
        GetAsync<AdminOperationsMetricsDto>(
            $"{MetricsPath}?termId={RequiredId(termId, nameof(termId)):D}" +
            $"&registrationWindowId={RequiredId(registrationWindowId, nameof(registrationWindowId)):D}",
            cancellationToken);

    public Task<AdminOperationsApiResult<AdminAuditEventPageDto>> SearchAuditAsync(
        int page = 1,
        int pageSize = 20,
        DateTime? occurredFromUtc = null,
        DateTime? occurredToUtc = null,
        Guid? actorId = null,
        string? action = null,
        string? sourceStream = null,
        CancellationToken cancellationToken = default)
    {
        var values = new List<string>
        {
            $"page={page}",
            $"pageSize={pageSize}"
        };
        Add(values, "occurredFromUtc", occurredFromUtc?.ToUniversalTime().ToString("O"));
        Add(values, "occurredToUtc", occurredToUtc?.ToUniversalTime().ToString("O"));
        Add(values, "actorId", actorId?.ToString("D"));
        Add(values, "action", action);
        Add(values, "sourceStream", sourceStream);
        return GetAsync<AdminAuditEventPageDto>(
            $"{AuditPath}?{string.Join('&', values)}",
            cancellationToken);
    }

    public Task<AdminOperationsApiResult<AdminExportJobDto>> CreateExportAsync(
        CreateAdminExportRequest request,
        CancellationToken cancellationToken = default) =>
        SendAsync<CreateAdminExportRequest, AdminExportJobDto>(
            HttpMethod.Post,
            ExportsPath,
            request,
            addAntiforgery: true,
            cancellationToken);

    public Task<AdminOperationsApiResult<AdminExportJobDto>> GetExportStatusAsync(
        Guid jobId,
        CancellationToken cancellationToken = default) =>
        GetAsync<AdminExportJobDto>(
            $"{ExportsPath}/{RequiredId(jobId, nameof(jobId)):D}",
            cancellationToken);

    public async Task<AdminOperationsApiResult<byte[]>> DownloadExportAsync(
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            $"{ExportsPath}/{RequiredId(jobId, nameof(jobId)):D}/download",
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return AdminOperationsApiResult<byte[]>.Success(
                await response.Content.ReadAsByteArrayAsync(cancellationToken),
                response.StatusCode);
        }

        return AdminOperationsApiResult<byte[]>.Failure(
            await ReadErrorAsync(response, cancellationToken),
            response.StatusCode);
    }

    private async Task<AdminOperationsApiResult<T>> GetAsync<T>(
        string path,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(
            path,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        return await ReadAsync<T>(response, cancellationToken);
    }

    private async Task<AdminOperationsApiResult<TResponse>> SendAsync<TRequest, TResponse>(
        HttpMethod method,
        string path,
        TRequest request,
        bool addAntiforgery,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        using var message = new HttpRequestMessage(method, path)
        {
            Content = JsonContent.Create(request)
        };
        if (addAntiforgery && _javascript is not null)
        {
            var token = await _javascript.InvokeAsync<string>(
                AntiforgeryInterop,
                cancellationToken);
            if (!string.IsNullOrWhiteSpace(token))
            {
                message.Headers.TryAddWithoutValidation(AntiforgeryHeader, token);
            }
        }

        using var response = await _httpClient.SendAsync(
            message,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        return await ReadAsync<TResponse>(response, cancellationToken);
    }

    private static async Task<AdminOperationsApiResult<T>> ReadAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            try
            {
                var value = await response.Content.ReadFromJsonAsync<T>(
                    JsonOptions,
                    cancellationToken);
                return value is null
                    ? AdminOperationsApiResult<T>.Failure(null, response.StatusCode)
                    : AdminOperationsApiResult<T>.Success(value, response.StatusCode);
            }
            catch (Exception exception) when (exception is
                HttpRequestException or NotSupportedException or JsonException)
            {
                return AdminOperationsApiResult<T>.Failure(null, response.StatusCode);
            }
        }

        return AdminOperationsApiResult<T>.Failure(
            await ReadErrorAsync(response, cancellationToken),
            response.StatusCode);
    }

    private static async Task<ApiError?> ReadErrorAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<ApiError>(
                JsonOptions,
                cancellationToken);
        }
        catch (Exception exception) when (exception is
            HttpRequestException or NotSupportedException or JsonException)
        {
            return null;
        }
    }

    private static Guid RequiredId(Guid value, string parameterName) =>
        value == Guid.Empty
            ? throw new ArgumentException("A non-empty identifier is required.", parameterName)
            : value;

    private static void Add(ICollection<string> values, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            values.Add($"{name}={Uri.EscapeDataString(value)}");
        }
    }
}

public sealed record AdminOperationsApiResult<T>(
    bool IsSuccess,
    T? Value,
    ApiError? Error,
    HttpStatusCode StatusCode)
{
    public static AdminOperationsApiResult<T> Success(T value, HttpStatusCode statusCode) =>
        new(true, value, null, statusCode);

    public static AdminOperationsApiResult<T> Failure(
        ApiError? error,
        HttpStatusCode statusCode) => new(false, default, error, statusCode);
}
