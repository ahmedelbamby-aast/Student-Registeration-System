using StudentRegistration.Client.UX;

namespace StudentRegistration.Client.Localization;

public sealed class ResourceUiTextProvider : IUiTextProvider
{
    public string Get(string key) => LocalizedUiText.Get(key);
}
