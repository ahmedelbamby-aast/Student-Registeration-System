using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.DesignSystem;

public sealed class DesignTokenContractTests
{
    private const string TokenContractPath =
        "specs/003-ux-storyboard-accessibility/design/tokens/contract.md";
    private const string BrandRegisterPath = "docs/BRAND_ASSETS.md";
    private const string LogoPath =
        "src/StudentRegistration.Client/wwwroot/brand/aastmt-logo.png";
    private const string OfficialLogoUrl =
        "https://aast.edu/template/en/topbar/img/main-menu/logo-2.png";
    private const string ExpectedLogoDigest =
        "aae2399dfbee27fe1b5a506f08645f4076fb6efacfc36eb0b87996517111a502";
    private const string TokenJsonPath =
        "src/StudentRegistration.Client/wwwroot/design/design-tokens.json";
    private const string TokenCssPath =
        "src/StudentRegistration.Client/wwwroot/css/design-tokens.css";

    [Fact]
    public void Contract_is_versioned_three_layer_neutral_and_blocks_inferred_brand_values()
    {
        var contract = RepositoryFiles.Read(TokenContractPath);

        RepositoryFiles.ContainsAll(
            contract,
            "primitive",
            "semantic",
            "component",
            "immutable",
            "neutral",
            "color",
            "typography",
            "spacing",
            "sizing",
            "border",
            "focus",
            "elevation",
            "motion",
            "breakpoint",
            "z-index",
            "must not be inferred from the AASTMT logo");
    }

    [Fact]
    public void Official_logo_register_and_local_copy_preserve_provenance_integrity_and_aspect_ratio()
    {
        var register = RepositoryFiles.Read(BrandRegisterPath);
        RepositoryFiles.ContainsAll(
            register,
            OfficialLogoUrl,
            "2026-07-13",
            ExpectedLogoDigest,
            "300 x 110",
            "30:11",
            LogoPath,
            "Arab Academy for Science, Technology and Maritime Transport",
            "no recoloring, cropping, stretching, or distortion");

        var bytes = File.ReadAllBytes(RepositoryFiles.PathTo(LogoPath));
        Assert.Equal(ExpectedLogoDigest, Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());
        Assert.True(bytes.AsSpan(1, 3).SequenceEqual("PNG"u8));

        var width = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(16, 4));
        var height = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(20, 4));
        Assert.Equal(300, width);
        Assert.Equal(110, height);
        Assert.Equal(width * 11, height * 30);
    }

    [Fact]
    public void Runtime_entry_uses_no_official_asset_hotlink()
    {
        var index = RepositoryFiles.Read(
            "src/StudentRegistration.Client/wwwroot/index.html");

        Assert.DoesNotContain(OfficialLogoUrl, index, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("src=\"https://aast.edu", index, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Canonical_token_set_is_approved_neutral_versioned_and_three_layer()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(TokenJsonPath));
        var root = document.RootElement;
        var colors = root.GetProperty("tokens").GetProperty("color");
        var colorNames = colors.EnumerateObject().Select(token => token.Name).ToArray();

        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("1.0.0", root.GetProperty("version").GetString());
        Assert.Equal("neutral", root.GetProperty("scope").GetString());
        Assert.False(root.GetProperty("brandValuesDerivedFromLogo").GetBoolean());
        Assert.Equal(
            "Ahmed ELbamby",
            root.GetProperty("approval").GetProperty("approvedBy").GetString());
        Assert.Contains(colorNames, name => name.StartsWith("primitive-", StringComparison.Ordinal));
        Assert.Contains(colorNames, name => name.StartsWith("semantic-", StringComparison.Ordinal));
        Assert.Contains(colorNames, name => name.StartsWith("component-", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("semantic-text-primary", "semantic-surface-default", 4.5)]
    [InlineData("semantic-text-secondary", "semantic-surface-default", 4.5)]
    [InlineData("semantic-link", "semantic-surface-default", 4.5)]
    [InlineData("semantic-information-text", "semantic-information-surface", 4.5)]
    [InlineData("semantic-success-text", "semantic-success-surface", 4.5)]
    [InlineData("semantic-warning-text", "semantic-warning-surface", 4.5)]
    [InlineData("semantic-danger-text", "semantic-danger-surface", 4.5)]
    [InlineData("semantic-focus-ring", "semantic-surface-default", 3.0)]
    public void Semantic_color_pairs_meet_required_contrast(
        string foregroundName,
        string backgroundName,
        double minimumRatio)
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(TokenJsonPath));
        var colors = document.RootElement.GetProperty("tokens").GetProperty("color");
        var foreground = colors.GetProperty(foregroundName).GetString()!;
        var background = colors.GetProperty(backgroundName).GetString()!;

        Assert.True(
            ContrastRatio(foreground, background) >= minimumRatio,
            $"{foregroundName} on {backgroundName} does not meet {minimumRatio}:1.");
    }

    [Fact]
    public void Css_projection_contains_every_json_token_and_accessibility_media_rules()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(TokenJsonPath));
        var css = RepositoryFiles.Read(TokenCssPath);
        var tokens = document.RootElement.GetProperty("tokens");

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

        RepositoryFiles.ContainsAll(
            css,
            "prefers-reduced-motion: reduce",
            "forced-colors: active",
            "--srs-focus-outline-width",
            "--srs-sizing-interactive-minimum");
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
