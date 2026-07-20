using Bunit;
using StudentRegistration.Client.Components.Registration;

namespace StudentRegistration.Client.UnitTests.Components;

public sealed class CapacityBreakdownTests
{
    [Fact]
    public void Renders_authoritative_values_in_the_required_order()
    {
        using var context = new BunitContext();

        var cut = context.Render<CapacityBreakdown>(parameters => parameters
            .Add(component => component.AccessibleName, "Group A capacity")
            .Add(component => component.TotalLabel, "Total")
            .Add(component => component.Total, 30)
            .Add(component => component.EnrolledLabel, "Enrolled")
            .Add(component => component.Enrolled, 22)
            .Add(component => component.HeldLabel, "Held")
            .Add(component => component.Held, 3)
            .Add(component => component.AvailableLabel, "Available")
            .Add(component => component.Available, 5));

        var root = cut.Find("div.srs-capacity-breakdown");
        Assert.Equal("Group A capacity", root.GetAttribute("aria-label"));
        Assert.Equal("status", root.GetAttribute("role"));
        Assert.Equal(["Total", "Enrolled", "Held", "Available"],
            cut.FindAll("dt").Select(element => element.TextContent.Trim()).ToArray());
        Assert.Equal(["30", "22", "3", "5"],
            cut.FindAll("dd").Select(element => element.TextContent.Trim()).ToArray());
    }

    [Theory]
    [InlineData(-1, 0, 0, 0)]
    [InlineData(10, -1, 0, 11)]
    [InlineData(10, 8, -1, 3)]
    [InlineData(10, 8, 1, -1)]
    [InlineData(10, 8, 1, 2)]
    public void Rejects_negative_or_internally_inconsistent_server_counts(
        int total,
        int enrolled,
        int held,
        int available)
    {
        using var context = new BunitContext();

        Assert.ThrowsAny<ArgumentException>(() => context.Render<CapacityBreakdown>(parameters => parameters
            .Add(component => component.AccessibleName, "Capacity")
            .Add(component => component.TotalLabel, "Total")
            .Add(component => component.Total, total)
            .Add(component => component.EnrolledLabel, "Enrolled")
            .Add(component => component.Enrolled, enrolled)
            .Add(component => component.HeldLabel, "Held")
            .Add(component => component.Held, held)
            .Add(component => component.AvailableLabel, "Available")
            .Add(component => component.Available, available)));
    }
}
