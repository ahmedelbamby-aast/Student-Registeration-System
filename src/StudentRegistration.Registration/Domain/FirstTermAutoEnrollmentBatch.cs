namespace StudentRegistration.Registration.Domain;

public enum FirstTermAutoEnrollmentBatchState
{
    Pending = 1,
    Running = 2,
    Complete = 3,
    CompletedWithFailures = 4,
    Failed = 5,
}

public enum FirstTermAutoEnrollmentItemState
{
    Pending = 1,
    Running = 2,
    Accepted = 3,
    Failed = 4,
}

public sealed class FirstTermAutoEnrollmentBatch
{
    private readonly List<FirstTermAutoEnrollmentItem> _items = [];

    private FirstTermAutoEnrollmentBatch()
    {
    }

    public FirstTermAutoEnrollmentBatch(
        Guid id,
        Guid termId,
        Guid catalogueVersionId,
        string cohortScope,
        string purpose,
        DateTime createdAtUtc)
    {
        RegistrationPlanDomainGuard.Identifier(id, nameof(id));
        RegistrationPlanDomainGuard.Identifier(termId, nameof(termId));
        RegistrationPlanDomainGuard.Identifier(catalogueVersionId, nameof(catalogueVersionId));
        EnsureUtc(createdAtUtc, nameof(createdAtUtc));

        Id = id;
        TermId = termId;
        CatalogueVersionId = catalogueVersionId;
        CohortScope = RegistrationPlanDomainGuard.Required(cohortScope, nameof(cohortScope));
        Purpose = RegistrationPlanDomainGuard.Required(purpose, nameof(purpose));
        State = FirstTermAutoEnrollmentBatchState.Pending;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid TermId { get; private set; }
    public Guid CatalogueVersionId { get; private set; }
    public string CohortScope { get; private set; } = string.Empty;
    public string Purpose { get; private set; } = string.Empty;
    public FirstTermAutoEnrollmentBatchState State { get; private set; }
    public string? LeaseOwner { get; private set; }
    public DateTime? LeaseExpiresAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? StartedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public byte[] Version { get; private set; } = [];
    public IReadOnlyList<FirstTermAutoEnrollmentItem> Items => _items;
    public int TotalStudents => _items.Count;
    public int AcceptedStudents => _items.Count(item => item.State is FirstTermAutoEnrollmentItemState.Accepted);
    public int FailedStudents => _items.Count(item => item.State is FirstTermAutoEnrollmentItemState.Failed);

    public void AddItem(FirstTermAutoEnrollmentItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (State is not FirstTermAutoEnrollmentBatchState.Pending)
        {
            throw new InvalidOperationException("Items cannot be added after batch processing begins.");
        }

        if (item.BatchId != Id)
        {
            throw new ArgumentException("The item must belong to this batch.", nameof(item));
        }

        if (_items.Any(existing =>
            existing.Id == item.Id || existing.StudentId == item.StudentId))
        {
            throw new ArgumentException("Batch item and student identifiers must be unique.", nameof(item));
        }

        _items.Add(item);
    }

    public void Start(
        string leaseOwner,
        DateTime startedAtUtc,
        DateTime leaseExpiresAtUtc)
    {
        if (State is not FirstTermAutoEnrollmentBatchState.Pending)
        {
            throw new InvalidOperationException("Only a pending batch can start.");
        }

        EnsureUtc(startedAtUtc, nameof(startedAtUtc));
        EnsureUtc(leaseExpiresAtUtc, nameof(leaseExpiresAtUtc));
        if (leaseExpiresAtUtc <= startedAtUtc)
        {
            throw new ArgumentOutOfRangeException(nameof(leaseExpiresAtUtc));
        }

        LeaseOwner = RegistrationPlanDomainGuard.Required(leaseOwner, nameof(leaseOwner));
        StartedAtUtc = startedAtUtc;
        LeaseExpiresAtUtc = leaseExpiresAtUtc;
        State = FirstTermAutoEnrollmentBatchState.Running;
    }

    public void Complete(DateTime completedAtUtc)
    {
        if (State is not FirstTermAutoEnrollmentBatchState.Running ||
            _items.Any(item => item.State is FirstTermAutoEnrollmentItemState.Pending
                or FirstTermAutoEnrollmentItemState.Running))
        {
            throw new InvalidOperationException("Every batch item must be terminal before completion.");
        }

        EnsureUtc(completedAtUtc, nameof(completedAtUtc));
        if (completedAtUtc < StartedAtUtc)
        {
            throw new ArgumentOutOfRangeException(nameof(completedAtUtc));
        }

        State = FailedStudents == 0
            ? FirstTermAutoEnrollmentBatchState.Complete
            : AcceptedStudents == 0
                ? FirstTermAutoEnrollmentBatchState.Failed
                : FirstTermAutoEnrollmentBatchState.CompletedWithFailures;
        CompletedAtUtc = completedAtUtc;
        LeaseOwner = null;
        LeaseExpiresAtUtc = null;
    }

    private static void EnsureUtc(DateTime value, string name)
    {
        if (value.Kind is not DateTimeKind.Utc)
        {
            throw new ArgumentException("The timestamp must be UTC.", name);
        }
    }
}

public sealed class FirstTermAutoEnrollmentItem
{
    private FirstTermAutoEnrollmentItem()
    {
    }

