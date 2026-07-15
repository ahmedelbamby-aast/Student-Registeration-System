using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Academics;

namespace StudentRegistration.Client.Features.Academics;

/// <summary>
/// Same-origin facade for the academic-context endpoints owned by SPEC-008.
/// Business, authorization, time, and concurrency decisions remain server-authoritative.
/// </summary>
public sealed class AcademicApiClient
{
    public const string PublicContextMethod = "GET";
    public const string PublicContextPath = "/api/public/context";
    public const string AppContextMethod = "GET";
    public const string AppContextPath = "/api/context";
    public const string AcademicContextMethod = "GET";
    public const string AcademicContextPath = "/api/students/me/academic-context";

    private const string AdminTermsPath = "/api/admin/terms";
    private const string AdminStudentsPath = "/api/admin/students";
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

    // Retained for focused component tests and read-only consumers.
    public AcademicApiClient(HttpClient httpClient)
        : this(httpClient, null)
    {
    }

    public AcademicApiClient(HttpClient httpClient, IJSRuntime? javascript)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _javascript = javascript;
    }

    public Task<AcademicApiResult<PublicContextDto>> GetPublicContextAsync(
        CancellationToken cancellationToken = default) =>
        GetAsync<PublicContextDto>(PublicContextPath, cancellationToken);

    public Task<AcademicApiResult<AppContextDto>> GetAppContextAsync(
        CancellationToken cancellationToken = default) =>
        GetAsync<AppContextDto>(AppContextPath, cancellationToken);

    public Task<AcademicApiResult<StudentAcademicContextDto>> GetStudentAcademicContextAsync(
        int transcriptPage = 1,
        int transcriptPageSize = 20,
        int provenancePage = 1,
        int provenancePageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var path = AcademicContextPath;
        if (transcriptPage != 1 || transcriptPageSize != 20 ||
            provenancePage != 1 || provenancePageSize != 20)
        {
            path +=
                $"?transcriptPage={transcriptPage}&transcriptPageSize={transcriptPageSize}" +
                $"&provenancePage={provenancePage}&provenancePageSize={provenancePageSize}";
        }

        return GetAsync<StudentAcademicContextDto>(path, cancellationToken);
    }

    public Task<AcademicApiResult<Page<AdminTermDto>>> ListAdminTermsAsync(
        string? query = null,
        int page = 1,
        int pageSize = 20,
        TermState? state = null,
        string sort = "code,id",
        CancellationToken cancellationToken = default)
    {
        var parameters = new List<string>
        {
            $"page={page}",
            $"pageSize={pageSize}",
            $"sort={Uri.EscapeDataString(sort)}"
        };
        if (!string.IsNullOrWhiteSpace(query))
        {
            parameters.Insert(0, $"query={Uri.EscapeDataString(query.Trim())}");
        }

        if (state is not null)
        {
            parameters.Add($"state={TermStateValue(state.Value)}");
        }

        return GetAsync<Page<AdminTermDto>>(
            $"{AdminTermsPath}?{string.Join('&', parameters)}",
            cancellationToken);
    }

    public Task<AcademicApiResult<AdminTermDto>> CreateAdminTermAsync(
        CreateTermRequest request,
        CancellationToken cancellationToken = default) =>
        SendMutationAsync<CreateTermRequest, AdminTermDto>(
            HttpMethod.Post,
            AdminTermsPath,
            request,
            cancellationToken);

    public Task<AcademicApiResult<AdminTermDto>> UpdateAdminTermAsync(
        string termId,
        UpdateTermRequest request,
        CancellationToken cancellationToken = default) =>
        SendMutationAsync<UpdateTermRequest, AdminTermDto>(
            HttpMethod.Put,
            $"{AdminTermsPath}/{PathSegment(termId, nameof(termId))}",
            request,
            cancellationToken);

    public Task<AcademicApiResult<AdminTermDto>> PublishAdminRegistrationWindowAsync(
        string termId,
        string windowId,
        PublishRegistrationWindowRequest request,
        CancellationToken cancellationToken = default) =>
        SendMutationAsync<PublishRegistrationWindowRequest, AdminTermDto>(
            HttpMethod.Post,
            $"{AdminTermsPath}/{PathSegment(termId, nameof(termId))}" +
            $"/registration-windows/{PathSegment(windowId, nameof(windowId))}/publish",
            request,
            cancellationToken);

    public Task<AcademicApiResult<Page<AdminStudentLocatorDto>>> SearchAdminStudentsAsync(
        string termId,
        string query,
        int page = 1,
        int pageSize = 20,
        string sort = "universityId,studentId",
        CancellationToken cancellationToken = default)
    {
        var path =
            $"{AdminStudentsPath}?termId={Uri.EscapeDataString(Required(termId, nameof(termId)))}" +
            $"&query={Uri.EscapeDataString(Required(query, nameof(query)).Trim())}" +
            $"&page={page}&pageSize={pageSize}&sort={Uri.EscapeDataString(sort)}";
        return GetAsync<Page<AdminStudentLocatorDto>>(path, cancellationToken);
    }

    public Task<AcademicApiResult<AdminStudentAcademicContextDto>>
        GetAdminStudentAcademicContextAsync(
            string studentId,
            string termId,
            int transcriptPage = 1,
            int transcriptPageSize = 20,
            int provenancePage = 1,
            int provenancePageSize = 20,
            CancellationToken cancellationToken = default)
    {
        var path =
            $"{AdminStudentsPath}/{PathSegment(studentId, nameof(studentId))}/academic-context" +
            $"?termId={Uri.EscapeDataString(Required(termId, nameof(termId)))}" +
            $"&transcriptPage={transcriptPage}&transcriptPageSize={transcriptPageSize}" +
            $"&provenancePage={provenancePage}&provenancePageSize={provenancePageSize}";
        return GetAsync<AdminStudentAcademicContextDto>(path, cancellationToken);
    }

    public Task<AcademicApiResult<AdminStudentAcademicContextDto>>
        CorrectAdminStudentAcademicProfileAsync(
            string studentId,
            AcademicProfileCorrectionRequest request,
            CancellationToken cancellationToken = default) =>
        SendMutationAsync<AcademicProfileCorrectionRequest, AdminStudentAcademicContextDto>(
            HttpMethod.Patch,
            $"{AdminStudentsPath}/{PathSegment(studentId, nameof(studentId))}/academic-profile",
            request,
            cancellationToken);

    private async Task<AcademicApiResult<T>> GetAsync<T>(
        string path,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        return await ReadAsync<T>(response, cancellationToken);
    }

    private async Task<AcademicApiResult<TResponse>> SendMutationAsync<TRequest, TResponse>(
        HttpMethod method,
        string path,
        TRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        using var message = new HttpRequestMessage(method, path)
        {
            Content = JsonContent.Create(request)
        };

        if (_javascript is not null)
        {
            var requestToken = await _javascript.InvokeAsync<string>(
                AntiforgeryInterop,
                cancellationToken);
            if (!string.IsNullOrWhiteSpace(requestToken))
            {
                message.Headers.TryAddWithoutValidation(AntiforgeryHeader, requestToken);
            }
        }

        using var response = await _httpClient.SendAsync(
            message,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        return await ReadAsync<TResponse>(response, cancellationToken);
    }

    private static async Task<AcademicApiResult<T>> ReadAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            try
            {
                var value = await response.Content.ReadFromJsonAsync<T>(
                    ResponseJsonOptions,
                    cancellationToken);
                return value is null
                    ? AcademicApiResult<T>.Failure(null, response.StatusCode)
                    : AcademicApiResult<T>.Success(value, response.StatusCode);
            }
            catch (Exception exception) when (
                exception is HttpRequestException or
                NotSupportedException or
                JsonException or
                ArgumentException)
            {
                return AcademicApiResult<T>.Failure(null, response.StatusCode);
            }
        }

        return AcademicApiResult<T>.Failure(
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
                ResponseJsonOptions,
                cancellationToken);
        }
        catch (Exception exception) when (
            exception is HttpRequestException or
            NotSupportedException or
            JsonException or
            ArgumentException)
        {
            return null;
        }
    }

    private static string PathSegment(string value, string parameterName) =>
        Uri.EscapeDataString(Required(value, parameterName));

    private static string Required(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A non-empty value is required.", parameterName);
        }

        return value;
    }

    private static string TermStateValue(TermState state) => state switch
    {
        TermState.Draft => "draft",
        TermState.RegistrationOpen => "registrationOpen",
        TermState.RegistrationClosed => "registrationClosed",
        TermState.Teaching => "teaching",
        TermState.Completed => "completed",
        TermState.Archived => "archived",
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
    };
}

public sealed record AcademicApiResult<T>(
    bool IsSuccess,
    T? Value,
    ApiError? Error,
    HttpStatusCode StatusCode)
{
    public static AcademicApiResult<T> Success(T value, HttpStatusCode statusCode) =>
        new(true, value, null, statusCode);

    public static AcademicApiResult<T> Failure(
        ApiError? error,
        HttpStatusCode statusCode) =>
        new(false, default, error, statusCode);
}
