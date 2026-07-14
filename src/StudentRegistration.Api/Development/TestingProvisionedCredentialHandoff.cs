using System.Collections.Concurrent;
using StudentRegistration.IdentityAccess.Application;
using StudentRegistration.IdentityAccess.Application.Ports;

namespace StudentRegistration.Api.Development;

/// <summary>
/// Process-bounded Testing handoff. It deliberately exposes completed values
/// only to an injected test fixture and never serializes them.
/// </summary>
public sealed class TestingProvisionedCredentialHandoff : IProvisionedCredentialHandoff
{
    private const int MaximumBatches = 256;
    private const int MaximumCredentialsPerBatch = 25_004;
    private readonly ConcurrentDictionary<Guid, IReadOnlyList<DemoCredential>> _pending = new();
    private readonly ConcurrentDictionary<Guid, IReadOnlyList<DemoCredential>> _completed = new();
    private readonly object _gate = new();

    public TestingProvisionedCredentialHandoff(IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(environment);
        if (!environment.IsEnvironment("Testing"))
        {
            throw new InvalidOperationException(
                "The in-memory import credential handoff is available only in Testing.");
        }
    }

    public Task PrepareAsync(
        Guid importId,
        IReadOnlyCollection<DemoCredential> credentials,
        CancellationToken cancellationToken)
    {
        ValidateImportId(importId);
        ArgumentNullException.ThrowIfNull(credentials);
        cancellationToken.ThrowIfCancellationRequested();
        if (credentials.Count is < 1 or > MaximumCredentialsPerBatch)
        {
            throw new ArgumentOutOfRangeException(nameof(credentials));
        }

        var snapshot = credentials
            .Select(credential => credential with { })
            .ToArray();
        lock (_gate)
        {
            if (_completed.ContainsKey(importId))
            {
                return Task.CompletedTask;
            }

            if (_pending.TryGetValue(importId, out var prepared))
            {
                if (!prepared.SequenceEqual(snapshot))
                {
                    throw new InvalidOperationException(
                        "IDENTITY_HANDOFF_PREPARE_MISMATCH: The import already has different pending credentials.");
                }

                return Task.CompletedTask;
            }

            if (_pending.Count + _completed.Count >= MaximumBatches)
            {
                throw new InvalidOperationException(
                    "IDENTITY_HANDOFF_CAPACITY_EXCEEDED: The Testing handoff is full.");
            }

            _pending.TryAdd(importId, snapshot);
        }

        return Task.CompletedTask;
    }

    public Task CompleteAsync(Guid importId, CancellationToken cancellationToken)
    {
        ValidateImportId(importId);
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            if (_completed.ContainsKey(importId))
            {
                _pending.TryRemove(importId, out _);
                return Task.CompletedTask;
            }

            if (!_pending.TryRemove(importId, out var credentials))
            {
                throw new InvalidOperationException(
                    "IDENTITY_HANDOFF_NOT_PREPARED: The import credential handoff is unavailable.");
            }

            _completed.TryAdd(importId, credentials);
        }

        return Task.CompletedTask;
    }

    public Task AbortAsync(Guid importId, CancellationToken cancellationToken)
    {
        ValidateImportId(importId);
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            _pending.TryRemove(importId, out _);
        }

        return Task.CompletedTask;
    }

    public bool TryGetCompleted(
        Guid importId,
        out IReadOnlyList<DemoCredential> credentials)
    {
        lock (_gate)
        {
            return _completed.TryGetValue(importId, out credentials!);
        }
    }

    private static void ValidateImportId(Guid importId)
    {
        if (importId == Guid.Empty)
        {
            throw new ArgumentException("An import ID is required.", nameof(importId));
        }
    }
}