    public FirstTermAutoEnrollmentItem(
        Guid id,
        Guid batchId,
        Guid studentId,
        Guid clientRequestId)
    {
        RegistrationPlanDomainGuard.Identifier(id, nameof(id));
        RegistrationPlanDomainGuard.Identifier(batchId, nameof(batchId));
        RegistrationPlanDomainGuard.Identifier(studentId, nameof(studentId));
        RegistrationPlanDomainGuard.Identifier(clientRequestId, nameof(clientRequestId));
        Id = id;
        BatchId = batchId;
        StudentId = studentId;
        ClientRequestId = clientRequestId;
        State = FirstTermAutoEnrollmentItemState.Pending;
    }

    public Guid Id { get; private set; }
    public Guid BatchId { get; private set; }
    public Guid StudentId { get; private set; }
    public Guid ClientRequestId { get; private set; }
    public FirstTermAutoEnrollmentItemState State { get; private set; }
    public Guid? SubmissionId { get; private set; }
    public string? FailureCode { get; private set; }
    public DateTime? StartedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public byte[] Version { get; private set; } = [];

    public void Start(DateTime startedAtUtc)
    {
        if (State is not FirstTermAutoEnrollmentItemState.Pending)
        {
            throw new InvalidOperationException("Only a pending batch item can start.");
        }

        EnsureUtc(startedAtUtc, nameof(startedAtUtc));
        State = FirstTermAutoEnrollmentItemState.Running;
        StartedAtUtc = startedAtUtc;
    }

    public void Accept(Guid submissionId, DateTime completedAtUtc)
    {
        EnsureRunning(completedAtUtc);
        RegistrationPlanDomainGuard.Identifier(submissionId, nameof(submissionId));
        SubmissionId = submissionId;
        State = FirstTermAutoEnrollmentItemState.Accepted;
        CompletedAtUtc = completedAtUtc;
    }

    public void Fail(string failureCode, DateTime completedAtUtc)
    {
        EnsureRunning(completedAtUtc);
        FailureCode = RegistrationPlanDomainGuard.Required(failureCode, nameof(failureCode));
        State = FirstTermAutoEnrollmentItemState.Failed;
        CompletedAtUtc = completedAtUtc;
    }

    private void EnsureRunning(DateTime completedAtUtc)
    {
        if (State is not FirstTermAutoEnrollmentItemState.Running)
        {
            throw new InvalidOperationException("Only a running batch item can complete.");
        }

        EnsureUtc(completedAtUtc, nameof(completedAtUtc));
        if (completedAtUtc < StartedAtUtc)
        {
            throw new ArgumentOutOfRangeException(nameof(completedAtUtc));
        }
    }

    private static void EnsureUtc(DateTime value, string name)
    {
        if (value.Kind is not DateTimeKind.Utc)
        {
            throw new ArgumentException("The timestamp must be UTC.", name);
        }
    }
}
