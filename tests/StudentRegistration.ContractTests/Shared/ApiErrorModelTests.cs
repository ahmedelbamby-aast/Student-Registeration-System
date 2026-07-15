using System.Text.Json;
using StudentRegistration.Contracts;
using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Shared;

public sealed class ApiErrorModelTests
{
    [Fact]
    public void Contract_has_the_canonical_owner_and_only_approved_fields()
    {
        using var ownership = JsonDocument.Parse(
            RepositoryFiles.Read(".specify/entity-ownership.json"));
        var path = ownership.RootElement.GetProperty("artifactOverrides")
            .GetProperty("006:ApiError")
            .GetString();

        Assert.Equal("src/StudentRegistration.Contracts/ApiError.cs", path);
        Assert.Equal(
            new[] { "Code", "CorrelationId", "CurrentVersion", "FieldErrors", "Message" },
            typeof(ApiError).GetProperties()
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Required_values_are_enforced_and_optional_values_default_to_null()
    {
        var error = new ApiError(
            "CONTEXT_UNAVAILABLE",
            "The service is temporarily unavailable.",
            "correlation-123");

        Assert.Equal("CONTEXT_UNAVAILABLE", error.Code);
        Assert.Equal("The service is temporarily unavailable.", error.Message);
        Assert.Equal("correlation-123", error.CorrelationId);
        Assert.Null(error.FieldErrors);
        Assert.Null(error.CurrentVersion);

        Assert.Throws<ArgumentException>(() =>
            new ApiError("", "Safe message.", "correlation-123"));
        Assert.Throws<ArgumentException>(() =>
            new ApiError("ERROR", " ", "correlation-123"));
        Assert.Throws<ArgumentException>(() =>
            new ApiError("ERROR", "Safe message.", ""));
        Assert.Throws<ArgumentException>(() =>
            new ApiError("ERROR", "Safe message.", "correlation-123", currentVersion: " "));
    }

    [Fact]
    public void Field_errors_require_nonempty_fields_and_messages_and_are_defensively_copied()
    {
        var messages = new List<string> { "University ID is required." };
        var details = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
        {
            ["universityId"] = messages
        };
        var error = new ApiError(
            "VALIDATION_ERROR",
            "One or more fields are invalid.",
            "correlation-456",
            details,
            "AQIDBA==");

        messages.Add("This mutation must not alter the contract.");
        details["password"] = ["Password is required."];

        Assert.Equal(["University ID is required."], error.FieldErrors!["universityId"]);
        Assert.False(error.FieldErrors.ContainsKey("password"));
        Assert.Equal("AQIDBA==", error.CurrentVersion);

        Assert.Throws<ArgumentException>(() => new ApiError(
            "VALIDATION_ERROR",
            "One or more fields are invalid.",
            "correlation-456",
            new Dictionary<string, IReadOnlyList<string>> { [""] = ["Required."] }));
        Assert.Throws<ArgumentException>(() => new ApiError(
            "VALIDATION_ERROR",
            "One or more fields are invalid.",
            "correlation-456",
            new Dictionary<string, IReadOnlyList<string>> { ["universityId"] = [] }));
    }

    [Fact]
    public void Field_errors_are_bounded_for_safe_predictable_responses()
    {
        var maximum = Enumerable.Range(1, 20).ToDictionary(
            index => $"field{index}",
            _ => (IReadOnlyList<string>)Enumerable.Repeat(new string('x', 256), 5).ToArray(),
            StringComparer.Ordinal);

        var error = new ApiError(
            "VALIDATION_ERROR",
            "One or more fields are invalid.",
            "correlation-bounded",
            maximum);

        Assert.Equal(20, error.FieldErrors!.Count);
        Assert.All(error.FieldErrors.Values, messages => Assert.Equal(5, messages.Count));

        maximum["field21"] = ["Too many fields."];
        Assert.Throws<ArgumentException>(() => new ApiError(
            "VALIDATION_ERROR",
            "One or more fields are invalid.",
            "correlation-too-many-fields",
            maximum));
        Assert.Throws<ArgumentException>(() => new ApiError(
            "VALIDATION_ERROR",
            "One or more fields are invalid.",
            "correlation-too-many-messages",
            new Dictionary<string, IReadOnlyList<string>>
            {
                ["field"] = Enumerable.Repeat("message", 6).ToArray()
            }));
        Assert.Throws<ArgumentException>(() => new ApiError(
            "VALIDATION_ERROR",
            "One or more fields are invalid.",
            "correlation-message-too-long",
            new Dictionary<string, IReadOnlyList<string>>
            {
                ["field"] = [new string('x', 257)]
            }));
    }

    [Fact]
    public void Web_serialization_omits_absent_optional_details_and_uses_safe_names()
    {
        var minimal = new ApiError(
            "CONTEXT_UNAVAILABLE",
            "The service is temporarily unavailable.",
            "correlation-123");
        var detailed = new ApiError(
            "STALE_VERSION",
            "The record changed. Refresh and try again.",
            "correlation-789",
            new Dictionary<string, IReadOnlyList<string>>
            {
                ["expectedRowVersion"] = ["The supplied version is stale."]
            },
            "AQIDBA==");

        using var minimalJson = JsonDocument.Parse(
            JsonSerializer.Serialize(minimal, JsonSerializerOptions.Web));
        using var detailedJson = JsonDocument.Parse(
            JsonSerializer.Serialize(detailed, JsonSerializerOptions.Web));

        Assert.Equal("CONTEXT_UNAVAILABLE", minimalJson.RootElement.GetProperty("code").GetString());
        Assert.Equal(
            "The service is temporarily unavailable.",
            minimalJson.RootElement.GetProperty("message").GetString());
        Assert.Equal(
            "correlation-123",
            minimalJson.RootElement.GetProperty("correlationId").GetString());
        Assert.False(minimalJson.RootElement.TryGetProperty("fieldErrors", out _));
        Assert.False(minimalJson.RootElement.TryGetProperty("currentVersion", out _));

        Assert.Equal(
            "The supplied version is stale.",
            detailedJson.RootElement.GetProperty("fieldErrors")
                .GetProperty("expectedRowVersion")[0]
                .GetString());
        Assert.Equal("AQIDBA==", detailedJson.RootElement.GetProperty("currentVersion").GetString());
        Assert.False(detailedJson.RootElement.TryGetProperty("exception", out _));
        Assert.False(detailedJson.RootElement.TryGetProperty("stackTrace", out _));
        Assert.False(detailedJson.RootElement.TryGetProperty("sql", out _));
    }
}
