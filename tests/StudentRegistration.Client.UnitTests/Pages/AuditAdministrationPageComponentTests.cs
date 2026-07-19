using Bunit;
using Microsoft.AspNetCore.Components;
using StudentRegistration.Client;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class AuditAdministrationPageComponentTests
{
    [Fact]
    public void Adm_09_renders_bounded_filters_and_keeps_export_actions_disabled_until_server_binding()
    {
        using var context = new BunitContext();
        var pageType = typeof(App).Assembly.GetType(
            "StudentRegistration.Client.Pages.AuditAdministrationPage");
        Assert.NotNull(pageType);

        var cut = context.Render<DynamicComponent>(parameters =>
            parameters.Add(component => component.Type, pageType));

        Assert.NotNull(cut.Find("main#main-content"));
        Assert.Equal("100", cut.Find("input[name='pageSize']").GetAttribute("max"));
        Assert.True(cut.Find("button").HasAttribute("disabled"));
        Assert.Contains("Only the server can authorize a scoped download", cut.Markup);
    }

    [Fact]
    public void Adm_09_preserves_bounded_filters_paging_immutable_detail_and_export_states()
    {
        var page = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/AuditAdministrationPage.razor");

        RepositoryFiles.ContainsAll(
            page,
            "aria-label=\"@(LocalizedUiText.Get(\"Scoped audit filters\"))\"",
            "name=\"pageSize\"",
            "max=\"100\"",
            "data-testid=\"audit-event-table\"",
            "data-testid=\"audit-event-card-list\"",
            "Immutable event detail",
            "Page 1 of 1",
            "queued",
            "ready",
            "failed",
            "expired",
            "restricted");
        Assert.DoesNotContain("Edit event", page, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Delete event", page, StringComparison.OrdinalIgnoreCase);
    }
}
