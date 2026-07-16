using StudentRegistration.Client.Features.Scheduling;

namespace StudentRegistration.AcceptanceTests.Specs.Spec012;

public sealed class SC_2OutcomeTests
{
    [Fact]
    public void Every_hard_conflict_blocks_review_with_keyboard_link_guidance_and_no_override()
    {
        var state = ConflictStateMapper.Map(
            [Spec012ConflictFixture.Overlap()],
            Spec012ConflictFixture.Meetings());

        Assert.True(state.ReviewBlocked);
        Assert.NotEmpty(state.BlockingReasons);
        var actions = Assert.Single(state.Conflicts).Source.Actions;
        Assert.All(actions, action => Assert.StartsWith("/", action.Route));
        Assert.Equal(2, actions.Count(action => action.Action == "change-group"));
        Assert.Equal(2, actions.Count(action => action.Action == "remove-group"));
        Assert.DoesNotContain(actions, action =>
            action.Action.Contains("override", StringComparison.OrdinalIgnoreCase));
    }
}
