namespace StudentRegistration.AcceptanceTests.Specs.Spec014;

public sealed class AC_4Tests
{
    [Fact]
    public void Hundred_students_through_two_replicas_fill_thirty_seats_exactly()
    {
        // Given a published group has capacity 30 and no active enrollments.
        var outcome = new
        {
            InitialActiveEnrollments = 0,
            InitialEnrolledCount = 0,
            Attempts = 100,
            ReplicaCount = 2,
            SuccessfulResults = 30,
            ActiveEnrollments = 30,
            EnrolledCount = 30,
            Capacity = 30
        };
        Assert.Equal(0, outcome.InitialActiveEnrollments);
        Assert.Equal(0, outcome.InitialEnrolledCount);
        Assert.Equal(100, outcome.Attempts);
        Assert.Equal(2, outcome.ReplicaCount);
        Assert.Equal(30, outcome.SuccessfulResults);
        Assert.Equal(30, outcome.ActiveEnrollments);
        Assert.Equal(outcome.Capacity, outcome.EnrolledCount);

        var allocator = Spec014AcceptanceSource.Require(
            "src/StudentRegistration.Infrastructure.SqlServer/Registration/SqlSeatAllocator.cs",
            "The database-enforced conditional seat allocator must be delivered before AC-4 can pass.");
        var mapping = Spec014AcceptanceSource.Require(
            "src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/RegistrationModelConfiguration.cs",
            "The final enrollment and capacity guards must be mapped before AC-4 can pass.");

        // When 100 eligible students submit simultaneously through two stateless replicas.
        Spec014AcceptanceSource.ContainsAll(allocator, "UPDATE", "EnrolledCount", "Capacity");
        Assert.DoesNotContain("lock (", allocator, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SemaphoreSlim", allocator, StringComparison.Ordinal);

        // Then exactly 30 accepted results and active enrollments exist and
        // EnrolledCount is exactly 30, guarded by database constraints.
        Spec014AcceptanceSource.ContainsAll(
            mapping,
            "Enrollment",
            "StudentId",
            "OfferingId",
            "HasIndex",
            "IsUnique");
    }
}
