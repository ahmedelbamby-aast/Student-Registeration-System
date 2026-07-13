using StudentRegistration.Client.Features.Frontend.Models;
using StudentRegistration.Client.UX;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec003.EdgeCases;

public sealed class EC_6Tests
{
    [Fact]
    public void Unknown_reason_uses_safe_generic_copy_reference_and_recovery_actions()
    {
        const string unknownCode = "FUTURE_POLICY_REASON";
        const string referenceId = "REF-UNKNOWN-001";
        var actions = new[]
        {
            new UiStatusAction("retry", "Try again", "retry"),
            new UiStatusAction("support", "Contact support", "support")
        };
        var mapper = new UiStateMapper(new ResourceKeyTextProvider());

        var result = mapper.Map(new UiStateInput(
            RouteUiState.Success,
            serverAccepted: false,
            unknownCode,
            referenceId,
            actions));

        Assert.Equal(RouteUiState.ServiceError, result.State);
        Assert.False(result.ServerAccepted);
        Assert.Equal(unknownCode, result.Status.Code);
        Assert.Equal(referenceId, result.Status.ReferenceId);
        Assert.Equal("Ui.State.UnknownReason.Heading", result.Status.Heading);
        Assert.Equal("Ui.State.UnknownReason.Message", result.Status.Message);
        Assert.Equal(["retry", "support"], result.Status.NextActions.Select(action => action.ActionId));

        var mapperSource = RepositoryFiles.Read("src/StudentRegistration.Client/UX/UiStateMapper.cs");
        var reasonMap = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/states/reason-map.md");
        RepositoryFiles.ContainsAll(
            reasonMap,
            "does not display an arbitrary",
            "log secrets",
            "or guess success");
        Assert.DoesNotContain("ILogger", mapperSource, StringComparison.Ordinal);
        Assert.DoesNotContain("Console.", mapperSource, StringComparison.Ordinal);
        Assert.DoesNotContain(
            typeof(UiStateInput).GetProperties(),
            property => property.Name.Contains("Payload", StringComparison.OrdinalIgnoreCase));
    }

    private sealed class ResourceKeyTextProvider : IUiTextProvider
    {
        public string Get(string key) => key;
    }
}
