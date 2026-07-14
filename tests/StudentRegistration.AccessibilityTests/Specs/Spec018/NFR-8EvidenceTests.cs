using System.Globalization;
using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AccessibilityTests.Specs.Spec018;

public sealed class Nfr8EvidenceTests
{
    private const string ManualEvidencePath =
        "docs/release-evidence/SPEC-018-screen-reader-manual.md";

    [Theory]
    [InlineData("semantic-text-primary", "semantic-surface-default", 4.5)]
    [InlineData("semantic-text-secondary", "semantic-surface-default", 4.5)]
    [InlineData("semantic-link", "semantic-surface-default", 4.5)]
    [InlineData("semantic-danger-text", "semantic-danger-surface", 4.5)]
    [InlineData("semantic-focus-ring", "semantic-surface-default", 3.0)]
    public void Current_static_color_tokens_meet_their_minimum_contrast_contract(
        string foregroundName,
        string backgroundName,
        double minimumRatio)
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(
            "src/StudentRegistration.Client/wwwroot/design/design-tokens.json"));
        var colors = document.RootElement.GetProperty("tokens").GetProperty("color");
        var foreground = colors.GetProperty(foregroundName).GetString()!;
        var background = colors.GetProperty(backgroundName).GetString()!;

        Assert.True(
            ContrastRatio(foreground, background) >= minimumRatio,
            $"{foregroundName} on {backgroundName} is below {minimumRatio}:1.");
    }

    [Fact]
    public void Current_static_css_has_focus_target_forced_color_and_reduced_motion_guards()
    {
        var tokens = RepositoryFiles.Read(
            "src/StudentRegistration.Client/wwwroot/css/design-tokens.css");
        var app = RepositoryFiles.Read(
            "src/StudentRegistration.Client/wwwroot/css/app.css");

        RepositoryFiles.ContainsAll(
            tokens,
            "--srs-sizing-interactive-minimum: 2.75rem",
            "--srs-focus-outline-width",
            "prefers-reduced-motion: reduce",
            "forced-colors: active");
        RepositoryFiles.ContainsAll(
            app,
            ":focus-visible",
            "outline-color: var(--srs-focus-ring-color)",
            "outline-width: var(--srs-focus-outline-width)");
    }

    [Fact]
    public void Current_shared_component_source_has_landmark_focus_and_live_region_guards()
    {
        var shell = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Components/Layout/AppShell.razor");
        var validation = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Components/Forms/AccessibleValidationSummary.razor");
        var alert = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Components/Feedback/Alert.razor");
        var conflict = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Components/Scheduling/ConflictPanel.razor");

        RepositoryFiles.ContainsAll(
            shell,
            "srs-skip-link",
            "href=\"#@MainContentId\"",
            "<main id=\"@MainContentId\" tabindex=\"-1\">",
            "role=\"banner\"",
            "role=\"contentinfo\"");
        RepositoryFiles.ContainsAll(
            validation,
            "role=\"alert\"",
            "tabindex=\"-1\"",
            "href=\"#@error.ControlId\"");
        RepositoryFiles.ContainsAll(alert, "role=\"@LiveRole\"", "aria-live=\"@LiveMode\"");
        RepositoryFiles.ContainsAll(
            conflict,
            "aria-labelledby=\"@HeadingId\"",
            "aria-live=\"polite\"",
            "data-conflict-icon aria-hidden=\"true\"",
            "@Heading");
    }

    [Fact]
    public void All_27_design_records_define_focus_order_and_accessibility_test_ids()
    {
        var directory = RepositoryFiles.PathTo(
            "specs/003-ux-storyboard-accessibility/design/pages");
        var records = Directory.EnumerateFiles(directory, "*.md")
            .Where(path => !Path.GetFileName(path).Equals(
                "identity-boundary.md",
                StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Assert.Equal(27, records.Length);
        foreach (var record in records)
        {
            var content = File.ReadAllText(record);
            RepositoryFiles.ContainsAll(
                content,
                "\"responsiveWidths\"",
                "\"focusOrder\"",
                "-A11Y-",
                "\"readinessState\": \"design-only\"");
        }
    }

    [Fact]
    public void Browser_matrix_is_planned_and_does_not_label_webkit_as_safari()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(
            "tests/StudentRegistration.E2ETests/browser-matrix.json"));
        var targets = document.RootElement.GetProperty("targets")
            .EnumerateArray()
            .ToArray();
        var required = targets.Where(target =>
            target.GetProperty("status").GetString() == "required").ToArray();
        var safari = Assert.Single(targets, target =>
            target.GetProperty("name").GetString() == "Apple Safari");

        Assert.Equal(4, required.Length);
        Assert.All(required, target =>
            Assert.Equal("planned", target.GetProperty("result").GetString()));
        Assert.Contains(required, target =>
            target.GetProperty("label").GetString() == "Playwright WebKit (not Safari)");
        Assert.Equal("deferred", safari.GetProperty("status").GetString());
        Assert.Equal("not-passed", safari.GetProperty("result").GetString());
    }

    [Fact]
    public void Manual_keyboard_and_nvda_template_is_complete_unsigned_and_release_blocking()
    {
        var evidence = RepositoryFiles.Read(ManualEvidencePath);

        RepositoryFiles.ContainsAll(
            evidence,
            "**Artifact version:** 1.0.0",
            "**Status:** NOT EXECUTED",
            "**Release gate:** BLOCKED",
            "Tester | NOT RECORDED",
            "Test date | NOT RECORDED",
            "Operating system | Windows - NOT RECORDED",
            "Assistive technology | NVDA",
            "NVDA version | NOT RECORDED",
            "Route | NOT EXECUTED",
            "Scenario | NOT EXECUTED",
            "Keyboard result | NOT EXECUTED",
            "Screen-reader result | NOT EXECUTED",
            "Overall result | NOT EXECUTED",
            "Defect links | NOT RECORDED",
            "UX sign-off | UNSIGNED",
            "QA sign-off | UNSIGNED",
            "## Machine-readable gate record",
            "tester: NOT RECORDED",
            "date: NOT RECORDED",
            "assistiveTechnology: NVDA version NOT RECORDED on Windows",
            "route: NOT EXECUTED",
            "scenario: NOT EXECUTED",
            "result: NOT EXECUTED",
            "defectLinks: NOT RECORDED",
            "uxQaSignOff: UNSIGNED",
            "Activation condition");
        Assert.DoesNotContain("**Status:** PASS", evidence, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("**Release gate:** PASS", evidence, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(Skip =
        "Activation condition: critical SPEC-003 routes must have executable pages and a pinned Playwright/axe harness before automated WCAG checks can run in Chrome, Edge, Firefox, and WebKit.")]
    public void Automated_browser_accessibility_suite_has_no_serious_failures()
    {
    }

    [Fact(Skip =
        "Activation condition: executable critical routes, Windows, an identified tester, and a recorded NVDA version must exist before manual keyboard and screen-reader journeys can be signed.")]
    public void Manual_keyboard_and_nvda_journeys_are_executed_and_signed()
    {
    }

    private static double ContrastRatio(string firstHex, string secondHex)
    {
        var first = RelativeLuminance(firstHex);
        var second = RelativeLuminance(secondHex);
        return (Math.Max(first, second) + 0.05) / (Math.Min(first, second) + 0.05);
    }

    private static double RelativeLuminance(string hex)
    {
        Assert.Matches("^#[0-9a-fA-F]{6}$", hex);
        var red = byte.Parse(hex.AsSpan(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255d;
        var green = byte.Parse(hex.AsSpan(3, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255d;
        var blue = byte.Parse(hex.AsSpan(5, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255d;
        return 0.2126 * Linearize(red) + 0.7152 * Linearize(green) + 0.0722 * Linearize(blue);
    }

    private static double Linearize(double channel) =>
        channel <= 0.04045
            ? channel / 12.92
            : Math.Pow((channel + 0.055) / 1.055, 2.4);
}
