using Bunit;
using Microsoft.AspNetCore.Components;
using StudentRegistration.Client.Components.Display;
using StudentRegistration.Client.Components.Layout;

namespace StudentRegistration.Client.UnitTests.Components;

public sealed class SharedSurfaceTests
{
    [Fact]
    public void Page_header_exposes_one_page_heading_and_optional_context_regions()
    {
        using var context = new BunitContext();

        var cut = context.Render<PageHeader>(parameters => parameters
            .Add(component => component.HeadingId, "registration-heading")
            .Add(component => component.Eyebrow, "Student workspace")
            .Add(component => component.Heading, "Registration")
            .Add(component => component.Description, "Review the current server state.")
            .Add(component => component.Status, builder =>
                builder.AddContent(0, "Open window"))
            .Add(component => component.Actions, builder =>
                builder.AddContent(0, "Primary action")));

        var root = cut.Find("section.srs-page-header");
        Assert.Equal("registration-heading", root.GetAttribute("aria-labelledby"));
        Assert.Equal("Registration", cut.Find("h1#registration-heading").TextContent);
        Assert.Equal("Student workspace", cut.Find(".srs-page-header__eyebrow").TextContent);
        Assert.Equal("Review the current server state.", cut.Find(".srs-page-header__description").TextContent);
        Assert.Contains("Open window", cut.Find(".srs-page-header__status").TextContent, StringComparison.Ordinal);
        Assert.Single(cut.FindAll(".srs-page-header__actions"));
    }

    [Fact]
    public void Surface_card_uses_a_named_section_and_keeps_actions_separate()
    {
        using var context = new BunitContext();

        var cut = context.Render<SurfaceCard>(parameters => parameters
            .Add(component => component.HeadingId, "capacity-heading")
            .Add(component => component.Heading, "Capacity")
            .Add(component => component.Description, "Authoritative counts")
            .Add(component => component.ChildContent, "Card body")
            .Add(component => component.Actions, "Card action"));

        var section = cut.Find("section.srs-surface-card");
        Assert.Equal("capacity-heading", section.GetAttribute("aria-labelledby"));
        Assert.Equal("Capacity", cut.Find("h2#capacity-heading").TextContent);
        Assert.Equal("Card body", cut.Find(".srs-surface-card__content").TextContent);
        Assert.Equal("Card action", cut.Find(".srs-surface-card__actions").TextContent);
    }

    [Fact]
    public void Shared_surfaces_reject_missing_accessible_names()
    {
        using var context = new BunitContext();

        Assert.ThrowsAny<ArgumentException>(() => context.Render<PageHeader>(parameters => parameters
            .Add(component => component.Heading, "Heading")));
        Assert.ThrowsAny<ArgumentException>(() => context.Render<SurfaceCard>(parameters => parameters
            .Add(component => component.HeadingId, "card-heading")
            .Add(component => component.ChildContent, "Body")));
    }
}
