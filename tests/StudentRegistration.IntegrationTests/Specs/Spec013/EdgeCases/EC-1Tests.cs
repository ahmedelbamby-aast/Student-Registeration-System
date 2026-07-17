using System.Reflection;

namespace StudentRegistration.IntegrationTests.Specs.Spec013.EdgeCases;

public sealed class EC_1Tests
{
    [Fact]
    public void Recommendation_does_not_reserve_a_group_and_final_commit_remains_authoritative()
    {
        var capacity = new CapacityBoundaryDouble();

        var option = capacity.Recommend("group-ai401-g01");
        capacity.MarkFull("group-ai401-g01");
        var commit = capacity.FinalCommit(option.GroupId);

        Assert.Equal("group-ai401-g01", option.GroupId);
        Assert.Equal(0, capacity.ReservedSeats);
        Assert.Equal(CommitOutcome.GroupFull, commit);
        Assert.Empty(capacity.QueryFreshViableGroups());
    }

    [Fact]
    public void Production_application_boundary_applies_options_without_a_reservation_api()
    {
        var service = Assembly.Load("StudentRegistration.Registration").GetType(
            "StudentRegistration.Registration.Application.RecommendationApplicationService");

        Assert.True(
            service is not null,
            "T049/T051/T053 must provide RecommendationApplicationService before EC-1 can pass against production code.");
        Assert.Contains(
            service!.GetMethods(BindingFlags.Instance | BindingFlags.Public),
            method => method.Name.Contains("Apply", StringComparison.Ordinal));
        Assert.DoesNotContain(
            service.GetMethods(BindingFlags.Instance | BindingFlags.Public),
            method => method.Name.Contains("Reserv", StringComparison.Ordinal));
    }

    private enum CommitOutcome
    {
        Committed,
        GroupFull
    }

    private sealed record RecommendedOption(string GroupId);

    private sealed class CapacityBoundaryDouble
    {
        private readonly Dictionary<string, bool> _available =
            new(StringComparer.Ordinal)
            {
                ["group-ai401-g01"] = true
            };

        public int ReservedSeats { get; private set; }

        public RecommendedOption Recommend(string groupId)
        {
            Assert.True(_available[groupId]);
            return new(groupId);
        }

        public void MarkFull(string groupId) =>
            _available[groupId] = false;

        public IReadOnlyList<string> QueryFreshViableGroups() =>
            _available
                .Where(group => group.Value)
                .Select(group => group.Key)
                .Order(StringComparer.Ordinal)
                .ToArray();

        public CommitOutcome FinalCommit(string groupId) =>
            _available[groupId]
                ? CommitOutcome.Committed
                : CommitOutcome.GroupFull;
    }
}
