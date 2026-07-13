using Bunit;
using StudentRegistration.Client.Components.Registration;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Components;

public sealed class CapacityIndicatorTests
{
    [Fact]
    public void Default_state_announces_supplied_occupied_total_and_remaining_values()
    {
        using var context = new BunitContext();

        var cut = context.Render<CapacityIndicator>(parameters => parameters
            .Add(component => component.AccessibleName, "Group A capacity")
            .Add(component => component.OccupiedLabel, "Occupied")
            .Add(component => component.Occupied, 18)
            .Add(component => component.TotalLabel, "Capacity")
            .Add(component => component.Total, 20)
            .Add(component => component.RemainingLabel, "Remaining")
            .Add(component => component.Remaining, 2));

        var status = cut.Find("[role=status]");
        Assert.Equal("Group A capacity", status.GetAttribute("aria-label"));
        Assert.Equal("polite", status.GetAttribute("aria-live"));
        Assert.Equal("true", status.GetAttribute("aria-atomic"));
        Assert.Equal("default", status.GetAttribute("data-state"));

        var terms = cut.FindAll("dt").Select(element => element.TextContent.Trim()).ToArray();
        var values = cut.FindAll("dd").Select(element => element.TextContent.Trim()).ToArray();
        Assert.Equal(["Occupied", "Capacity", "Remaining"], terms);
        Assert.Equal(["18", "20", "2"], values);
    }

    [Fact]
    public void Loading_and_error_states_render_only_supplied_messages()
    {
        using var context = new BunitContext();

        var loading = context.Render<CapacityIndicator>(parameters => parameters
            .Add(component => component.AccessibleName, "Group B capacity")
            .Add(component => component.OccupiedLabel, "Occupied")
            .Add(component => component.TotalLabel, "Capacity")
            .Add(component => component.RemainingLabel, "Remaining")
            .Add(component => component.IsLoading, true)
            .Add(component => component.LoadingText, "Refreshing capacity"));

        Assert.Equal("true", loading.Find("[role=status]").GetAttribute("aria-busy"));
        Assert.Equal("loading", loading.Find("[role=status]").GetAttribute("data-state"));
        Assert.Equal("Refreshing capacity", loading.Find("[data-loading]").TextContent.Trim());

        var error = context.Render<CapacityIndicator>(parameters => parameters
            .Add(component => component.AccessibleName, "Group C capacity")
            .Add(component => component.OccupiedLabel, "Occupied")
            .Add(component => component.TotalLabel, "Capacity")
            .Add(component => component.RemainingLabel, "Remaining")
            .Add(component => component.ErrorMessage, "Capacity is unavailable"));

        Assert.Equal("error", error.Find("[role=status]").GetAttribute("data-state"));
        Assert.Equal("Capacity is unavailable", error.Find("[role=alert]").TextContent.Trim());
    }

    [Fact]
    public void Noninteractive_indicator_has_no_hover_active_focus_disabled_or_keyboard_state()
    {
        using var context = new BunitContext();

        var cut = context.Render<CapacityIndicator>(parameters => parameters
            .Add(component => component.AccessibleName, "Group D capacity")
            .Add(component => component.OccupiedLabel, "Occupied")
            .Add(component => component.TotalLabel, "Capacity")
            .Add(component => component.RemainingLabel, "Remaining"));

        Assert.Empty(cut.FindAll("button, a, input, select, textarea, [tabindex]"));

        var styles = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Components/Registration/CapacityIndicator.razor.css");
        Assert.Contains("var(--srs-", styles, StringComparison.Ordinal);
        Assert.DoesNotContain(":hover", styles, StringComparison.Ordinal);
        Assert.DoesNotContain(":active", styles, StringComparison.Ordinal);
        Assert.DoesNotContain(":focus", styles, StringComparison.Ordinal);
        Assert.DoesNotContain(":disabled", styles, StringComparison.Ordinal);
    }
}
