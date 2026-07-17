namespace StudentRegistration.IntegrationTests.Specs.Spec014.EdgeCases;

public sealed class EC_4Tests
{
    [Fact]
    public async Task Capacity_reduction_and_enrollment_share_one_group_boundary()
    {
        var boundary = new SharedBoundary();
        var capacity = 1;
        var enrolled = 0;

        var enrollment = boundary.ExecuteAsync(async () =>
        {
            await Task.Yield();
            if (enrolled < capacity)
            {
                enrolled++;
            }
        });
        var reduction = boundary.ExecuteAsync(() =>
        {
            // A reduction that would make the invariant false is rejected.
            if (0 >= enrolled)
            {
                capacity = 0;
            }

            return Task.CompletedTask;
        });

        await Task.WhenAll(enrollment, reduction);

        Assert.Equal(1, boundary.MaximumConcurrent);
        Assert.InRange(enrolled, 0, capacity);

        ProductionEdgeCapability.Require(
            "StudentRegistration.Infrastructure.SqlServer",
            "StudentRegistration.Infrastructure.SqlServer.Registration.SqlSeatAllocator",
            "AllocateAsync",
            "ReduceCapacityAsync");
    }
}
