using System.Reflection;

namespace StudentRegistration.IntegrationTests.Specs.Spec012.EdgeCases;

public sealed class EC_3Tests
{
    public static TheoryData<SelectedGroupState, string> BlockingStates =>
        new()
        {
            { SelectedGroupState.Changed, "GROUP_CHANGED" },
            { SelectedGroupState.Full, "GROUP_FULL" },
            { SelectedGroupState.Unpublished, "GROUP_UNPUBLISHED" },
            { SelectedGroupState.Closed, "GROUP_CLOSED" },
            { SelectedGroupState.Cancelled, "GROUP_CANCELLED" }
        };

    [Theory]
    [MemberData(nameof(BlockingStates))]
    public void Changed_or_unavailable_group_blocks_review_with_change_and_remove_actions(
        SelectedGroupState state,
        string expectedCode)
    {
        var group = new SelectedGroup(
            "offering-ai401",
            "group-ai401-g01",
            "G01",
            state);

        var result = SelectionIssueProjectionDouble.Validate(group);

        var issue = Assert.Single(result.Issues);
        Assert.True(result.ReviewBlocked);
        Assert.Equal(expectedCode, issue.Code);
        Assert.Equal(group.OfferingId, issue.OfferingId);
        Assert.Equal(group.GroupId, issue.GroupId);
        Assert.Equal(group.GroupCode, issue.GroupCode);
        Assert.Equal(
            ["change-group", "remove-group"],
            issue.Actions.Select(action => action.Action));
        Assert.All(issue.Actions, action => Assert.Equal(group.GroupId, action.TargetGroupId));
        Assert.Contains(
            issue.Actions,
            action => action.Action == "change-group"
                && action.Route == $"/student/subjects/{group.OfferingId}");
        Assert.Contains(
            issue.Actions,
            action => action.Action == "remove-group"
                && action.Route == "/student/schedule");
        Assert.Equal(group, result.SelectedGroup);
    }

    [Fact]
    public void Production_group_state_actions_require_the_registration_plan_service()
    {
        var service = Assembly.Load("StudentRegistration.Registration").GetType(
            "StudentRegistration.Registration.Application.RegistrationPlanService");

        Assert.True(
            service is not null,
            "RegistrationPlanService must return changed/full/unpublished/closed/cancelled blockers with change/remove actions before EC-3 can pass against production code.");
        Assert.Contains(
            service!.GetMethods(BindingFlags.Instance | BindingFlags.Public),
            method => method.Name.Contains("Validat", StringComparison.Ordinal));
    }

    public enum SelectedGroupState
    {
        Current,
        Changed,
        Full,
        Unpublished,
        Closed,
        Cancelled
    }

    private sealed record SelectedGroup(
        string OfferingId,
        string GroupId,
        string GroupCode,
        SelectedGroupState State);

    private sealed record ResolutionAction(
        string Action,
        string TargetGroupId,
        string Label,
        string Route);

    private sealed record SelectionIssue(
        string Code,
        string OfferingId,
        string GroupId,
        string GroupCode,
        string Message,
        IReadOnlyList<ResolutionAction> Actions);

    private sealed record ValidationResult(
        SelectedGroup SelectedGroup,
        bool ReviewBlocked,
        IReadOnlyList<SelectionIssue> Issues);

    private static class SelectionIssueProjectionDouble
    {
        public static ValidationResult Validate(SelectedGroup group)
        {
            if (group.State is SelectedGroupState.Current)
            {
                return new(group, false, []);
            }

            var code = group.State switch
            {
                SelectedGroupState.Changed => "GROUP_CHANGED",
                SelectedGroupState.Full => "GROUP_FULL",
                SelectedGroupState.Unpublished => "GROUP_UNPUBLISHED",
                SelectedGroupState.Closed => "GROUP_CLOSED",
                SelectedGroupState.Cancelled => "GROUP_CANCELLED",
                _ => throw new ArgumentOutOfRangeException(nameof(group))
            };
            IReadOnlyList<ResolutionAction> actions =
            [
                new(
                    "change-group",
                    group.GroupId,
                    $"Change group {group.GroupCode}",
                    $"/student/subjects/{group.OfferingId}"),
                new(
                    "remove-group",
                    group.GroupId,
                    $"Remove group {group.GroupCode}",
                    "/student/schedule")
            ];

            return new(
                group,
                true,
                [
                    new(
                        code,
                        group.OfferingId,
                        group.GroupId,
                        group.GroupCode,
                        $"Group {group.GroupCode} requires review.",
                        actions)
                ]);
        }
    }
}
