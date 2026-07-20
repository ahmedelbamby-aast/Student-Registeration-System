using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec018;

public sealed class AC_4Tests
{
    [Fact]
    public void Critical_routes_have_automated_evidence_and_explicit_manual_demo_waiver()
    {
        var evidence = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-018-screen-reader-manual.md");

        Spec018AcceptanceEvidence.Matches(
            evidence,
            @"(?im)^\*\*Status:\*\*\s+WAIVED-DEMO\s*$");
        Spec018AcceptanceEvidence.Matches(
            evidence,
            @"(?im)^\*\*Release gate:\*\*\s+WAIVED-DEMO\s*$");
        Spec018AcceptanceEvidence.ContainsAll(
            evidence,
            "**Production authority:** Not granted",
            "**Approved by:** Ahmed ELbamby",
            "**Approval date:** 2026-07-20",
            "Chrome",
            "Edge",
            "Firefox",
            "Playwright WebKit",
            "NVDA",
            "NOT PERFORMED - DEMO WAIVER",
            "manual NVDA scenarios are not",
            "productionAuthorized: false");
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
            @"(?im)Critical/major manual barriers unresolved\s*\|\s*NOT ASSESSED - MANUAL DEMO WAIVER\s*\|");
        Assert.DoesNotContain("**Status:** PASS", evidence, StringComparison.OrdinalIgnoreCase);
    }
}
