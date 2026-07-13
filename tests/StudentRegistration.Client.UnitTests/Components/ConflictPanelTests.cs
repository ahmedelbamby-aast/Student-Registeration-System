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
}
