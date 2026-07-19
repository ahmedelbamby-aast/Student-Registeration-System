using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec018;

public sealed class AC_4Tests
{
    [Fact]
    public void Critical_routes_pass_automated_keyboard_and_signed_screen_reader_gates()
    {
        var evidence = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-018-screen-reader-manual.md");

        Spec018AcceptanceEvidence.Matches(evidence, @"(?im)^\*\*Status:\*\*\s+PASS\s*$");
        Spec018AcceptanceEvidence.Matches(
            evidence,
            @"(?im)^\*\*Release gate:\*\*\s+PASS\s*$");
        Assert.DoesNotMatch(
            @"(?i)NOT EXECUTED|NOT RECORDED|UNSIGNED|UNKNOWN - EXECUTION REQUIRED",
            evidence);
        Spec018AcceptanceEvidence.ContainsAll(
            evidence,
            "Chrome",
            "Edge",
            "Firefox",
            "Playwright WebKit",
            "NVDA",
            "Windows");
        foreach (var field in new[]
                 {
                     "tester",
                     "date",
                     "assistiveTechnology",
                     "route",
                     "scenario",
                     "result",
                     "defectLinks",
                     "uxQaSignOff"
                 })
        {
            Spec018AcceptanceEvidence.Matches(
                evidence,
                $@"(?im)^\s*{field}\s*:\s*\S.+$");
        }

        Spec018AcceptanceEvidence.Matches(
            evidence,
            @"(?im)Serious automated findings unresolved\s*\|\s*0\s*\|");
        Spec018AcceptanceEvidence.Matches(
            evidence,
            @"(?im)Critical/major manual barriers unresolved\s*\|\s*0\s*\|");
    }
}
