using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec003.EdgeCases;

public sealed class EC_9Tests
{
    [Theory]
    [InlineData(
        "src/StudentRegistration.Client/Pages/StudentLoginPage.razor",
        "University ID",
        "autocomplete=\"username\"",
        "Password",
        "autocomplete=\"current-password\"")]
    [InlineData(
        "src/StudentRegistration.Client/Pages/StudentActivationPage.razor",
        "University ID",
        "autocomplete=\"username\"",
        "New password",
        "autocomplete=\"new-password\"")]
    [InlineData(
        "src/StudentRegistration.Client/Pages/StaffLoginPage.razor",
        "Staff username",
        "autocomplete=\"username\"",
        "Password",
        "autocomplete=\"current-password\"")]
    [InlineData(
        "src/StudentRegistration.Client/Pages/AccountRecoveryPage.razor",
        "University ID or Staff username",
        "autocomplete=\"username\"",
        "New password",
        "autocomplete=\"new-password\"")]
    public void Identity_autofill_keeps_visible_labels_and_reviewable_controls_without_secret_storage(
        string pagePath,
        string identityLabel,
        string identityAutocomplete,
        string secretLabel,
        string secretAutocomplete)
    {
        var page = RepositoryFiles.Read(pagePath);
        var field = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Components/Forms/FormField.razor");
        var resourceCatalog = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Localization/UiText.resx");

        RepositoryFiles.ContainsAll(
            page,
            identityAutocomplete,
            secretAutocomplete,
            "AllowSecretReveal=\"true\"");
        Assert.Contains("Label=\"@(LocalizedUiText.Get(", page, StringComparison.Ordinal);
        Assert.Contains($"LocalizedUiText.Get(\"{identityLabel}\")", page, StringComparison.Ordinal);
        Assert.Contains($"LocalizedUiText.Get(\"{secretLabel}\")", page, StringComparison.Ordinal);
        Assert.Contains($"<data name=\"{identityLabel}\"", resourceCatalog, StringComparison.Ordinal);
        Assert.Contains($"<data name=\"{secretLabel}\"", resourceCatalog, StringComparison.Ordinal);
        RepositoryFiles.ContainsAll(
            field,
            "<label class=\"srs-form-field__label\" for=\"@InputId\">@Label</label>",
            "value=\"@Value\"",
            "aria-controls=\"@InputId\"",
            "aria-pressed=",
            "LocalizedUiText.Format(\"Show {0}\", Label)",
            "LocalizedUiText.Format(\"Hide {0}\", Label)");
        Assert.DoesNotContain("placeholder-only", page, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("localStorage", page, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("sessionStorage", page, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Console.", page, StringComparison.Ordinal);
    }
}
