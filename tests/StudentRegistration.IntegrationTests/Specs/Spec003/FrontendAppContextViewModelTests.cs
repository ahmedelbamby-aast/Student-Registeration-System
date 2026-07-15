using System.Text.Json;
using StudentRegistration.Client.Features.Frontend.Models;
using StudentRegistration.Contracts;

namespace StudentRegistration.IntegrationTests.Specs.Spec003;

public sealed class FrontendAppContextViewModelTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 13, 12, 0, 0, TimeSpan.FromHours(3));

    [Fact]
    public void Serializes_the_complete_authoritative_context_contract()
    {
        var model = new FrontendAppContextView(
            Now,
            "Africa/Cairo",
            "Summer 2026",
            "Summer 2026",
            RegistrationWindowState.Open,
            Window(RegistrationWindowState.Open),
            "Ahmed Student",
            ["Student"],
            "Student",
            roleSelectionRequired: false,
            "active",
            Now.AddHours(1),
            "available",
            "/status/support-reference");

        using var document = JsonDocument.Parse(
            JsonSerializer.Serialize(model, JsonSerializerOptions.Web));
        var root = document.RootElement;

        Assert.Equal("Africa/Cairo", root.GetProperty("timeZone").GetString());
        Assert.Equal("Summer 2026", root.GetProperty("teachingTerm").GetString());
        Assert.Equal("Summer 2026", root.GetProperty("registrationTerm").GetString());
        Assert.Equal("open", root.GetProperty("registrationWindowState").GetString());
        Assert.Equal(
            "open",
            root.GetProperty("registrationWindow").GetProperty("state").GetString());
        Assert.Equal("Student", root.GetProperty("activeRole").GetString());
        Assert.Equal("/status/support-reference", root.GetProperty("supportReferencePath").GetString());
        Assert.Single(root.GetProperty("authorizedRoles").EnumerateArray());
    }

    [Fact]
    public void Rejects_a_missing_active_role_when_role_selection_is_not_required()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new FrontendAppContextView(
                Now,
                "Africa/Cairo",
                "Summer 2026",
                "Summer 2026",
                RegistrationWindowState.Open,
                Window(RegistrationWindowState.Open),
                "Ahmed Student",
                ["Student"],
                activeRole: null,
                roleSelectionRequired: false,
                "active",
                Now.AddHours(1),
                "available",
                "/status/support-reference"));

        Assert.Equal("activeRole", exception.ParamName);
    }

    [Fact]
    public void Serializes_missing_terms_and_window_as_an_explicit_none_state()
    {
        var model = new FrontendAppContextView(
            Now,
            "Africa/Cairo",
            teachingTerm: null,
            registrationTerm: null,
            RegistrationWindowState.None,
            registrationWindow: null,
            "Ahmed Student",
            ["Student"],
            "Student",
            roleSelectionRequired: false,
            "active",
            Now.AddHours(1),
            "available",
            "/status/support-reference");

        using var document = JsonDocument.Parse(
            JsonSerializer.Serialize(model, JsonSerializerOptions.Web));
        var root = document.RootElement;

        Assert.Equal(JsonValueKind.Null, root.GetProperty("teachingTerm").ValueKind);
        Assert.Equal(JsonValueKind.Null, root.GetProperty("registrationTerm").ValueKind);
        Assert.Equal("none", root.GetProperty("registrationWindowState").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("registrationWindow").ValueKind);
    }

    [Fact]
    public void Rejects_a_window_summary_that_does_not_match_the_server_state()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new FrontendAppContextView(
                Now,
                "Africa/Cairo",
                "Summer 2026",
                "Summer 2026",
                RegistrationWindowState.Open,
                Window(RegistrationWindowState.Upcoming),
                "Ahmed Student",
                ["Student"],
                "Student",
                roleSelectionRequired: false,
                "active",
                Now.AddHours(1),
                "available",
                "/status/support-reference"));

        Assert.Equal("registrationWindow", exception.ParamName);
    }

    private static RegistrationWindowSummaryDto Window(RegistrationWindowState state) =>
        new(
            "WINDOW-1",
            state,
            Now.AddDays(-1).UtcDateTime,
            Now.AddDays(7).UtcDateTime,
            "window-v1");
}
