using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using StudentRegistration.IdentityAccess.Application.Ports;

namespace StudentRegistration.Api.Development;

/// <summary>
/// Process-local Testing adapter. Proofs are never written to disk and are
/// removed when the isolated test fixture consumes them.
/// </summary>
public sealed class TestingRecoveryProofDelivery : IAccountRecoveryProofDelivery
{
    private const string TestingEnvironment = "Testing";
    private readonly ConcurrentDictionary<string, RecoveryProofDeliveryRequest> _deliveries =
        new(StringComparer.Ordinal);
    private readonly IHostEnvironment _environment;
    private readonly TimeProvider _timeProvider;

    public TestingRecoveryProofDelivery(
        IHostEnvironment environment,
        TimeProvider timeProvider)
    {
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        EnsureTesting();
    }

    public Task<RecoveryProofDeliveryResult> DeliverAsync(
        RecoveryProofDeliveryRequest request,
        CancellationToken cancellationToken)
    {
        EnsureTesting();
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        RemoveExpired();
        if (request.ExpiresAtUtc.Kind != DateTimeKind.Utc
            || request.ExpiresAtUtc <= _timeProvider.GetUtcNow().UtcDateTime)
        {
            return Task.FromResult(
                new RecoveryProofDeliveryResult(false, "testing-expired"));
        }

        var deliveryReference = $"testing:{Guid.NewGuid():N}";
        if (!_deliveries.TryAdd(deliveryReference, request))
        {
            throw new InvalidOperationException(
                "Unable to allocate a unique Testing recovery delivery reference.");
        }

        return Task.FromResult(
            new RecoveryProofDeliveryResult(true, deliveryReference));
    }

    public bool TryTake(
        string deliveryReference,
        out RecoveryProofDeliveryRequest? request)
    {
        EnsureTesting();
        RemoveExpired();
        return _deliveries.TryRemove(deliveryReference, out request);
    }

    private void RemoveExpired()
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        foreach (var delivery in _deliveries)
        {
            if (delivery.Value.ExpiresAtUtc <= utcNow)
            {
                _deliveries.TryRemove(delivery.Key, out _);
            }
        }
    }

    private void EnsureTesting()
    {
        if (!_environment.IsEnvironment(TestingEnvironment))
        {
            throw new InvalidOperationException(
                "The in-memory recovery proof adapter is available only in Testing.");
        }
    }
}
