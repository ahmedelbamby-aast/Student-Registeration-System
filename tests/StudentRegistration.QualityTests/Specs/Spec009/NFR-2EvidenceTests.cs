using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using StudentRegistration.Academics.Application;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec009;

public sealed class NFR_2EvidenceTests
{
    private const string EvidencePath =
        "docs/release-evidence/SPEC-009-NFR-2.md";
    private const string TestPath =
        "tests/StudentRegistration.QualityTests/Specs/Spec009/NFR-2EvidenceTests.cs";

    [Fact]
    public void Same_policy_version_and_input_produce_byte_identical_ordered_results()
    {
        var service = new PolicyAdministrationService();
        var draft = PolicyAdministrationService.CreateDemoPolicySet(
            Guid.Parse("00000000-0000-0000-0000-000000009201"));
        Assert.True(service.Validate(draft).IsValid);
        var input = new PolicySimulationInput(
            Gpa: 2.5m,
            EarnedCredits: 96m,
            Standing: "Active",
            RegistrationWindowOpen: true,
            HasBlockingHold: false,
            RequestedCredits: 18,
            RequestedCourseCodes: ["DS413"],
            CompletedCourseCodes: [],
            AllGroupsHaveCapacity: true,
            HasTimetableConflict: false);

        var baseline = service.Simulate(draft, input);
        var baselineBytes = JsonSerializer.SerializeToUtf8Bytes(baseline);
        for (var iteration = 0; iteration < 100; iteration++)
        {
            Assert.Equal(
                baselineBytes,
                JsonSerializer.SerializeToUtf8Bytes(service.Simulate(draft, input)));
        }

        Assert.Equal(
            baseline.RuleResults
                .Select(result => result.RuleCode)
                .Order(StringComparer.Ordinal),
            baseline.RuleResults.Select(result => result.RuleCode));
        Assert.All(
            baseline.RuleResults,
            result =>
            {
                Assert.False(string.IsNullOrWhiteSpace(result.Explanation));
                Assert.False(string.IsNullOrWhiteSpace(result.SourceReference));
            });
    }

    [Fact]
    public void Evidence_is_source_bound_and_records_repeatable_simulation()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);
        RepositoryFiles.ContainsAll(
            evidence,
            "# SPEC-009 NFR-2 Deterministic Simulation Evidence",
            "100 repeated simulations",
            "byte-identical",
            "ordinal rule-code order",
            "source reference",
            "2.0 GPA",
            "96 earned credits",
            "1 passed",
            "0 failed",
            $"Quality test normalized-LF SHA-256: `{SourceHash(TestPath)}`",
            "**Result: PASS.**");
        Assert.DoesNotMatch(
            @"(?i)\b(?:PENDING|TODO|TBD|FIXME|PLACEHOLDER|NOT\s+RUN|NOT\s+EXECUTED)\b",
            evidence);
    }

    private static string SourceHash(string relativePath)
    {
        var source = RepositoryFiles.Read(relativePath)
            .Replace("\r\n", "\n", StringComparison.Ordinal);
        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(source)));
    }
}
