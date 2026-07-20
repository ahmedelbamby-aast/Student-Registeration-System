using StudentRegistration.Registration.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec014;

public sealed class FirstTermAutoEnrollmentBatchModelTests
{
    [Fact]
    public void Batch_runs_items_and_records_complete_or_failed_outcomes_idempotently()
    {
        var batch = new FirstTermAutoEnrollmentBatch(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "AI:2026", "term-one-roots", Utc(8));
        var accepted = new FirstTermAutoEnrollmentItem(
            Guid.NewGuid(), batch.Id, Guid.NewGuid(), Guid.NewGuid());
        var failed = new FirstTermAutoEnrollmentItem(
            Guid.NewGuid(), batch.Id, Guid.NewGuid(), Guid.NewGuid());
        batch.AddItem(accepted);
        batch.AddItem(failed);

        batch.Start("worker-1", Utc(9), Utc(10));
        accepted.Start(Utc(9));
        accepted.Accept(Guid.NewGuid(), Utc(9));
        failed.Start(Utc(9));
        failed.Fail("NO_COMPLETE_SCHEDULE", Utc(9));
        batch.Complete(Utc(10));

        Assert.Equal(FirstTermAutoEnrollmentBatchState.CompletedWithFailures, batch.State);
        Assert.Equal(2, batch.TotalStudents);
        Assert.Equal(1, batch.AcceptedStudents);
        Assert.Equal(1, batch.FailedStudents);
        Assert.Throws<InvalidOperationException>(() => batch.AddItem(
            new FirstTermAutoEnrollmentItem(Guid.NewGuid(), batch.Id, Guid.NewGuid(), Guid.NewGuid())));
    }

    [Fact]
    public void Batch_rejects_duplicate_students_and_invalid_lease()
    {
        var batch = new FirstTermAutoEnrollmentBatch(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "AI:2026", "term-one-roots", Utc(8));
        var studentId = Guid.NewGuid();
        batch.AddItem(new FirstTermAutoEnrollmentItem(Guid.NewGuid(), batch.Id, studentId, Guid.NewGuid()));

        Assert.Throws<ArgumentException>(() => batch.AddItem(
            new FirstTermAutoEnrollmentItem(Guid.NewGuid(), batch.Id, studentId, Guid.NewGuid())));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            batch.Start("worker", Utc(10), Utc(9)));
    }

    private static DateTime Utc(int hour) =>
        new(2026, 7, 20, hour, 0, 0, DateTimeKind.Utc);
}
