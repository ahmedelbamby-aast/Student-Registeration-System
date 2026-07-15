using StudentRegistration.Academics.Application.Ports;

namespace StudentRegistration.ApplicationTests.Infrastructure;

internal sealed class FakeAcademicContextReader : IAcademicContextReader
{
    public IReadOnlyList<AcademicTermContextRecord> Terms { get; set; } = [];

    public IReadOnlyList<RegistrationWindowContextRecord> Windows { get; set; } = [];

    public int SnapshotReads { get; private set; }

    public Task<AcademicContextSnapshot> ResolveContextAsync(
        AcademicStudentScope? studentScope,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        SnapshotReads++;

        var matches = Windows
            .Where(window => studentScope is null || window.AppliesTo(studentScope))
            .ToArray();
        return Task.FromResult(new AcademicContextSnapshot(Terms, matches));
    }
}

internal sealed class MutableTimeProvider(DateTimeOffset utcNow) : TimeProvider
{
    public DateTimeOffset UtcNow { get; set; } = utcNow;

    public override DateTimeOffset GetUtcNow() => UtcNow;
}
