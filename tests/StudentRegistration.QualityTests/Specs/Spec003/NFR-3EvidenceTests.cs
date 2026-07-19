using System.Globalization;
using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec003;

public sealed class NFR_3EvidenceTests
{
    [Theory]
    [InlineData("semantic-text-primary", "semantic-surface-default", 4.5)]
    [InlineData("semantic-text-secondary", "semantic-surface-default", 4.5)]
    [InlineData("semantic-link", "semantic-surface-default", 4.5)]
    [InlineData("semantic-information-text", "semantic-information-surface", 4.5)]
    [InlineData("semantic-success-text", "semantic-success-surface", 4.5)]
    [InlineData("semantic-warning-text", "semantic-warning-surface", 4.5)]
    [InlineData("semantic-danger-text", "semantic-danger-surface", 4.5)]
    [InlineData("semantic-focus-ring", "semantic-surface-default", 3.0)]
    public void Approved_runtime_color_pairs_meet_wcag_ratios(
        string foregroundName,
        string backgroundName,
        double minimum)
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(
            "src/StudentRegistration.Client/wwwroot/design/design-tokens.json"));
        var colors = document.RootElement.GetProperty("tokens").GetProperty("color");
        var actual = Contrast(
            colors.GetProperty(foregroundName).GetString()!,
            colors.GetProperty(backgroundName).GetString()!);
        Assert.True(actual >= minimum, $"{foregroundName}/{backgroundName}: {actual:F2}:1 < {minimum}:1");
    }

    private static double Contrast(string first, string second)
    {
        var a = Luminance(first);
        var b = Luminance(second);
        return (Math.Max(a, b) + .05) / (Math.Min(a, b) + .05);
    }

    private static double Luminance(string hex)
    {
        var channels = new[] { 1, 3, 5 }
            .Select(offset => byte.Parse(hex.AsSpan(offset, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255d)
            .Select(value => value <= .04045 ? value / 12.92 : Math.Pow((value + .055) / 1.055, 2.4))
            .ToArray();
        return .2126 * channels[0] + .7152 * channels[1] + .0722 * channels[2];
    }
}
