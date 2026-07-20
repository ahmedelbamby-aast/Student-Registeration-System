using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Components;

public sealed class ComponentCatalogTests
{
    private const string ManifestPath = ".specify/component-manifest.json";
    private const string CataloguePath =
        "specs/003-ux-storyboard-accessibility/design/components/catalogue.md";

    [Fact]
    public void Catalogue_covers_all_and_only_the_28_canonical_components_and_source_paths()
    {
        using var manifest = JsonDocument.Parse(RepositoryFiles.Read(ManifestPath));
        var catalogue = RepositoryFiles.Read(CataloguePath);
        var components = manifest.RootElement.GetProperty("components").EnumerateArray().ToArray();

        Assert.Equal(28, components.Length);
        foreach (var component in components)
        {
            Assert.Contains(component.GetProperty("name").GetString()!, catalogue, StringComparison.Ordinal);
            Assert.Contains(component.GetProperty("path").GetString()!, catalogue, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Catalogue_governs_variants_states_keyboard_names_and_token_only_styling()
    {
        var catalogue = RepositoryFiles.Read(CataloguePath);

        RepositoryFiles.ContainsAll(
            catalogue,
            "default",
            "hover",
            "active",
            "focus-visible",
            "disabled",
            "loading",
            "error",
            "keyboard semantics",
            "accessible name",
            "token-only styling",
            "44 CSS px");
    }
}
