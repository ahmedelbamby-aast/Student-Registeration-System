using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Academics;

namespace StudentRegistration.Client.Features.Academics;

public sealed class CatalogueApiClient
{
    private const string ProgramsPath = "/api/admin/programs";
    private const string VersionsPath = "/api/admin/catalogue/versions";
    private const string DraftsPath = "/api/admin/catalogue/drafts";
    private const string ImportsPath = "/api/admin/catalogue/imports";
    private const string PoliciesPath = "/api/admin/policies";
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

    public CatalogueApiClient(HttpClient httpClient)
        : this(httpClient, null)
    {
    }

    public CatalogueApiClient(HttpClient httpClient, IJSRuntime? javascript)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _javascript = javascript;
    }

    public Task<AcademicApiResult<Page<ProgramAdminDto>>> ListProgramsAsync(
        int page = 1,
        int pageSize = 20,
        string sort = "code,id",
        CancellationToken cancellationToken = default) =>
        GetAsync<Page<ProgramAdminDto>>(
            $"{ProgramsPath}?page={page}&pageSize={pageSize}&sort={Uri.EscapeDataString(sort)}",
            cancellationToken);

    public Task<AcademicApiResult<Page<CatalogueVersionSummaryDto>>> ListVersionsAsync(
        int page = 1,
        int pageSize = 20,
        string sort = "publishedAtUtc-desc,id",
        CancellationToken cancellationToken = default) =>
        GetAsync<Page<CatalogueVersionSummaryDto>>(
            $"{VersionsPath}?page={page}&pageSize={pageSize}&sort={Uri.EscapeDataString(sort)}",
            cancellationToken);

    public Task<AcademicApiResult<CatalogueDraftDto>> GetDraftAsync(
        Guid draftId,
        CancellationToken cancellationToken = default) =>
        GetAsync<CatalogueDraftDto>(
            $"{DraftsPath}/{RequiredId(draftId, nameof(draftId)):D}",
            cancellationToken);

    public Task<AcademicApiResult<CatalogueDraftDto>> UpdateDraftAsync(
        Guid draftId,
        CatalogueDraftMutationRequest request,
        CancellationToken cancellationToken = default) =>
        SendMutationAsync<CatalogueDraftMutationRequest, CatalogueDraftDto>(
            HttpMethod.Put,
            $"{DraftsPath}/{RequiredId(draftId, nameof(draftId)):D}",
            request,
            cancellationToken);

    public Task<AcademicApiResult<ImportBatchDto>> CreateImportAsync(
        CreateImportRequest request,
        CancellationToken cancellationToken = default) =>
        SendMutationAsync<CreateImportRequest, ImportBatchDto>(
            HttpMethod.Post,
            ImportsPath,
            request,
            cancellationToken);

    public Task<AcademicApiResult<ImportBatchDto>> GetImportAsync(
        Guid importId,
        CancellationToken cancellationToken = default) =>
        GetAsync<ImportBatchDto>(
            $"{ImportsPath}/{RequiredId(importId, nameof(importId)):D}",
            cancellationToken);

    public Task<AcademicApiResult<CatalogueValidationResult>> ValidateImportAsync(
        Guid importId,
        CatalogueValidationRequest request,
        CancellationToken cancellationToken = default) =>
        SendMutationAsync<CatalogueValidationRequest, CatalogueValidationResult>(
            HttpMethod.Post,
            $"{ImportsPath}/{RequiredId(importId, nameof(importId)):D}/validate",
            request,
            cancellationToken);

    public Task<AcademicApiResult<CatalogueVersionSummaryDto>> PublishImportAsync(
        Guid importId,
        PublishVersionRequest request,
        CancellationToken cancellationToken = default) =>
        SendMutationAsync<PublishVersionRequest, CatalogueVersionSummaryDto>(
            HttpMethod.Post,
            $"{ImportsPath}/{RequiredId(importId, nameof(importId)):D}/publish",
            request,
            cancellationToken);

    public Task<AcademicApiResult<Page<PolicySetAdminDto>>> ListPoliciesAsync(
        int page = 1,
        int pageSize = 20,
        string sort = "effectiveFromUtc-desc,id",
        CancellationToken cancellationToken = default) =>
        GetAsync<Page<PolicySetAdminDto>>(
            $"{PoliciesPath}?page={page}&pageSize={pageSize}&sort={Uri.EscapeDataString(sort)}",
            cancellationToken);

    public Task<AcademicApiResult<PolicySetAdminDto>> CreatePolicyAsync(
        CreatePolicySetRequest request,
        CancellationToken cancellationToken = default) =>
        SendMutationAsync<CreatePolicySetRequest, PolicySetAdminDto>(
            HttpMethod.Post,
            PoliciesPath,
            request,
            cancellationToken);

    public Task<AcademicApiResult<PolicySetAdminDto>> UpdatePolicyAsync(
        Guid policySetId,
        PolicySetMutationRequest request,
        CancellationToken cancellationToken = default) =>
        SendMutationAsync<PolicySetMutationRequest, PolicySetAdminDto>(
            HttpMethod.Put,
            $"{PoliciesPath}/{RequiredId(policySetId, nameof(policySetId)):D}",
            request,
            cancellationToken);

    public Task<AcademicApiResult<CatalogueValidationResult>> ValidatePolicyAsync(
        Guid policySetId,
        PolicyValidationRequest request,
        CancellationToken cancellationToken = default) =>
        SendMutationAsync<PolicyValidationRequest, CatalogueValidationResult>(
            HttpMethod.Post,
            $"{PoliciesPath}/{RequiredId(policySetId, nameof(policySetId)):D}/validate",
            request,
            cancellationToken);

    public Task<AcademicApiResult<PolicySimulationResult>> SimulatePolicyAsync(
        Guid policySetId,
        PolicySimulationRequest request,
        CancellationToken cancellationToken = default) =>
        SendMutationAsync<PolicySimulationRequest, PolicySimulationResult>(
            HttpMethod.Post,
            $"{PoliciesPath}/{RequiredId(policySetId, nameof(policySetId)):D}/simulate",
            request,
            cancellationToken);

    public Task<AcademicApiResult<PolicySetAdminDto>> PublishPolicyAsync(
        Guid policySetId,
        PolicyPublishRequest request,
        CancellationToken cancellationToken = default) =>
        SendMutationAsync<PolicyPublishRequest, PolicySetAdminDto>(
            HttpMethod.Post,
            $"{PoliciesPath}/{RequiredId(policySetId, nameof(policySetId)):D}/publish",
            request,
            cancellationToken);

    private async Task<AcademicApiResult<T>> GetAsync<T>(
        string path,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(
            path,
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
                exception is HttpRequestException
                    or NotSupportedException
                    or JsonException
                    or ArgumentException)
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
            exception is HttpRequestException
                or NotSupportedException
                or JsonException
                or ArgumentException)
        {
            return null;
        }
    }

    private static Guid RequiredId(Guid value, string parameterName) =>
        value == Guid.Empty
            ? throw new ArgumentException("A non-empty identifier is required.", parameterName)
            : value;
}
