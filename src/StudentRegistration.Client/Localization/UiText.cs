using System.Globalization;
using System.Resources;

namespace StudentRegistration.Client.Localization;

/// <summary>
/// Resolves the externalized English UI catalogue using the active UI culture.
/// English source text may be used as a resource key, which keeps call sites
/// readable while allowing a culture-specific resource to replace the value.
/// </summary>
public static class LocalizedUiText
{
    private static readonly ResourceManager Resources = CreateResourceManager();

    private static ResourceManager CreateResourceManager()
    {
        var resources = new ResourceManager(
            "StudentRegistration.Client.Localization.LocalizedUiText",
            typeof(LocalizedUiText).Assembly)
        {
            IgnoreCase = true
        };
        return resources;
    }

    public static string Get(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("A stable UI text key is required.", nameof(key));
        }

        var normalized = key.Trim();
        return Resources.GetString(normalized, CultureInfo.CurrentUICulture) ?? normalized;
    }

    public static string Format(string key, params object?[] arguments) =>
        string.Format(CultureInfo.CurrentCulture, Get(key), arguments);

    public static string Format(FormattableString value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return string.Format(
            CultureInfo.CurrentCulture,
            Get(value.Format),
            value.GetArguments());
    }
}
