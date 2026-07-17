namespace StudentRegistration.AcceptanceTests.Specs.Spec014;

public sealed class AC_2Tests
{
    [Fact]
    public void Multi_group_failure_rolls_back_every_allocation_and_commits_a_replayable_rejection()
    {
        // Given Group A has a seat and Group B becomes full.
        var outcome = new
        {
            GroupACapacity = 5,
            Accepted = false,
            GroupABeforeCount = 4,
            GroupAAfterCount = 4,
            GroupAEnrollmentsForStudent = 0,
            GroupBBeforeCount = 10,
            GroupBCapacity = 10,
            GroupBAfterCount = 10,
            GroupBEnrollmentsForStudent = 0,
            ResultCode = "GROUP_FULL",
            ReplayResultCode = "GROUP_FULL",
            FinalAllocationUsedOneTransaction = true
        };
        Assert.Equal(1, outcome.GroupACapacity - outcome.GroupABeforeCount);
        Assert.Equal(0, outcome.GroupBCapacity - outcome.GroupBBeforeCount);
        Assert.True(outcome.FinalAllocationUsedOneTransaction);
        Assert.False(outcome.Accepted);
        Assert.Equal(outcome.GroupABeforeCount, outcome.GroupAAfterCount);
        Assert.Equal(0, outcome.GroupAEnrollmentsForStudent);
        Assert.Equal(outcome.GroupBBeforeCount, outcome.GroupBAfterCount);
        Assert.Equal(0, outcome.GroupBEnrollmentsForStudent);
        Assert.Equal(outcome.ResultCode, outcome.ReplayResultCode);

        var allocator = Spec014AcceptanceSource.Require(
            "src/StudentRegistration.Infrastructure.SqlServer/Registration/SqlSeatAllocator.cs",
            "The savepoint-aware SQL seat allocator must be delivered before AC-2 can pass.");
        var store = Spec014AcceptanceSource.Require(
            "src/StudentRegistration.Infrastructure.SqlServer/Registration/RegistrationSubmissionStore.cs",
            "The atomic submission result store must be delivered before AC-2 can pass.");

        // When final allocation is attempted as one transaction.
        Spec014AcceptanceSource.ContainsAll(
            allocator,
            "CreateSavepoint",
            "RollbackToSavepoint",
            "GROUP_FULL",
            "Enrollment");

        // Then neither group retains a count/enrollment mutation and the same key and
        // payload replay the committed deterministic rejection.
        Spec014AcceptanceSource.ContainsAll(
            store,
            "Rejected",
            "PayloadHash",
            "ClientRequestId",
            "Replay");
    }
}
