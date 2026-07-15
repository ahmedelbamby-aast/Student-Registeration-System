using Bunit;
using StudentRegistration.Client.Components.Layout;
using StudentRegistration.Client.Features.Frontend.Models;
using StudentRegistration.Contracts;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Components;

public sealed class AppShellTests
{
    private const string CssPath =
        "src/StudentRegistration.Client/Components/Layout/AppShell.razor.css";

    [Fact]
    public void Renders_native_landmarks_and_the_complete_authoritative_context()
    {
        using var context = new BunitContext();
        var appContext = CreateAppContext();

        var cut = context.Render<AppShell>(parameters => parameters
            .Add(component => component.Context, appContext)
            .Add(component => component.SkipLinkLabel, "Skip to content")
            .Add(component => component.BrandLogoSource, "/brand/aastmt-logo.png")
            .Add(component => component.BrandLogoAccessibleName, "Academy logo")
            .Add(component => component.ServerDateTimeLabel, "Server time")
            .Add(component => component.TeachingTermLabel, "Teaching term")
            .Add(component => component.RegistrationTermLabel, "Registration term")
            .Add(component => component.RegistrationWindowLabel, "Registration window")
            .Add(component => component.AuthorizedRolesLabel, "Authorized roles")
            .Add(component => component.ActiveRoleLabel, "Active role")
            .Add(component => component.SessionLabel, "Session")
            .Add(component => component.ServiceLabel, "Service")
            .Add(component => component.SupportLinkLabel, "Support")
            .Add(component => component.Navigation, "<nav aria-label=\"Student navigation\"><a href=\"/student\">Dashboard</a></nav>")
            .Add(component => component.ChildContent, "<p id=\"protected-content\">Registration workspace</p>"));

        Assert.Equal("#main-content", cut.Find("a.srs-skip-link").GetAttribute("href"));
        Assert.NotNull(cut.Find("header[role='banner']"));
        Assert.NotNull(cut.Find("main#main-content"));
        Assert.NotNull(cut.Find("footer[role='contentinfo']"));
        Assert.Equal("Academy logo", cut.Find("img").GetAttribute("alt"));
        Assert.Equal("/brand/aastmt-logo.png", cut.Find("img").GetAttribute("src"));
        Assert.Equal(appContext.ServerDateTime.ToString("O"), cut.Find("time[data-context='server-time']").GetAttribute("datetime"));
        Assert.Equal("/status/support", cut.Find("a[data-context='support']").GetAttribute("href"));
        Assert.NotNull(cut.Find("#protected-content"));
        Assert.Contains("Africa/Cairo", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Spring 2026", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Summer 2026", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Ahmed Student", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Student", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Active", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Available", cut.Markup, StringComparison.Ordinal);
        Assert.Equal("open", cut.Find("[data-context='registration-window']").GetAttribute("data-window-state"));
        Assert.Equal(2, cut.FindAll("[data-context='registration-window'] time").Count);
    }

    [Fact]
    public void Renders_the_server_none_state_without_inventing_window_times()
    {
        using var context = new BunitContext();
        var appContext = CreateAppContext(
            registrationTerm: null,
            RegistrationWindowState.None,
            registrationWindow: null);

        var cut = context.Render<AppShell>(parameters => parameters
            .Add(component => component.Context, appContext)
            .Add(component => component.RegistrationTermLabel, "Registration term")
            .Add(component => component.RegistrationWindowLabel, "Registration window")
            .Add(component => component.NoRegistrationTermLabel, "No registration term")
            .Add(component => component.NoRegistrationWindowLabel, "No registration window"));

        Assert.Equal(
            "No registration term",
            cut.Find("[data-context='registration-term']").TextContent.Trim());
        var window = cut.Find("[data-context='registration-window']");
        Assert.Equal("none", window.GetAttribute("data-window-state"));
        Assert.Contains("No registration window", window.TextContent, StringComparison.Ordinal);
        Assert.Empty(window.QuerySelectorAll("time"));
    }

    [Fact]
    public void Loading_and_error_states_keep_landmarks_and_fail_closed()
    {
        using var context = new BunitContext();

        var loading = context.Render<AppShell>(parameters => parameters
            .Add(component => component.IsLoading, true)
            .Add(component => component.LoadingLabel, "Loading application context")
            .Add(component => component.SkipLinkLabel, "Skip to content")
            .Add(component => component.ChildContent, "<p id=\"protected-content\">Protected</p>"));

        Assert.Equal("status", loading.Find("[data-state='loading']").GetAttribute("role"));
        Assert.Equal("true", loading.Find("[data-state='loading']").GetAttribute("aria-busy"));
        Assert.NotNull(loading.Find("main"));
        Assert.Empty(loading.FindAll("#protected-content"));

        var status = new UiStatus(
            "CONTEXT_UNAVAILABLE",
            "Context unavailable",
            "Try the safe status action.",
            UiStatusSeverity.Error,
            [new UiStatusAction("status", "Open status", "/status/503")],
            "REF-003");
        var failed = context.Render<AppShell>(parameters => parameters
            .Add(component => component.Status, status)
            .Add(component => component.SkipLinkLabel, "Skip to content")
            .Add(component => component.ChildContent, "<p id=\"protected-content\">Protected</p>"));

        Assert.Equal("alert", failed.Find("[data-state='error']").GetAttribute("role"));
        Assert.Contains("Context unavailable", failed.Markup, StringComparison.Ordinal);
        Assert.Contains("REF-003", failed.Markup, StringComparison.Ordinal);
        Assert.Equal("/status/503", failed.Find("[data-state='error'] a").GetAttribute("href"));
        Assert.Empty(failed.FindAll("#protected-content"));
    }

    [Fact]
    public void Interactive_shell_styles_use_tokens_for_hover_active_and_focus_states()
    {
        var css = RepositoryFiles.Read(CssPath);

        RepositoryFiles.ContainsAll(
            css,
            ".srs-skip-link:focus",
            ".srs-app-shell a:hover",
            ".srs-app-shell a:active",
            ".srs-app-shell a:focus-visible",
            "var(--srs-");
    }

    private static FrontendAppContextView CreateAppContext(
        string? registrationTerm = "Summer 2026",
        RegistrationWindowState registrationWindowState = RegistrationWindowState.Open,
        RegistrationWindowSummaryDto? registrationWindow = null) =>
        new(
            new DateTimeOffset(2026, 7, 13, 10, 30, 0, TimeSpan.FromHours(3)),
            "Africa/Cairo",
            "Spring 2026",
            registrationTerm,
            registrationWindowState,
            registrationWindow ?? (registrationWindowState is RegistrationWindowState.None
                ? null
                : new RegistrationWindowSummaryDto(
                    "WINDOW-1",
                    registrationWindowState,
                    new DateTime(2026, 7, 13, 5, 0, 0, DateTimeKind.Utc),
                    new DateTime(2026, 7, 20, 14, 0, 0, DateTimeKind.Utc),
                    "window-v1")),
            "Ahmed Student",
            ["Student"],
            "Student",
            false,
            "Active",
            new DateTimeOffset(2026, 7, 13, 12, 30, 0, TimeSpan.FromHours(3)),
            "Available",
            "/status/support");
}
