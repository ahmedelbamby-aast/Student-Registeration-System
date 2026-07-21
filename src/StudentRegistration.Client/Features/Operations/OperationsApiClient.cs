using System.Text.Json;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Operations;

namespace StudentRegistration.Client.Features.Operations;

public sealed class OperationsApiClient(HttpClient httpClient)
{
    public const string HealthPath = "/api/health";
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public async Task<OperationsApiResult<HealthSummary>> GetHealthAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(
            HealthPath,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        string payload;
        try
        {
            payload = await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is HttpRequestException or ObjectDisposedException)
        {
            return OperationsApiResult<HealthSummary>.Failure(null);
        }

        if (response.IsSuccessStatusCode ||
            response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
        {
            try
            {
                var summary = JsonSerializer.Deserialize<HealthSummary>(payload, JsonOptions);
                if (summary is not null)
                {
                    return OperationsApiResult<HealthSummary>.Success(summary);
                }
            }
            catch (HttpRequestException)
            {
            }
            catch (NotSupportedException)
            {
            }
            catch (JsonException)
            {
            }
            catch (ArgumentException)
            {
                // A structured API error is not a health summary. Continue to
                // the error-contract parser below instead of surfacing a
                // constructor validation exception to the status page.
            }
        }

        ApiError? error = null;
        try
        {
            error = JsonSerializer.Deserialize<ApiError>(payload, JsonOptions);
        }
        catch (HttpRequestException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (JsonException)
        {
        }
        catch (ArgumentException)
        {
        }

        return OperationsApiResult<HealthSummary>.Failure(error);
    }
}

public sealed record OperationsApiResult<T>(bool IsSuccess, T? Value, ApiError? Error)
{
    public static OperationsApiResult<T> Success(T value) => new(true, value, null);

    public static OperationsApiResult<T> Failure(ApiError? error) =>
        new(false, default, error);
}
