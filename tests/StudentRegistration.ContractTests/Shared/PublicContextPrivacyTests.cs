using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using StudentRegistration.Contracts;
using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Shared;

public sealed class PublicContextPrivacyTests
{
    private static readonly DateTime ServerTimeUtc =
        new(2026, 7, 13, 8, 30, 0, DateTimeKind.Utc);

    [Fact]
    public void Public_context_has_the_canonical_owner_and_exact_six_fields()
    {
        using var ownership = JsonDocument.Parse(
            RepositoryFiles.Read(".specify/entity-ownership.json"));
        Assert.Equal(
            "src/StudentRegistration.Contracts/PublicContextDto.cs",
            ownership.RootElement.GetProperty("artifactOverrides")
                .GetProperty("006:PublicContextDto")
                .GetString());

        Assert.Equal(
            [
                "RegistrationTermLabel",
                "RegistrationWindowState",
                "ServerTimeUtc",
                "ServiceState",
                "TeachingTermLabel",
                "TimeZoneId"
            ],
            typeof(PublicContextDto).GetProperties()
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal)
                .ToArray());
    }

    [Fact]
    public void Public_context_serializes_only_privacy_safe_public_values()
    {
        var context = CreateContext(
            teachingTermLabel: "Summer 2026",
            registrationTermLabel: "Fall 2026",
            registrationWindowState: RegistrationWindowState.Upcoming,
            serviceState: ServiceState.Available);
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(context, options));
        var root = document.RootElement;

        Assert.Equal(6, root.EnumerateObject().Count());
        Assert.Equal("2026-07-13T08:30:00Z", root.GetProperty("serverTimeUtc").GetString());
        Assert.Equal("Africa/Cairo", root.GetProperty("timeZoneId").GetString());
        Assert.Equal("Summer 2026", root.GetProperty("teachingTermLabel").GetString());
        Assert.Equal("Fall 2026", root.GetProperty("registrationTermLabel").GetString());
        Assert.Equal("upcoming", root.GetProperty("registrationWindowState").GetString());
        Assert.Equal("available", root.GetProperty("serviceState").GetString());

        foreach (var forbidden in new[]
                 {
                     "user", "displayName", "role", "student", "capacity",
                     "health", "topology", "connection", "internal"
                 })
        {
            Assert.DoesNotContain(
                root.EnumerateObject(),
                property => property.Name.Contains(forbidden, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void Nullable_labels_mean_authoritative_absence_and_missing_registration_term_requires_none()
    {
        var noApplicableTerms = CreateContext(
            teachingTermLabel: null,
            registrationTermLabel: null,
            registrationWindowState: RegistrationWindowState.None);

        Assert.Null(noApplicableTerms.TeachingTermLabel);
        Assert.Null(noApplicableTerms.RegistrationTermLabel);
        Assert.Equal(RegistrationWindowState.None, noApplicableTerms.RegistrationWindowState);
        Assert.Throws<ArgumentException>(() => CreateContext(
            registrationTermLabel: null,
            registrationWindowState: RegistrationWindowState.Open));
    }

    [Fact]
    public void Public_context_rejects_non_utc_or_invalid_public_values()
    {
        Assert.Throws<ArgumentException>(() => CreateContext(
            serverTimeUtc: DateTime.SpecifyKind(ServerTimeUtc, DateTimeKind.Unspecified)));
        Assert.Throws<ArgumentException>(() => CreateContext(timeZoneId: "  "));
        Assert.Throws<ArgumentException>(() => CreateContext(teachingTermLabel: "  "));
        Assert.Throws<ArgumentException>(() => CreateContext(registrationTermLabel: "  "));
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateContext(
            registrationWindowState: (RegistrationWindowState)999));
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateContext(
            serviceState: (ServiceState)999));
    }

    [Fact]
    public void Approved_contract_keeps_public_context_separate_from_authenticated_and_internal_data()
    {
        var apiContract = RepositoryFiles.Read(
            "specs/006-domain-class-api-contracts/contracts/api.md");
        var normalizedContract = Regex.Replace(apiContract, @"\s+", " ");
        RepositoryFiles.ContainsAll(
            normalizedContract,
            "interface PublicContextDto",
            "serverTimeUtc: string;",
            "timeZoneId: string;",
            "teachingTermLabel: string | null;",
            "registrationTermLabel: string | null;",
            "registrationWindowState: \"open\" | \"upcoming\" | \"closed\" | \"none\";",
            "serviceState: \"available\" | \"maintenance\" | \"unavailable\";",
            "SPEC-008 owns `GET /api/public/context`");
    }

    private static PublicContextDto CreateContext(
        DateTime? serverTimeUtc = null,
        string timeZoneId = "Africa/Cairo",
        string? teachingTermLabel = null,
        string? registrationTermLabel = "Fall 2026",
        RegistrationWindowState registrationWindowState = RegistrationWindowState.Open,
        ServiceState serviceState = ServiceState.Available) =>
        new(
            serverTimeUtc ?? ServerTimeUtc,
            timeZoneId,
            teachingTermLabel,
            registrationTermLabel,
            registrationWindowState,
            serviceState);
}
