using System.Text.Json;
using System.Text.Json.Serialization;
using StudentRegistration.Contracts;
using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Shared;

public sealed class AppContextModelTests
{
    private static readonly DateTime ServerTimeUtc =
        new(2026, 7, 13, 8, 30, 0, DateTimeKind.Utc);

    private static readonly DateTime ExpiresAtUtc =
        new(2026, 7, 13, 10, 30, 0, DateTimeKind.Utc);

    [Fact]
    public void Contracts_have_the_canonical_owner_paths_and_no_framework_dependency()
    {
        using var ownership = JsonDocument.Parse(
            RepositoryFiles.Read(".specify/entity-ownership.json"));
        var overrides = ownership.RootElement.GetProperty("artifactOverrides");

        Assert.Equal(
            "src/StudentRegistration.Contracts/AppContextDto.cs",
            overrides.GetProperty("006:AppContext").GetString());
        Assert.Equal(
            "src/StudentRegistration.Contracts/TermSummaryDto.cs",
            overrides.GetProperty("006:TermSummaryDto").GetString());
        Assert.Equal(
            "src/StudentRegistration.Contracts/RegistrationWindowSummaryDto.cs",
            overrides.GetProperty("006:RegistrationWindowSummaryDto").GetString());
        Assert.Same(typeof(AppContextDto).Assembly, typeof(TermSummaryDto).Assembly);
        Assert.Same(typeof(AppContextDto).Assembly, typeof(RegistrationWindowSummaryDto).Assembly);
        Assert.DoesNotContain(
            typeof(AppContextDto).Assembly.GetReferencedAssemblies(),
            reference =>
                reference.Name?.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal) is true ||
                reference.Name?.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal) is true ||
                reference.Name?.StartsWith("Microsoft.Data.SqlClient", StringComparison.Ordinal) is true);
    }

    [Fact]
    public void Registration_window_summary_is_bounded_versioned_and_matches_context_state()
    {
        var window = CreateWindow(RegistrationWindowState.Open);

        Assert.Equal("window-summer-all", window.Id);
        Assert.Equal(RegistrationWindowState.Open, window.State);
        Assert.Equal(new DateTime(2026, 7, 13, 8, 0, 0, DateTimeKind.Utc), window.OpensAtUtc);
        Assert.Equal(new DateTime(2026, 7, 13, 18, 0, 0, DateTimeKind.Utc), window.ClosesAtUtc);
        Assert.Equal("CQoLDA==", window.RowVersion);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateWindow(RegistrationWindowState.None));
        Assert.Throws<ArgumentException>(() => new RegistrationWindowSummaryDto(
            "window-summer-all",
            RegistrationWindowState.Open,
            DateTime.SpecifyKind(window.OpensAtUtc, DateTimeKind.Local),
            window.ClosesAtUtc,
            window.RowVersion));

        Assert.Throws<ArgumentException>(() => new AppContextDto(
            ServerTimeUtc,
            "Africa/Cairo",
            teachingTerm: null,
            CreateTerm(TermState.RegistrationOpen),
            RegistrationWindowState.Open,
            registrationWindow: null,
            ServiceState.Available,
            "Ahmed Student",
            ["Student"],
            "Student",
            SessionState.Active,
            ExpiresAtUtc,
            "/support/reference"));
        Assert.Throws<ArgumentException>(() => new AppContextDto(
            ServerTimeUtc,
            "Africa/Cairo",
            teachingTerm: null,
            CreateTerm(TermState.RegistrationOpen),
            RegistrationWindowState.Open,
            CreateWindow(RegistrationWindowState.Upcoming),
            ServiceState.Available,
            "Ahmed Student",
            ["Student"],
            "Student",
            SessionState.Active,
            ExpiresAtUtc,
            "/support/reference"));
    }

    [Fact]
    public void Term_summary_is_a_separate_minimal_versioned_contract()
    {
        var term = CreateTerm(TermState.RegistrationOpen);

        Assert.Equal("term-2026-summer", term.Id);
        Assert.Equal("2026-SUMMER", term.Code);
        Assert.Equal("Summer 2026", term.Label);
        Assert.Equal(TermState.RegistrationOpen, term.State);
        Assert.Equal("AQIDBA==", term.RowVersion);
        Assert.Equal(
            new[] { "Code", "Id", "Label", "RowVersion", "State" },
            typeof(TermSummaryDto).GetProperties()
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateTerm((TermState)999));
    }

    [Fact]
    public void Teaching_and_registration_terms_are_independently_nullable_authoritative_values()
    {
        var registrationOnly = CreateContext(
            teachingTerm: null,
            registrationTerm: CreateTerm(TermState.RegistrationOpen),
            registrationWindowState: RegistrationWindowState.Open);
        var teachingOnly = CreateContext(
            teachingTerm: CreateTerm(TermState.Teaching),
            registrationTerm: null,
            registrationWindowState: RegistrationWindowState.None);

        Assert.Null(registrationOnly.TeachingTerm);
        Assert.NotNull(registrationOnly.RegistrationTerm);
        Assert.NotNull(teachingOnly.TeachingTerm);
        Assert.Null(teachingOnly.RegistrationTerm);
        Assert.Throws<ArgumentException>(() => CreateContext(
            teachingTerm: null,
            registrationTerm: null,
            registrationWindowState: RegistrationWindowState.Open));
    }

    [Fact]
    public void Active_role_obeys_the_authenticated_session_state_invariant()
    {
        var active = CreateContext(
            authorizedRoles: ["Student"],
            activeRole: "Student",
            sessionState: SessionState.Active);
        var selectionRequired = CreateContext(
            authorizedRoles: ["Lecturer", "TeachingAssistant"],
            activeRole: null,
            sessionState: SessionState.RoleSelectionRequired);

        Assert.Equal("Student", active.ActiveRole);
        Assert.Null(selectionRequired.ActiveRole);
        Assert.Throws<ArgumentException>(() => CreateContext(
            authorizedRoles: ["Student"],
            activeRole: null,
            sessionState: SessionState.Active));
        Assert.Throws<ArgumentException>(() => CreateContext(
            authorizedRoles: ["Student"],
            activeRole: null,
            sessionState: SessionState.RoleSelectionRequired));
        Assert.Throws<ArgumentException>(() => CreateContext(
            authorizedRoles: ["Student"],
            activeRole: "Admin",
            sessionState: SessionState.Active));
    }

    [Fact]
    public void Context_rejects_undeclared_contract_states()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateContext(
            registrationWindowState: (RegistrationWindowState)999));
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateContext(
            serviceState: (ServiceState)999));
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateContext(
            sessionState: (SessionState)999));
    }

    [Fact]
    public void Web_serialization_uses_the_approved_field_and_state_shapes()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
        var context = CreateContext(
            teachingTerm: CreateTerm(TermState.Teaching),
            registrationTerm: CreateTerm(TermState.RegistrationOpen),
            registrationWindowState: RegistrationWindowState.Open,
            authorizedRoles: ["Student", "TeachingAssistant"],
            activeRole: null,
            sessionState: SessionState.RoleSelectionRequired);

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(context, options));
        var root = document.RootElement;

        Assert.Equal("2026-07-13T08:30:00Z", root.GetProperty("serverTimeUtc").GetString());
        Assert.Equal("Africa/Cairo", root.GetProperty("timeZoneId").GetString());
        Assert.Equal(
            "teaching",
            root.GetProperty("teachingTerm").GetProperty("state").GetString());
        Assert.Equal(
            "registrationOpen",
            root.GetProperty("registrationTerm").GetProperty("state").GetString());
        Assert.Equal("open", root.GetProperty("registrationWindowState").GetString());
        Assert.Equal(
            "window-summer-all",
            root.GetProperty("registrationWindow").GetProperty("id").GetString());
        Assert.Equal(
            "open",
            root.GetProperty("registrationWindow").GetProperty("state").GetString());
        Assert.Equal("available", root.GetProperty("serviceState").GetString());
        Assert.Equal(
            "role-selection-required",
            root.GetProperty("sessionState").GetString());
        Assert.Equal("/support/reference", root.GetProperty("supportReferencePath").GetString());
        Assert.False(root.TryGetProperty("rowVersionInternal", out _));
        Assert.False(root.TryGetProperty("navigation", out _));
    }

    private static TermSummaryDto CreateTerm(TermState state) =>
        new(
            "term-2026-summer",
            "2026-SUMMER",
            "Summer 2026",
            state,
            "AQIDBA==");

    private static RegistrationWindowSummaryDto CreateWindow(
        RegistrationWindowState state) =>
        new(
            "window-summer-all",
            state,
            new DateTime(2026, 7, 13, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 13, 18, 0, 0, DateTimeKind.Utc),
            "CQoLDA==");

    private static AppContextDto CreateContext(
        TermSummaryDto? teachingTerm = null,
        TermSummaryDto? registrationTerm = null,
        RegistrationWindowState registrationWindowState = RegistrationWindowState.None,
        IReadOnlyList<string>? authorizedRoles = null,
        string? activeRole = "Student",
        SessionState sessionState = SessionState.Active,
        ServiceState serviceState = ServiceState.Available) =>
        new(
            ServerTimeUtc,
            "Africa/Cairo",
            teachingTerm,
            registrationTerm,
            registrationWindowState,
            registrationWindowState is RegistrationWindowState.None
                ? null
                : CreateWindow(registrationWindowState),
            serviceState,
            "Ahmed Student",
            authorizedRoles ?? ["Student"],
            activeRole,
            sessionState,
            ExpiresAtUtc,
            "/support/reference");
}
