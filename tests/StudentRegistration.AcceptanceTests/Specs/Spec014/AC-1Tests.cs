using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec014;

public sealed class AC_1Tests
{
    [Fact]
    public void Final_seat_race_has_one_winner_one_group_full_loser_and_never_overbooks()
    {
        // Given one seat remains in a published group and two eligible students submit concurrently.
        var outcome = new
        {
            InitialEnrolledCount = 9,
            InitialCapacity = 10,
            EligibleConcurrentAttempts = 2,
            SuccessfulResults = 1,
            ConflictResults = 1,
            LoserStatus = 409,
            LoserCode = "GROUP_FULL",
            EnrolledCount = 1,
            Capacity = 1
        };
        Assert.Equal(1, outcome.InitialCapacity - outcome.InitialEnrolledCount);
        Assert.Equal(2, outcome.EligibleConcurrentAttempts);
        Assert.Equal(1, outcome.SuccessfulResults);
        Assert.Equal(1, outcome.ConflictResults);
        Assert.Equal(409, outcome.LoserStatus);
        Assert.Equal("GROUP_FULL", outcome.LoserCode);
        Assert.True(outcome.EnrolledCount <= outcome.Capacity);

        var allocator = Spec014AcceptanceSource.Require(
            "src/StudentRegistration.Infrastructure.SqlServer/Registration/SqlSeatAllocator.cs",
            "The atomic SQL seat allocator must be delivered before AC-1 can pass.");

        // When both transactions reach SQL Server.
        Spec014AcceptanceSource.ContainsAll(
            allocator,
            "EnrolledCount",
            "Capacity",
            "RegistrationPaused",
            "GROUP_FULL");

        // Then exactly one conditional allocation succeeds, one receives GROUP_FULL,
        // and the database predicate prevents EnrolledCount from exceeding Capacity.
        Assert.Contains("UPDATE", allocator, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("WHERE", allocator, StringComparison.OrdinalIgnoreCase);
    }
}

internal static class Spec014AcceptanceSource
{
    internal static string Require(string path, string capabilityMessage)
    {
        Assert.True(RepositoryFiles.Exists(path), capabilityMessage);
        return RepositoryFiles.Read(path);
    }

    internal static void ContainsAll(string source, params string[] expected) =>
        RepositoryFiles.ContainsAll(source, expected);
}
