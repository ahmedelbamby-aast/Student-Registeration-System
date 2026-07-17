namespace StudentRegistration.Contracts.Operations;

public sealed record OperationalMetricScope
{
    public OperationalMetricScope(Guid termId, Guid registrationWindowId)
    {
        if (termId == Guid.Empty)
        {
            throw new ArgumentException("A term scope is required.", nameof(termId));
        }
        if (registrationWindowId == Guid.Empty)
        {
            throw new ArgumentException(
                "A registration-window scope is required.",
                nameof(registrationWindowId));
        }

        TermId = termId;
        RegistrationWindowId = registrationWindowId;
    }

    public Guid TermId { get; }

    public Guid RegistrationWindowId { get; }
}

public enum OperationalMetricSourceState
{
    Available,
    Unavailable,
}

public sealed record OperationalMetricReadResult(
    OperationalMetricSourceState SourceState,
    IReadOnlyList<OperationalMetric> Metrics);

public interface IOperationalMetricReader
{
    Task<OperationalMetricReadResult> ReadAsync(
        OperationalMetricScope scope,
        CancellationToken cancellationToken = default);
}
