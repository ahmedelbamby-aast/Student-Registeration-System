using System.Net;
using System.Text.RegularExpressions;
using StudentRegistration.Client.Localization;
using StudentRegistration.Client.UX;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec003;

public sealed class NFR_10EvidenceTests
{
    private static readonly Regex LocalizedKey = new(
        "LocalizedUiText\\.(?:Get|Format)\\(\"(?<key>(?:\\\\.|[^\"\\\\])*)\"",
        RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);

    private static readonly Regex ResourceKey = new(
        "<data name=\"(?<key>[^\"]+)\"",
        RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);

    private static readonly Regex StringLiteral = new(
        "(?<![$@])\"(?<value>(?:\\\\.|[^\"\\\\])*)\"",
        RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1));

    [Fact]
    public void English_ui_text_is_externalized_registered_and_resource_complete()
    {
        var program = RepositoryFiles.Read("src/StudentRegistration.Client/Program.cs");
        RepositoryFiles.ContainsAll(program, "AddSingleton<IUiTextProvider, ResourceUiTextProvider>()");

        var resource = RepositoryFiles.Read("src/StudentRegistration.Client/Localization/UiText.resx");
        var resourceKeys = ResourceKey.Matches(resource)
            .Select(match => WebUtility.HtmlDecode(match.Groups["key"].Value))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var referencedKeys = ClientSourceFiles()
            .SelectMany(path => LocalizedKey.Matches(File.ReadAllText(path)))
            .Select(match => Regex.Unescape(match.Groups["key"].Value))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.True(referencedKeys.Count >= 1_000, $"Expected a complete UI resource surface; found {referencedKeys.Count} referenced keys.");
        Assert.DoesNotContain(referencedKeys, key => !resourceKeys.Contains(key));

        var provider = new ResourceUiTextProvider();
        foreach (var key in Enum.GetValues<RouteUiState>().Select(state => $"Ui.State.{state}"))
        {
            Assert.NotEqual(key, provider.Get(key));
        }
    }

    [Fact]
    public void User_facing_control_code_has_no_unexternalized_English_literals()
    {
        var residuals = new List<string>();
        foreach (var path in ClientSourceFiles().Where(IsRazorControlSource))
        {
            var lines = File.ReadAllLines(path);
            var start = path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase)
                ? Array.FindIndex(lines, line => line.Contains("@code", StringComparison.Ordinal))
                : 0;
            if (start < 0)
            {
                continue;
            }

            var inRawMachineFixture = false;
            for (var index = start; index < lines.Length; index++)
            {
                var line = lines[index];
                if (line.Contains("\"\"\"", StringComparison.Ordinal))
                {
                    inRawMachineFixture = !inRawMachineFixture;
                    continue;
                }
                if (inRawMachineFixture || IsNonUserSourceLine(line))
                {
                    continue;
                }

                foreach (Match match in StringLiteral.Matches(line))
                {
                    var value = Regex.Unescape(match.Groups["value"].Value);
                    if (LooksUserFacing(value) && !AllowedMachineLiteral(value))
                    {
                        residuals.Add($"{Relative(path)}:{index + 1}: {value}");
                    }
                }
            }
        }

        Assert.True(residuals.Count == 0,
            "Unexternalized user-facing control-code literals:\n" + string.Join('\n', residuals));
    }

    [Fact]
    public void Invariant_culture_is_restricted_to_explicit_machine_protocol_allowlist()
    {
        var rules = new[]
        {
            new MachineCultureRule("Components/Scheduling/ConflictPanel.razor", "FormatTimeToken", "semantic HTML time token"),
            new MachineCultureRule("Features/Registration/EligibilityPresentation.razor", "SourceAccessedOn.ToString", "HTML date attribute"),
            new MachineCultureRule("Features/Registration/EligibilityPresentation.razor", "EffectiveFromUtc.ToString", "HTML instant attribute"),
            new MachineCultureRule("Features/Registration/EligibilityPresentation.razor", "effectiveTo.ToString", "HTML instant attribute"),
            new MachineCultureRule("Features/Registration/RegistrationApiClient.cs", "(\"page\"", "HTTP query integer"),
            new MachineCultureRule("Features/Registration/RegistrationApiClient.cs", "(\"pageSize\"", "HTTP query integer"),
            new MachineCultureRule("Pages/CatalogueAdministrationPage.razor", "_importAccessedOn", "HTML date input"),
            new MachineCultureRule("Pages/CatalogueAdministrationPage.razor", "_normalMaxCredits", "API decimal input"),
            new MachineCultureRule("Pages/CatalogueAdministrationPage.razor", "_probationMaxCredits", "API decimal input"),
            new MachineCultureRule("Pages/CatalogueAdministrationPage.razor", "value.ToString(CultureInfo.InvariantCulture)", "JSON numeric draft"),
            new MachineCultureRule("Pages/RoleGatewayPage.razor", "ToString(\"O\"", "HTML instant attribute"),
            new MachineCultureRule("Pages/StaffAvailabilityPage.razor", "editor.StartLocal", "HTML time input"),
            new MachineCultureRule("Pages/StaffAvailabilityPage.razor", "editor.EndLocal", "HTML time input"),
            new MachineCultureRule("Pages/StudentAdministrationPage.razor", "CurrentGpa.ToString", "HTML number input"),
            new MachineCultureRule("Pages/StudentAdministrationPage.razor", "_correction.Value", "API decimal input"),
            new MachineCultureRule("Pages/StudentDashboardPage.razor", "ToString(\"O\"", "HTML instant attribute"),
            new MachineCultureRule("Pages/TermAdministrationPage.razor", "_editor.TeachingStartsOn", "HTML date input"),
            new MachineCultureRule("Pages/TermAdministrationPage.razor", "_editor.TeachingEndsOn", "HTML date input"),
            new MachineCultureRule("Pages/TermAdministrationPage.razor", "window.OpensAtUtc.ToString", "deterministic concurrency hash"),
            new MachineCultureRule("Pages/TermAdministrationPage.razor", "window.ClosesAtUtc.ToString", "deterministic concurrency hash"),
            new MachineCultureRule("Pages/TermAdministrationPage.razor", "yyyy-MM-ddTHH:mm:ss.FFFFFFF", "HTML datetime-local parser"),
            new MachineCultureRule("Pages/TermAdministrationPage.razor", "DateValue", "HTML date value"),
            new MachineCultureRule("Pages/TermAdministrationPage.razor", "yyyy-MM-ddTHH:mm", "HTML datetime-local value")
        };

        var observed = new List<string>();
        foreach (var path in ClientSourceFiles())
        {
            var lines = File.ReadAllLines(path);
            for (var index = 0; index < lines.Length; index++)
            {
                if (!lines[index].Contains("InvariantCulture", StringComparison.Ordinal))
                {
                    continue;
                }
                var context = string.Join(' ', lines.Skip(Math.Max(0, index - 2)).Take(5)).Trim();
                var relative = Relative(path);
                Assert.True(rules.Any(rule => rule.Path == relative && context.Contains(rule.Marker, StringComparison.Ordinal)),
                    $"InvariantCulture is not allowlisted: {relative}:{index + 1}: {lines[index].Trim()}");
                observed.Add(relative + "|" + context);
            }
        }

        Assert.All(rules, rule => Assert.Contains(observed,
            item => item.StartsWith(rule.Path + "|", StringComparison.Ordinal)
                && item.Contains(rule.Marker, StringComparison.Ordinal)));
    }

    [Fact]
    public void Runtime_layout_is_direction_safe_and_shared_formats_use_current_culture()
    {
        var css = string.Join('\n', Directory.EnumerateFiles(
                RepositoryFiles.PathTo("src/StudentRegistration.Client"), "*.css", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .Select(File.ReadAllText));
        RepositoryFiles.ContainsAll(css, "inline-size", "block-size", "margin-inline", "padding-inline", "inset-inline", "border-inline");

        var localization = RepositoryFiles.Read("src/StudentRegistration.Client/Localization/UiText.cs");
        RepositoryFiles.ContainsAll(localization, "CurrentUICulture", "CurrentCulture");
    }

    [Fact]
    public void Resource_provider_fails_safe_to_the_stable_key_for_unknown_text()
    {
        IUiTextProvider provider = new ResourceUiTextProvider();
        Assert.Equal("Ui.Unknown.FutureKey", provider.Get("Ui.Unknown.FutureKey"));
        Assert.Throws<ArgumentException>(() => provider.Get(" "));
    }

    private static IEnumerable<string> ClientSourceFiles() =>
        Directory.EnumerateFiles(RepositoryFiles.PathTo("src/StudentRegistration.Client"), "*.*", SearchOption.AllDirectories)
            .Where(path => path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                && !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));

    private static bool IsRazorControlSource(string path) =>
        path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase)
        || path.EndsWith(".razor.cs", StringComparison.OrdinalIgnoreCase);

    private static bool IsNonUserSourceLine(string line)
    {
        var trimmed = line.TrimStart();
        return trimmed.StartsWith("<", StringComparison.Ordinal)
            || trimmed.StartsWith("//", StringComparison.Ordinal)
            || line.Contains("\"@(", StringComparison.Ordinal)
            || line.Contains("LocalizedUiText.", StringComparison.Ordinal)
            || line.Contains("throw new ", StringComparison.Ordinal)
            || line.Contains("ToString(", StringComparison.Ordinal)
            || line.Contains("TryParse", StringComparison.Ordinal)
            || line.Contains(".Parse(", StringComparison.Ordinal)
            || line.Contains(".Append(", StringComparison.Ordinal)
            || line.Contains("nameof(", StringComparison.Ordinal)
            || line.Contains("[Parameter", StringComparison.Ordinal)
            || line.Contains("srs-", StringComparison.Ordinal);
    }

    private static bool LooksUserFacing(string value) =>
        value.Any(char.IsLetter)
        && (value.Any(char.IsWhiteSpace) || Regex.IsMatch(value, "^[A-Z][a-z]+$", RegexOptions.CultureInvariant));

    private static bool AllowedMachineLiteral(string value) => value is
        "Escape" or "Admin" or "Student" or "Lecturer" or "TeachingAssistant"
        or "Available" or "Unavailable" or "Draft" or "Published" or "Open" or "Closed" or "Unknown"
        or "Credits" or "Active" or "Level" or "Required" or "Cohort"
        or "{DescriptionId} {ErrorId}" or "registration-window-{index + 1}-heading";

    private static string Relative(string path) => Path.GetRelativePath(
        RepositoryFiles.PathTo("src/StudentRegistration.Client"), path).Replace('\\', '/');

    private sealed record MachineCultureRule(string Path, string Marker, string Rationale);
}
