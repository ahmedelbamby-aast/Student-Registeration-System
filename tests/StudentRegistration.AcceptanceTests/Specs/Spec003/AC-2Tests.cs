using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec003;

public sealed class AC_2Tests
{
    private const string LogoPath =
        "src/StudentRegistration.Client/wwwroot/brand/aastmt-logo.png";
    private const string ExpectedLogoDigest =
        "aae2399dfbee27fe1b5a506f08645f4076fb6efacfc36eb0b87996517111a502";
    private const string OfficialLogoUrl =
        "https://aast.edu/template/en/topbar/img/main-menu/logo-2.png";

    [Fact]
    public void Versioned_design_system_review_has_complete_measurable_shared_evidence()
    {
        // Given the shared design system is reviewed before any route is visually implemented.
        using var tokensDocument = JsonDocument.Parse(
            RepositoryFiles.Read(
                "src/StudentRegistration.Client/wwwroot/design/design-tokens.json"));
        var tokenRoot = tokensDocument.RootElement;
        var tokens = tokenRoot.GetProperty("tokens");

        Assert.Equal("1.0.0", tokenRoot.GetProperty("version").GetString());
        Assert.Equal("neutral", tokenRoot.GetProperty("scope").GetString());
        Assert.False(tokenRoot.GetProperty("brandValuesDerivedFromLogo").GetBoolean());
        Assert.Equal(
            "approved",
            tokenRoot.GetProperty("approval").GetProperty("status").GetString());
        Assert.Equal(
            "Ahmed ELbamby",
            tokenRoot.GetProperty("approval").GetProperty("approvedBy").GetString());

        var categoryNames = tokens.EnumerateObject()
            .Select(category => category.Name)
            .ToHashSet(StringComparer.Ordinal);
        Assert.Equal(
            new HashSet<string>(
                [
                    "color",
                    "typography",
                    "spacing",
                    "sizing",
                    "border",
                    "focus",
                    "elevation",
                    "motion",
                    "breakpoint",
                    "zIndex"
                ],
                StringComparer.Ordinal),
            categoryNames);

        using var componentManifest = JsonDocument.Parse(
            RepositoryFiles.Read(".specify/component-manifest.json"));
        var components = componentManifest.RootElement.GetProperty("components")
            .EnumerateArray()
            .ToArray();
        Assert.Equal(20, components.Length);
        Assert.Equal(
            20,
            components.Select(component => component.GetProperty("name").GetString())
                .Distinct(StringComparer.Ordinal)
                .Count());

        // When token, component-state, logo, contrast, and pointer-size evidence is checked.
        var catalogue = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/components/catalogue.md");
        Assert.All(
            components,
            component => RepositoryFiles.ContainsAll(
                catalogue,
                component.GetProperty("name").GetString()!,
                component.GetProperty("path").GetString()!));
        RepositoryFiles.ContainsAll(
            catalogue,
            "default",
            "hover",
            "active",
            "focus-visible",
            "disabled",
            "loading",
            "error",
            "token-only styling",
            "44 CSS px");

        var colors = tokens.GetProperty("color");
        AssertContrast(colors, "semantic-text-primary", "semantic-surface-default", 4.5);
        AssertContrast(colors, "semantic-text-secondary", "semantic-surface-default", 4.5);
        AssertContrast(colors, "semantic-link", "semantic-surface-default", 4.5);
        AssertContrast(colors, "semantic-danger-text", "semantic-danger-surface", 4.5);
        AssertContrast(colors, "semantic-focus-ring", "semantic-surface-default", 3.0);

        var targetValue = tokens.GetProperty("sizing")
            .GetProperty("interactive-minimum")
            .GetString()!;
        Assert.EndsWith("rem", targetValue, StringComparison.Ordinal);
        var targetRem = double.Parse(
            targetValue[..^3],
            NumberStyles.Number,
            CultureInfo.InvariantCulture);
        Assert.True(targetRem * 16 >= 44, $"Pointer target was {targetRem * 16:F1} CSS px.");

        var brandRegister = RepositoryFiles.Read("docs/BRAND_ASSETS.md");
        RepositoryFiles.ContainsAll(
            brandRegister,
            OfficialLogoUrl,
            ExpectedLogoDigest,
            "300 x 110",
            "30:11",
            LogoPath,
            "Arab Academy for Science, Technology and Maritime Transport",
            "no recoloring, cropping, stretching, or distortion");

        var logoBytes = File.ReadAllBytes(RepositoryFiles.PathTo(LogoPath));
        Assert.Equal(
            ExpectedLogoDigest,
            Convert.ToHexString(SHA256.HashData(logoBytes)).ToLowerInvariant());
        Assert.True(logoBytes.AsSpan(1, 3).SequenceEqual("PNG"u8));
        var width = BinaryPrimitives.ReadInt32BigEndian(logoBytes.AsSpan(16, 4));
        var height = BinaryPrimitives.ReadInt32BigEndian(logoBytes.AsSpan(20, 4));
        Assert.Equal(300, width);
        Assert.Equal(110, height);
        Assert.Equal(width * 11, height * 30);

        // Then the local unchanged asset and token projection are ready as shared design evidence.
        var appShell = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Components/Layout/AppShell.razor");
        var appShellStyles = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Components/Layout/AppShell.razor.css");
        RepositoryFiles.ContainsAll(
            appShell,
            "src=\"@BrandLogoSource\"",
            "alt=\"@BrandLogoAccessibleName\"");
        RepositoryFiles.ContainsAll(
            appShellStyles,
            ".srs-app-shell__logo",
            "height: auto");

        var css = RepositoryFiles.Read(
            "src/StudentRegistration.Client/wwwroot/css/design-tokens.css");
        foreach (var category in tokens.EnumerateObject())
        {
            foreach (var token in category.Value.EnumerateObject())
            {
                Assert.Contains(
                    $"--srs-{category.Name}-{token.Name}:",
                    css,
                    StringComparison.Ordinal);
            }
        }
    }

    private static void AssertContrast(
        JsonElement colors,
        string foregroundName,
        string backgroundName,
        double minimumRatio)
    {
        var ratio = ContrastRatio(
            colors.GetProperty(foregroundName).GetString()!,
            colors.GetProperty(backgroundName).GetString()!);
        Assert.True(
            ratio >= minimumRatio,
            $"{foregroundName} on {backgroundName} was {ratio:F2}:1, below {minimumRatio}:1.");
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
        var red = byte.Parse(
            hex.AsSpan(1, 2),
            NumberStyles.HexNumber,
            CultureInfo.InvariantCulture) / 255d;
        var green = byte.Parse(
            hex.AsSpan(3, 2),
            NumberStyles.HexNumber,
            CultureInfo.InvariantCulture) / 255d;
        var blue = byte.Parse(
            hex.AsSpan(5, 2),
            NumberStyles.HexNumber,
            CultureInfo.InvariantCulture) / 255d;
        return 0.2126 * Linearize(red) + 0.7152 * Linearize(green) + 0.0722 * Linearize(blue);
    }

    private static double Linearize(double channel) =>
        channel <= 0.04045
            ? channel / 12.92
            : Math.Pow((channel + 0.055) / 1.055, 2.4);
}
