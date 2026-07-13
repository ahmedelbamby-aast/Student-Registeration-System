using Bunit;
using StudentRegistration.Client.Components.Scheduling;
using StudentRegistration.Client.Features.Frontend.Models;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Components;

public sealed class ConflictPanelTests
{
    private const string ConflictContractPath =
        "specs/003-ux-storyboard-accessibility/design/components/conflict-panel.md";
    private const string SubmissionBlockersPath =
        "specs/003-ux-storyboard-accessibility/design/states/submission-blockers.md";
    private const string TimetableEquivalencePath =
        "specs/003-ux-storyboard-accessibility/design/components/timetable-equivalence.md";

    [Fact]
    public void Panel_names_the_conflict_and_renders_every_group_overlap_alternative_and_manual_action()
    {
        using var context = new BunitContext();

        var cut = Render(context);
        var region = cut.Find("section[role=region]");

        Assert.Equal("schedule-conflict-heading", region.GetAttribute("aria-labelledby"));
        Assert.Equal("polite", region.GetAttribute("aria-live"));
        Assert.Equal("default", region.GetAttribute("data-state"));
        Assert.Equal("true", cut.Find("[data-conflict-icon]").GetAttribute("aria-hidden"));
        Assert.Contains("Conflict", cut.Find("h2").TextContent, StringComparison.Ordinal);
        Assert.Contains("Submission is blocked", cut.Markup, StringComparison.Ordinal);

        var groups = cut.FindAll("[data-conflict-group]");
        Assert.Equal(2, groups.Count);
        Assert.Contains("AI301", groups[0].TextContent, StringComparison.Ordinal);
        Assert.Contains("Machine Learning", groups[0].TextContent, StringComparison.Ordinal);
        Assert.Contains("G1", groups[0].TextContent, StringComparison.Ordinal);

        var overlaps = cut.FindAll("[data-overlap-slot]");
        Assert.Equal(2, overlaps.Count);
        Assert.Contains("Monday", overlaps[0].TextContent, StringComparison.Ordinal);
        Assert.Contains("10:00", overlaps[0].TextContent, StringComparison.Ordinal);
        Assert.Contains("11:30", overlaps[0].TextContent, StringComparison.Ordinal);
        Assert.Contains("AI301:G1", overlaps[0].TextContent, StringComparison.Ordinal);

        var alternative = cut.Find("[data-alternative-id=ALT-01]");
        Assert.Contains("Move Computer Vision to G3", alternative.TextContent, StringComparison.Ordinal);
        Assert.Contains("AI302:G3", alternative.TextContent, StringComparison.Ordinal);

        var links = cut.FindAll("[data-resolution-link]");
        Assert.Equal(2, links.Count);
        Assert.Equal("/student/registration/groups/AI302", links[0].GetAttribute("href"));
        Assert.Null(links[0].GetAttribute("tabindex"));
    }

    [Fact]
    public void Disabled_loading_and_error_states_do_not_expose_false_manual_actions()
    {
        using var context = new BunitContext();

        var disabled = Render(
            context,
            isDisabled: true,
            disabledReason: "Registration window is closed");
        Assert.Equal("disabled", disabled.Find("section").GetAttribute("data-state"));
        Assert.Equal("true", disabled.Find("section").GetAttribute("aria-disabled"));
        Assert.Empty(disabled.FindAll("a"));
        Assert.Equal(2, disabled.FindAll("[data-resolution-link][aria-disabled=true]").Count);
        Assert.Equal(
            "Registration window is closed",
            disabled.Find("[data-disabled-reason]").TextContent.Trim());

        var loading = Render(context, isLoading: true, loadingText: "Checking conflicts");
        Assert.Equal("loading", loading.Find("section").GetAttribute("data-state"));
        Assert.Equal("true", loading.Find("section").GetAttribute("aria-busy"));
        Assert.Equal("Checking conflicts", loading.Find("[role=status]").TextContent.Trim());
        Assert.Empty(loading.FindAll("[data-conflict-group]"));

        var error = Render(context, errorMessage: "Conflict details are unavailable");
        Assert.Equal("error", error.Find("section").GetAttribute("data-state"));
        Assert.Equal(
            "Conflict details are unavailable",
            error.Find("[role=alert]").TextContent.Trim());
        Assert.Empty(error.FindAll("[data-resolution-link]"));
    }

