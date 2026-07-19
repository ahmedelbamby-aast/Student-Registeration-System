using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec001;

public sealed class NFR_1EvidenceTests
{
    [Fact]
    public void Critical_routes_have_complete_passing_automated_wcag_evidence()
    {
        using var evidence = JsonDocument.Parse(RepositoryFiles.Read(
            "docs/release-evidence/SPEC-018-NFR-8-browser-matrix.json"));
        var root = evidence.RootElement;

        Assert.Equal("SPEC-018/NFR-8", root.GetProperty("requirement").GetString());
        Assert.Equal("PASS", root.GetProperty("result").GetString());
        Assert.Equal(36, root.GetProperty("browserRouteCombinations").GetInt32());
        Assert.Equal(36, root.GetProperty("passedCombinations").GetInt32());
        Assert.Equal(0, root.GetProperty("failedCombinations").GetInt32());
        Assert.Equal(
            new[]
            {
                "AUTH-02", "AUTH-04", "STU-02", "STU-04", "STU-05",
                "STU-06", "STF-01", "STF-03", "STF-04"
            },
            root.GetProperty("routes").EnumerateArray()
                .Select(route => route.GetString()).ToArray());
        Assert.Equal(4, root.GetProperty("browsers").GetArrayLength());
        Assert.Equal(
            new[]
            {
                "axe serious-or-worse findings",
                "keyboard focus and semantics",
                "44 CSS pixel targets",
                "responsive overflow"
            },
            root.GetProperty("checks").EnumerateArray()
                .Select(check => check.GetString()).ToArray());
    }

    [Fact]
    public void Accessibility_evidence_is_measurable_and_keeps_manual_release_boundary_truthful()
    {
        using var probe = JsonDocument.Parse(RepositoryFiles.Read(
            "docs/release-evidence/SPEC-018-NFR-8-nvda-probe.json"));
        var root = probe.RootElement;
        Assert.Equal("PASS", root.GetProperty("result").GetString());
        Assert.True(root.GetProperty("speechEventCount").GetInt32() > 0);
        Assert.True(root.GetProperty("focusEventCount").GetInt32() > 0);
        Assert.Equal(0, root.GetProperty("keyboardProbeExitCode").GetInt32());
        Assert.Equal("BLOCKED", root.GetProperty("releaseGate").GetString());

        RepositoryFiles.ContainsAll(
            Normalize(RepositoryFiles.Read("docs/release-evidence/SPEC-001-NFR-1.md")),
            "**Automated evidence result:** PASS",
            "36/36",
            "zero serious-or-worse findings",
            "manual Windows/NVDA usability sign-off is not claimed",
            "Production authority: not granted");
    }

    private static string Normalize(string value) => string.Join(
        " ",
        value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
