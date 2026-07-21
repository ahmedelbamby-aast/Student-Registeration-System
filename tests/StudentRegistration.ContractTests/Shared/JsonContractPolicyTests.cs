using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StudentRegistration.Api.Composition;
using StudentRegistration.Contracts;

namespace StudentRegistration.ContractTests.Shared;

public sealed class JsonContractPolicyTests
{
    [Fact]
    public void Policy_uses_web_names_documented_enum_strings_and_rejects_enum_numbers()
    {
        var options = ResolveOptions();
        var probe = new SerializationProbe(
            new DateTime(2026, 7, 13, 8, 30, 0, DateTimeKind.Utc),
            1234.5m,
            "Africa/Cairo",
            TermState.RegistrationOpen);

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(probe, options));
        var root = document.RootElement;

        Assert.Equal("2026-07-13T08:30:00Z", root.GetProperty("occurredAtUtc").GetString());
        Assert.Equal("1234.5", root.GetProperty("creditValue").GetRawText());
        Assert.Equal("Africa/Cairo", root.GetProperty("timeZoneId").GetString());
        Assert.Equal("registrationOpen", root.GetProperty("termState").GetString());
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<SerializationProbe>(
                """
                {"occurredAtUtc":"2026-07-13T08:30:00Z","creditValue":1234.5,"timeZoneId":"Africa/Cairo","termState":1}
                """,
                options));
    }

    [Fact]
    public void Decimal_serialization_is_invariant_under_a_non_latin_current_culture()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ar-EG");
            var json = JsonSerializer.Serialize(
                new { CreditValue = 18.5m },
                ResolveOptions());

            Assert.Equal("{\"creditValue\":18.5}", json);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void Contract_specific_nullability_is_preserved()
    {
        var options = ResolveOptions();
        var context = new AppContextDto(
            new DateTime(2026, 7, 13, 8, 30, 0, DateTimeKind.Utc),
            "Africa/Cairo",
            teachingTerm: null,
            registrationTerm: null,
            RegistrationWindowState.None,
            registrationWindow: null,
            ServiceState.Available,
            "Ahmed Student",
            ["Student"],
            "Student",
            SessionState.Active,
            new DateTime(2026, 7, 13, 10, 30, 0, DateTimeKind.Utc),
            "/support/reference");
        var error = new ApiError(
            "CONTEXT_UNAVAILABLE",
            "The service is temporarily unavailable.",
            "correlation-123");

        using var contextJson = JsonDocument.Parse(JsonSerializer.Serialize(context, options));
        using var errorJson = JsonDocument.Parse(JsonSerializer.Serialize(error, options));

        Assert.Equal(JsonValueKind.Null, contextJson.RootElement.GetProperty("teachingTerm").ValueKind);
        Assert.Equal(JsonValueKind.Null, contextJson.RootElement.GetProperty("registrationTerm").ValueKind);
        Assert.Equal(JsonValueKind.Null, contextJson.RootElement.GetProperty("registrationWindow").ValueKind);
        Assert.False(errorJson.RootElement.TryGetProperty("fieldErrors", out _));
        Assert.False(errorJson.RootElement.TryGetProperty("currentVersion", out _));
    }

    [Fact]
    public void Client_web_defaults_deserialize_the_server_enum_contract()
    {
        var teachingTerm = new TermSummaryDto(
            "term-teaching",
            "2026-SUMMER",
            "Summer 2026",
            TermState.Teaching,
            "AQIDBA==");
        var registrationTerm = new TermSummaryDto(
            "term-registration",
            "2026-FALL",
            "Fall 2026",
            TermState.RegistrationOpen,
            "BQYHCA==");
        var serverContext = new AppContextDto(
            new DateTime(2026, 7, 13, 8, 30, 0, DateTimeKind.Utc),
            "Africa/Cairo",
            teachingTerm,
            registrationTerm,
            RegistrationWindowState.Open,
            new RegistrationWindowSummaryDto(
                "window-fall-all",
                RegistrationWindowState.Open,
                new DateTime(2026, 7, 13, 8, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 7, 13, 18, 0, 0, DateTimeKind.Utc),
                "CQoLDA=="),
            ServiceState.Available,
            "Ahmed Student",
            ["Lecturer"],
            activeRole: "Lecturer",
            SessionState.Active,
            new DateTime(2026, 7, 13, 10, 30, 0, DateTimeKind.Utc),
            "/support/reference");

        var serverJson = JsonSerializer.Serialize(serverContext, ResolveOptions());
        var clientContext = JsonSerializer.Deserialize<AppContextDto>(
            serverJson,
            JsonSerializerOptions.Web);

        Assert.NotNull(clientContext);
        Assert.Equal(TermState.Teaching, clientContext.TeachingTerm!.State);
        Assert.Equal(TermState.RegistrationOpen, clientContext.RegistrationTerm!.State);
        Assert.Equal(RegistrationWindowState.Open, clientContext.RegistrationWindowState);
        Assert.Equal("window-fall-all", clientContext.RegistrationWindow!.Id);
        Assert.Equal(ServiceState.Available, clientContext.ServiceState);
        Assert.Equal(SessionState.Active, clientContext.SessionState);
    }

    private static JsonSerializerOptions ResolveOptions()
    {
        var services = new ServiceCollection();
        services.AddStudentRegistrationJsonContracts();
        using var provider = services.BuildServiceProvider();

        return provider.GetRequiredService<IOptions<JsonOptions>>()
            .Value
            .SerializerOptions;
    }

    private sealed record SerializationProbe(
        DateTime OccurredAtUtc,
        decimal CreditValue,
        string TimeZoneId,
        TermState TermState);
}
