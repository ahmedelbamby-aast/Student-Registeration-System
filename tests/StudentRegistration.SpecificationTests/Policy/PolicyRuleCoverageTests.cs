using System.Text.RegularExpressions;
using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Policy;

public sealed class PolicyRuleCoverageTests
{
    [Fact]
    public void Demo_profile_covers_every_required_rule_category_and_boundary()
    {
        var coverage = RepositoryFiles.Read(
            "specs/002-aastmt-policy-rulebook/contracts/policy-rule-coverage.md");

        RepositoryFiles.ContainsAll(
            coverage,
            "RegistrationWindow",
            "AcademicStanding",
            "BlockingHold",
            "Prerequisite",
            "CreditLoad",
            "ProbationLoad",
            "RepeatEligibility",
            "Capacity",
            "MeetingConflict",
            "minimum 9",
            "target 18",
            "maximum 18",
            "GPA below 2.0",
            "maximum 12",
            "FirstSuccessfulCommit",
            "travel buffer 0");
    }

    [Fact]
    public void Demo_curriculum_has_exactly_19_rows_and_field_level_credit_provenance()
    {
        var curriculum = RepositoryFiles.Read("docs/DEMO_CURRICULUM.md");
        var coverage = RepositoryFiles.Read(
            "specs/002-aastmt-policy-rulebook/contracts/policy-rule-coverage.md");

        var rows = Regex.Matches(curriculum, @"(?m)^\|\s*\d+\s*\|\s*[A-Z]{2}\d{3}\s*\|");
        Assert.Equal(19, rows.Count);
        RepositoryFiles.ContainsAll(
            coverage,
            "exactly 19",
            "OfficialAASTMT",
            "Credits=3",
            "SyntheticDemo",
            "`DEMO-`");
    }
}
