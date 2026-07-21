using StudentRegistration.TestSupport;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace StudentRegistration.AcceptanceTests.Specs.Spec005;

[Collection(Spec005SqlAcceptanceCollection.Name)]
public sealed class AC_2Tests(Spec005SqlAcceptanceDatabase database)
{
    [Fact]
    public void Capacity_reduction_below_active_enrollment_is_rejected_without_mutation()
    {
        var service = RepositoryFiles.Read(
            "src/StudentRegistration.Scheduling/Application/SectionGroupCapacityService.cs");
        var allocator = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/Registration/SqlSeatAllocator.cs");
        var integration = RepositoryFiles.Read(
            "tests/StudentRegistration.IntegrationTests/Scheduling/GroupCapacityRaceTests.cs");

        RepositoryFiles.ContainsAll(
            service,
            "CapacityBelowEnrolled",
            "capacity < state.EnrolledCount",
            "ChangeCapacity");
        RepositoryFiles.ContainsAll(
            allocator,
            "ReduceCapacityAsync",
            "[EnrolledCount] + [HeldSeatCount] <= {newCapacity}",
            "UPDLOCK, HOLDLOCK");
        RepositoryFiles.ContainsAll(
            integration,
            "Capacity_below_enrollment_changes_nothing",
            "Assert.Equal(30, store.Capacity)",
            "Assert.Equal(0, store.CommitCount)");
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Real_sql_preserves_capacity_twenty_when_reduction_to_nineteen_is_invalid()
    {
        var graph = await database.SeedAsync(20, 20, 20);
        await using var context = database.CreateContext();
        await Assert.ThrowsAsync<SqlException>(() =>
            context.Database.ExecuteSqlInterpolatedAsync($"""
                UPDATE [scheduling].[SectionGroups]
                SET [Capacity] = 19
                WHERE [Id] = {graph.GroupId};
                """));
        Assert.Equal(
            20,
            await context.Database.SqlQuery<int>($"""
                SELECT [Capacity] AS [Value]
                FROM [scheduling].[SectionGroups]
                WHERE [Id] = {graph.GroupId}
                """).SingleAsync());
    }
}