    [Fact]
    public void Panel_styles_cover_native_link_interactions_and_token_only_states()
    {
        var styles = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Components/Scheduling/ConflictPanel.razor.css");

        RepositoryFiles.ContainsAll(
            styles,
            ":hover",
            ":active",
            ":focus-visible",
            "[aria-disabled=\"true\"]",
            "var(--srs-",
            "min-height: var(--srs-sizing-interactive-minimum)");
        Assert.DoesNotContain("#", styles, StringComparison.Ordinal);
    }

    [Fact]
    public void Conflict_contract_uses_noncolor_blocking_text_complete_intervals_and_manual_actions()
    {
        var contract = RepositoryFiles.Read(ConflictContractPath);

        RepositoryFiles.ContainsAll(
            contract,
            "Red X",
            "Conflict",
            "subject code and name",
            "group code",
            "day",
            "start",
            "end",
            "explanation",
            "change group",
            "remove subject",
            "manual resolution",
            "not color alone",
            "polite live region",
            "does not steal focus");
    }

    [Fact]
    public void Submission_contract_disables_submit_and_exposes_every_textual_blocker()
    {
        var contract = RepositoryFiles.Read(SubmissionBlockersPath);

        RepositoryFiles.ContainsAll(
            contract,
            "hard conflict",
            "blocking validation",
            "disabled",
            "every blocking reason",
            "validation summary",
            "aria-describedby",
            "server revalidation",
            "double activation",
            "cannot override");
    }

    [Fact]
    public void Timetable_contract_keeps_calendar_and_chronological_list_information_equivalent()
    {
        var contract = RepositoryFiles.Read(TimetableEquivalencePath);

        RepositoryFiles.ContainsAll(
            contract,
            "same canonical collection",
            "subject code and name",
            "group code",
            "Lecturer",
            "Teaching Assistant",
            "room/location",
            "day",
            "start",
            "end",
            "chronological",
            "calendar is never the only representation");
    }

    private static IRenderedComponent<ConflictPanel> Render(
        BunitContext context,
        bool isDisabled = false,
        string? disabledReason = null,
        bool isLoading = false,
        string? loadingText = null,
        string? errorMessage = null) =>
        context.Render<ConflictPanel>(parameters => parameters
            .Add(component => component.ComponentId, "schedule-conflict")
            .Add(component => component.Heading, "Conflict")
            .Add(component => component.Explanation, "Submission is blocked until this conflict is resolved.")
            .Add(component => component.SubjectsHeading, "Involved subjects")
            .Add(component => component.SubjectLabel, "Subject")
            .Add(component => component.GroupLabel, "Group")
            .Add(component => component.OverlapsHeading, "Overlapping times")
            .Add(component => component.DayLabel, "Day")
            .Add(component => component.StartLabel, "Start")
            .Add(component => component.EndLabel, "End")
            .Add(component => component.InvolvedGroupsLabel, "Groups")
            .Add(component => component.AlternativesHeading, "Suggested alternatives")
            .Add(component => component.ReplacementGroupsLabel, "Replacement groups")
            .Add(component => component.ResolutionHeading, "Resolve manually")
            .Add(component => component.Conflict, CreateConflict())
            .Add(component => component.IsDisabled, isDisabled)
            .Add(component => component.DisabledReason, disabledReason)
            .Add(component => component.IsLoading, isLoading)
            .Add(component => component.LoadingText, loadingText)
            .Add(component => component.ErrorMessage, errorMessage));

    private static ConflictView CreateConflict() =>
        new(
            [
                new ConflictSubjectGroupView("AI301", "Machine Learning", "G1"),
                new ConflictSubjectGroupView("AI302", "Computer Vision", "G2")
            ],
            [
                new ConflictOverlapSlotView(
                    DayOfWeek.Monday,
                    new TimeOnly(10, 0),
                    new TimeOnly(11, 30),
                    ["AI301:G1", "AI302:G2"]),
                new ConflictOverlapSlotView(
                    DayOfWeek.Wednesday,
                    new TimeOnly(12, 0),
                    new TimeOnly(13, 0),
                    ["AI301:G1", "AI302:G2"])
            ],
            [
                new ConflictAlternativeView(
                    "ALT-01",
                    "Move Computer Vision to G3",
                    ["AI302:G3"])
            ],
            [
                new ConflictResolutionLinkView(
                    "Change Computer Vision group",
                    "/student/registration/groups/AI302"),
                new ConflictResolutionLinkView(
                    "Remove Machine Learning",
                    "/student/registration/subjects/AI301/remove")
            ]);
}
