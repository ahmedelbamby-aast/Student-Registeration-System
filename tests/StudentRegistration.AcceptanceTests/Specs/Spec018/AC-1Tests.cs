using System.Text.Json;
using System.Text.RegularExpressions;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec018;

public sealed class AC_1Tests
{
    [Fact]
    public void Target_mix_meets_latency_error_and_registration_invariant_budgets()
    {
        var load = Spec018AcceptanceEvidence.RequirePassingNfr(2);
        var latency = Spec018AcceptanceEvidence.RequirePassingNfr(3);
        var invariants = Spec018AcceptanceEvidence.RequirePassingNfr(4);
        var replicas = Spec018AcceptanceEvidence.RequirePassingNfr(5);
        var failures = Spec018AcceptanceEvidence.RequirePassingNfr(6);

        Spec018AcceptanceEvidence.ContainsAll(
            load,
            "10 minutes",
            "75 submissions/s",
            "300 reads/s");
        Spec018AcceptanceEvidence.ContainsAll(
            replicas,
            "two independently addressable API replicas");
        Spec018AcceptanceEvidence.Matches(latency, @"Catalogue p95\s*<=\s*300 ms");
        Spec018AcceptanceEvidence.Matches(latency, @"commit p95\s*<=\s*(2,000 ms|2 s)");
        Spec018AcceptanceEvidence.Matches(latency, @"optimizer p95\s*<=\s*500 ms");
        Spec018AcceptanceEvidence.ContainsAll(
            invariants,
            "zero overbooking",
            "zero duplicate active offering enrollment",
            "zero partial atomic submissions");
        Spec018AcceptanceEvidence.Matches(
            failures,
            @"strictly below 0\.1%|<\s*0\.1%");
    }
}

internal static class Spec018AcceptanceEvidence
{
    public static string RequirePassingNfr(int number)
    {
        var evidence = RepositoryFiles.Read(
            $"docs/release-evidence/SPEC-018-NFR-{number}.md");
        Matches(evidence, @"(?im)^\*\*Release result:\*\*\s+PASS\s*$");
        Matches(evidence, @"(?im)^Runtime execution result:\s+PASS\b");
        return evidence;
    }

    public static string RequirePassingTraceability()
    {
        var evidence = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-018-traceability.md");
        Assert.DoesNotMatch(
            @"(?im)\|\s*(PENDING|BLOCKED|MISSING|NOT EXECUTED)\s*\|",
            evidence);
        return evidence;
    }

    public static string RequireReleaseApproval()
    {
        var evidence = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-018-release-approval.md");
        RequireField(evidence, "releaseDecision", "approved");
        RequireField(evidence, "productionAuthorized", "false");
        RequireField(evidence, "officialAastmtApproval", "false");
        return evidence;
    }

    public static JsonDocument ReadJson(string relativePath) =>
        JsonDocument.Parse(RepositoryFiles.Read(relativePath));

    public static void RequireField(string evidence, string name, string value) =>
        Matches(
            evidence,
            $@"(?im)^\s*{Regex.Escape(name)}\s*:\s*{Regex.Escape(value)}\s*$");

    public static void ContainsAll(string evidence, params string[] values) =>
        RepositoryFiles.ContainsAll(evidence, values);

    public static void Matches(string evidence, string pattern) =>
        Assert.Matches(new Regex(pattern, RegexOptions.CultureInvariant), evidence);
}
