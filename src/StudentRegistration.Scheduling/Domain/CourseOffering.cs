namespace StudentRegistration.Scheduling.Domain;

public enum CourseOfferingState
{
    Draft = 1,
    Published = 2,
    Closed = 3,
    Cancelled = 4,
}

public sealed class CourseOffering
{
    private CourseOffering()
    {
    }

    public CourseOffering(
        Guid id,
        Guid termId,
        Guid courseId,
        CourseOfferingState state)
    {
        SchedulingDomainValue.Identifier(id, nameof(id));
        SchedulingDomainValue.Identifier(termId, nameof(termId));
        SchedulingDomainValue.Identifier(courseId, nameof(courseId));
        SchedulingDomainValue.Defined(state, nameof(state));

        Id = id;
        TermId = termId;
        CourseId = courseId;
        State = state;
    }

    public Guid Id { get; private set; }

    public Guid TermId { get; private set; }

    public Guid CourseId { get; private set; }

    public CourseOfferingState State { get; private set; }

    public byte[] Version { get; private set; } = [];

    public void Publish()
    {
        if (State is not CourseOfferingState.Draft)
        {
            throw new InvalidOperationException("Only a draft offering can be published.");
        }

        State = CourseOfferingState.Published;
    }

    public void Close()
    {
        if (State is not CourseOfferingState.Published)
        {
            throw new InvalidOperationException("Only a published offering can be closed.");
        }

        State = CourseOfferingState.Closed;
    }

    public void Cancel()
    {
        if (State is not CourseOfferingState.Published)
        {
            throw new InvalidOperationException("Only a published offering can be cancelled.");
        }

        State = CourseOfferingState.Cancelled;
    }
}
