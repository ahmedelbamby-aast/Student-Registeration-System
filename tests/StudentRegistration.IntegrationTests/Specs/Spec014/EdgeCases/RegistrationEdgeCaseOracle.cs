using System.Reflection;

namespace StudentRegistration.IntegrationTests.Specs.Spec014.EdgeCases;

internal static class ProductionEdgeCapability
{
    public static void Require(
        string assemblyName,
        string typeName,
        params string[] methodNames)
    {
        var assembly = Assembly.Load(assemblyName);
        var type = assembly.GetType(typeName);

        Assert.True(
            type is not null,
            $"SPEC-014 expected-red: production capability '{typeName}' is not implemented.");

        var methods = type!.GetMethods(
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
        foreach (var methodName in methodNames)
        {
            Assert.True(
                methods.Any(method => string.Equals(
                    method.Name,
                    methodName,
                    StringComparison.Ordinal)),
                $"SPEC-014 expected-red: production capability '{typeName}.{methodName}' is not implemented.");
        }
    }
}

internal sealed class AtomicRegistrationStore(int capacity = 1)
{
    private State _committed = new(capacity);

    public State Committed => _committed.Copy();

    public Transaction Begin() => new(this, _committed.Copy());

    internal void Commit(State state) => _committed = state.Copy();

    internal sealed class Transaction(AtomicRegistrationStore owner, State state)
    {
        private State? _savepoint;
        private bool _aborted;

        public State State => state;

        public void Claim(string payloadHash)
        {
            EnsureUsable();
            state.ClaimPayloadHash = payloadHash;
            state.Processing = true;
            state.ExecutionCount++;
        }

        public void CreateAllocationSavepoint()
        {
            EnsureUsable();
            _savepoint = state.Copy();
        }

        public void Allocate()
        {
            EnsureUsable();
            if (state.SeatCount >= state.Capacity)
            {
                throw new InvalidOperationException("GROUP_FULL");
            }

            state.SeatCount++;
            state.EnrollmentCount++;
        }

        public void Complete(string resultCode, string? reference = null)
        {
            EnsureUsable();
            state.Processing = false;
            state.ResultCode = resultCode;
            state.Reference = reference;
            state.ReceiptStored = reference is not null;
        }

        public void AbortTransaction() => _aborted = true;

        public void RollbackToAllocationSavepoint()
        {
            EnsureUsable();
            state = _savepoint?.Copy()
                ?? throw new InvalidOperationException("Allocation savepoint was not created.");
        }

        public void Commit()
        {
            EnsureUsable();
            owner.Commit(state);
        }

        public void Rollback()
        {
            // The working state, including an uncommitted claim, is discarded.
        }

        private void EnsureUsable()
        {
            if (_aborted)
            {
                throw new InvalidOperationException("TRANSACTION_ABORTED");
            }
        }
    }

    internal sealed class State(int capacity)
    {
        public int Capacity { get; set; } = capacity;
        public int SeatCount { get; set; }
        public int EnrollmentCount { get; set; }
        public int ExecutionCount { get; set; }
        public bool Processing { get; set; }
        public bool ReceiptStored { get; set; }
        public string? ClaimPayloadHash { get; set; }
        public string? ResultCode { get; set; }
        public string? Reference { get; set; }

        public State Copy() => new(Capacity)
        {
            SeatCount = SeatCount,
            EnrollmentCount = EnrollmentCount,
            ExecutionCount = ExecutionCount,
            Processing = Processing,
            ReceiptStored = ReceiptStored,
            ClaimPayloadHash = ClaimPayloadHash,
            ResultCode = ResultCode,
            Reference = Reference
        };
    }
}

internal sealed class SharedBoundary
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private int _inside;

    public int MaximumConcurrent { get; private set; }

    public async Task ExecuteAsync(Func<Task> work)
    {
        await _gate.WaitAsync();
        try
        {
            MaximumConcurrent = Math.Max(
                MaximumConcurrent,
                Interlocked.Increment(ref _inside));
            await work();
        }
        finally
        {
            Interlocked.Decrement(ref _inside);
            _gate.Release();
        }
    }
}
