using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using Microsoft.Playwright;
using StudentRegistration.AccessibilityTests.Infrastructure;
using StudentRegistration.TestSupport;
using Xunit.Abstractions;
using Xunit.Sdk;

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
    public void Browser_matrix_is_executed_and_does_not_label_webkit_as_safari()
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
        {
            Assert.Equal("passed", target.GetProperty("result").GetString());
            Assert.Equal("executed", target.GetProperty("preflightStatus").GetString());
        });
        Assert.Contains(required, target =>
            target.GetProperty("label").GetString() == "Playwright WebKit (not Safari)");
        Assert.Equal("deferred", safari.GetProperty("status").GetString());
        Assert.Equal("not-passed", safari.GetProperty("result").GetString());
    }

    [Fact]
    public void Manual_keyboard_and_nvda_record_has_explicit_non_production_demo_waiver()
    {
        var evidence = RepositoryFiles.Read(ManualEvidencePath);

        RepositoryFiles.ContainsAll(
            evidence,
            "**Artifact version:** 1.1.0",
            "**Status:** WAIVED-DEMO",
            "**Release gate:** WAIVED-DEMO",
            "**Production authority:** Not granted",
            "**Approved by:** Ahmed ELbamby",
            "**Approval date:** 2026-07-20",
            "Tester | NOT PERFORMED - DEMO WAIVER",
            "Assistive technology | NVDA - MANUAL EXECUTION NOT REQUIRED FOR DEMO",
            "Overall result | WAIVED-DEMO",
            "UX sign-off | NOT REQUIRED FOR DEMO - AHMED WAIVER",
            "QA sign-off | NOT REQUIRED FOR DEMO - AHMED WAIVER",
            "## Machine-readable gate record",
            "tester: NOT PERFORMED - DEMO WAIVER",
            "date: 2026-07-20",
            "assistiveTechnology: NVDA manual execution not required for demo",
            "route: nine critical routes - manual execution waived",
            "scenario: manual keyboard and NVDA journeys waived for demo",
            "result: WAIVED-DEMO",
            "approvedBy: Ahmed ELbamby",
            "approvedOn: 2026-07-20",
            "productionAuthorized: false",
            "Future production activation condition");
        Assert.DoesNotContain("**Status:** PASS", evidence, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("**Release gate:** PASS", evidence, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("**Status:** NOT EXECUTED", evidence, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Automated_browser_accessibility_suite_has_no_serious_failures()
    {
        using var evidence = JsonDocument.Parse(RepositoryFiles.Read(
            "docs/release-evidence/SPEC-018-NFR-8-browser-matrix.json"));
        var root = evidence.RootElement;

        Assert.Equal("PASS", root.GetProperty("result").GetString());
        Assert.Equal(36, root.GetProperty("browserRouteCombinations").GetInt32());
        Assert.Equal(36, root.GetProperty("passedCombinations").GetInt32());
        Assert.Equal(0, root.GetProperty("failedCombinations").GetInt32());
        Assert.Equal(4, root.GetProperty("browsers").GetArrayLength());
        Assert.Equal(9, root.GetProperty("routes").GetArrayLength());
        Assert.False(root.GetProperty("manualSignOffSatisfied").GetBoolean());
    }

    [Fact]
    public void Automated_nvda_windows_integration_probe_is_objective_and_not_manual_signoff()
    {
        using var evidence = JsonDocument.Parse(RepositoryFiles.Read(
            "docs/release-evidence/SPEC-018-NFR-8-nvda-probe.json"));
        var root = evidence.RootElement;

        Assert.Equal("PASS", root.GetProperty("result").GetString());
        Assert.Equal("2026.1.1", root.GetProperty("nvdaVersion").GetString());
        Assert.Equal(0, root.GetProperty("keyboardProbeExitCode").GetInt32());
        Assert.True(root.GetProperty("speechEventCount").GetInt32() > 0);
        Assert.True(root.GetProperty("focusEventCount").GetInt32() > 0);
        Assert.Equal("not-performed", root.GetProperty("manualTester").GetString());
        Assert.Equal("unsigned", root.GetProperty("uxQaSignOff").GetString());
        Assert.Equal("BLOCKED", root.GetProperty("releaseGate").GetString());
    }

    [Fact(Skip =
        "Ahmed-approved non-production demo waiver: manual keyboard and NVDA execution is not required; production remains unauthorized.")]
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

[Collection(AxeAccessibilityCollection.CollectionName)]
public sealed class Nfr8NvdaProbeTests(
    AxeAccessibilityFixture fixture,
    ITestOutputHelper output)
{
    [Fact]
    [Trait("Category", "NvdaIntegrationProbe")]
    public async Task Staff_login_exposes_an_objective_keyboard_focus_and_validation_journey_to_nvda()
    {
        if (!IsEnabled("SRS_NVDA_PROBE"))
        {
            throw SkipException.ForSkip(
                "Set SRS_NVDA_PROBE=true and use Run-NvdaProbe.ps1 to execute the real NVDA/Windows integration probe.");
        }

        Assert.True(
            Process.GetProcessesByName("nvda").Any(process => !process.HasExited),
            "The official NVDA process is not running; an ordinary browser test cannot claim this probe.");
        output.WriteLine($"browserTarget={fixture.BrowserTarget}");
        output.WriteLine($"nvdaExecutable={Environment.GetEnvironmentVariable("SRS_NVDA_EXECUTABLE")}");
        output.WriteLine($"nvdaVersion={Environment.GetEnvironmentVariable("SRS_NVDA_VERSION")}");
        output.WriteLine($"nvdaLog={Environment.GetEnvironmentVariable("SRS_NVDA_LOG_PATH")}");

        await using var context = await fixture.OpenContextAsync(1280, 900);
        var page = await context.NewPageAsync();
        await page.GotoAsync("/staff/login", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });
        await page.GetByRole(
                AriaRole.Heading,
                new PageGetByRoleOptions { Name = "Staff login", Exact = true })
            .WaitForAsync();
        await Task.Delay(1_000);

        var initialFocus = await ReadFocusAsync(page);
        output.WriteLine($"step=initial; focus={initialFocus}");
        if (!initialFocus.Contains("Skip to main content", StringComparison.OrdinalIgnoreCase))
        {
            await page.Keyboard.PressAsync("Tab");
            var firstTabFocus = await ReadFocusAsync(page);
            output.WriteLine($"step=first-tab; focus={firstTabFocus}");
            for (var attempt = 0;
                 attempt < 5 && !firstTabFocus.Contains(
                     "Skip to main content",
                     StringComparison.OrdinalIgnoreCase);
                 attempt++)
            {
                await page.Keyboard.PressAsync("Shift+Tab");
                firstTabFocus = await ReadFocusAsync(page);
                output.WriteLine($"step=reverse-tab-{attempt + 1}; focus={firstTabFocus}");
            }
        }

        await AssertAndLogFocusAsync(page, "skip-link", "Skip to main content");
        await page.Keyboard.PressAsync("Enter");
        await AssertAndLogFocusAsync(page, "main", "Staff login");

        await page.Keyboard.PressAsync("Tab");
        await AssertAndLogFocusAsync(page, "username", "Staff username");
        await page.Keyboard.TypeAsync("nvda-probe-user");
        await page.Keyboard.PressAsync("Tab");
        await AssertAndLogFocusAsync(page, "password", "Password");
        await page.Keyboard.TypeAsync(new string('x', 16));

        var signInFocused = false;
        for (var attempt = 0; attempt < 4 && !signInFocused; attempt++)
        {
            await page.Keyboard.PressAsync("Tab");
            var focus = await ReadFocusAsync(page);
            output.WriteLine($"step=tab-{attempt + 1}; focus={focus}");
            signInFocused = focus.Contains("Sign in", StringComparison.OrdinalIgnoreCase);
            await Task.Delay(500);
        }

        Assert.True(signInFocused, "Keyboard focus did not reach the Sign in command.");
        await page.Keyboard.PressAsync("Enter");
        var alert = page.GetByRole(AriaRole.Alert);
        await alert.WaitForAsync();
        var alertText = await alert.InnerTextAsync();
        output.WriteLine($"step=validation-alert; text={OneLine(alertText)}");
        Assert.Contains("Staff sign-in unavailable", alertText, StringComparison.Ordinal);
        await AxeAccessibilityFixture.AssertNoSeriousAxeViolationsAsync(page);
        await Task.Delay(1_500);
    }

    private async Task AssertAndLogFocusAsync(IPage page, string step, string expectedText)
    {
        var focus = await ReadFocusAsync(page);
        output.WriteLine($"step={step}; focus={focus}");
        Assert.Contains(expectedText, focus, StringComparison.OrdinalIgnoreCase);
        await Task.Delay(750);
    }

    private static Task<string> ReadFocusAsync(IPage page) =>
        page.EvaluateAsync<string>(
            """
            () => {
                const element = document.activeElement;
                if (!element) return 'none';
                const label = element.labels?.[0]?.innerText
                    || element.getAttribute('aria-label')
                    || element.innerText
                    || element.getAttribute('name')
                    || element.id;
                return `${element.tagName.toLowerCase()}#${element.id || '-'}:${String(label || '').trim()}`;
            }
            """);

    private static string OneLine(string value) =>
        string.Join(" ", value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static bool IsEnabled(string variable) =>
        string.Equals(Environment.GetEnvironmentVariable(variable), "true", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(Environment.GetEnvironmentVariable(variable), "1", StringComparison.Ordinal);
}
