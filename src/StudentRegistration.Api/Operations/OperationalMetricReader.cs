using StudentRegistration.Contracts.Operations;

namespace StudentRegistration.Api.Operations;

public sealed class OperationalMetricReader(OperationalTelemetry telemetry)
    : IOperationalMetricReader
{
    private readonly OperationalTelemetry _telemetry =
        telemetry ?? throw new ArgumentNullException(nameof(telemetry));

    public Task<OperationalMetricReadResult> ReadAsync(
        OperationalMetricScope scope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);
        cancellationToken.ThrowIfCancellationRequested();

        // OperationalTelemetry already enforces bounded, allow-listed,
        // privacy-safe series. Scope is validated at the contract boundary; the
        // POC has one active registration window and never adds high-cardinality
        // term/window identifiers to metric dimensions.
        var metrics = _telemetry.Snapshot();
        return Task.FromResult(new OperationalMetricReadResult(
            OperationalMetricSourceState.Available,
            metrics));
    }
}
