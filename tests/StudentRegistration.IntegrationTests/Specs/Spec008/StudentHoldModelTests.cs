using System.Reflection;
using StudentRegistration.Academics.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec008;

public sealed class StudentHoldModelTests
{
    private static readonly DateTime EffectiveFromUtc =
        new(2026, 7, 14, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime EffectiveToUtc = EffectiveFromUtc.AddDays(2);
    private static readonly DateTime ImportedAtUtc = EffectiveFromUtc.AddMinutes(-5);

    [Fact]
    public void Constructor_preserves_term_scoped_blocking_hold_and_provenance()
    {
        var id = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var termId = Guid.NewGuid();

        var hold = new StudentHold(
            id,
            studentId,
            termId,
            "REGISTRATION-HOLD",
            "Resolve the synthetic demo hold before registration.",
            true,
            EffectiveFromUtc,
            EffectiveToUtc,
            "synthetic-hold-import",
            "fixture-42/hold-1",
            ImportedAtUtc);

        Assert.Equal(id, hold.Id);
        Assert.Equal(studentId, hold.StudentId);
        Assert.Equal(termId, hold.TermId);
        Assert.Equal("REGISTRATION-HOLD", hold.Code);
        Assert.Equal("Resolve the synthetic demo hold before registration.", hold.Message);
        Assert.True(hold.BlocksRegistration);
        Assert.Equal(EffectiveFromUtc, hold.EffectiveFromUtc);
        Assert.Equal(EffectiveToUtc, hold.EffectiveToUtc);
        Assert.Equal("synthetic-hold-import", hold.Source);
        Assert.Equal("fixture-42/hold-1", hold.SourceReference);
        Assert.Equal(ImportedAtUtc, hold.ImportedAtUtc);
        Assert.All(
            typeof(StudentHold).GetProperties(BindingFlags.Instance | BindingFlags.Public),
            property => Assert.Null(property.SetMethod));
    }

    [Fact]
    public void Active_period_is_half_open_and_nullable_end_remains_active()
    {
        var bounded = CreateHold(effectiveToUtc: EffectiveToUtc);

        Assert.False(bounded.IsActiveAt(EffectiveFromUtc.AddTicks(-1)));
        Assert.True(bounded.IsActiveAt(EffectiveFromUtc));
        Assert.True(bounded.IsActiveAt(EffectiveToUtc.AddTicks(-1)));
        Assert.False(bounded.IsActiveAt(EffectiveToUtc));

        var unbounded = CreateHold(effectiveToUtc: null, blocksRegistration: false);
        Assert.False(unbounded.BlocksRegistration);
        Assert.Null(unbounded.EffectiveToUtc);
        Assert.True(unbounded.IsActiveAt(EffectiveFromUtc.AddYears(10)));
    }

    [Fact]
    public void Constructor_rejects_missing_identity_content_or_provenance_invalid_range_and_non_utc_instants()
    {
        Assert.Throws<ArgumentException>(() => CreateHold(id: Guid.Empty));
        Assert.Throws<ArgumentException>(() => CreateHold(studentId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => CreateHold(termId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => CreateHold(code: " "));
        Assert.Throws<ArgumentException>(() => CreateHold(message: " "));
        Assert.Throws<ArgumentException>(() => CreateHold(source: " "));
        Assert.Throws<ArgumentException>(() => CreateHold(sourceReference: " "));
        Assert.Throws<ArgumentException>(() =>
            CreateHold(effectiveToUtc: EffectiveFromUtc));
        Assert.Throws<ArgumentException>(() =>
            CreateHold(effectiveToUtc: EffectiveFromUtc.AddTicks(-1)));
        Assert.Throws<ArgumentException>(() => CreateHold(
            effectiveFromUtc: DateTime.SpecifyKind(EffectiveFromUtc, DateTimeKind.Unspecified)));
        Assert.Throws<ArgumentException>(() => CreateHold(
            effectiveToUtc: DateTime.SpecifyKind(EffectiveToUtc, DateTimeKind.Local)));
        Assert.Throws<ArgumentException>(() => CreateHold(
            importedAtUtc: DateTime.SpecifyKind(ImportedAtUtc, DateTimeKind.Unspecified)));
    }

    private static StudentHold CreateHold(
        Guid? id = null,
        Guid? studentId = null,
        Guid? termId = null,
        string code = "REGISTRATION-HOLD",
        string message = "Resolve the synthetic demo hold before registration.",
        bool blocksRegistration = true,
        DateTime? effectiveFromUtc = null,
        DateTime? effectiveToUtc = null,
        string source = "synthetic-hold-import",
        string sourceReference = "fixture-42/hold-1",
        DateTime? importedAtUtc = null) =>
        new(
            id ?? Guid.NewGuid(),
            studentId ?? Guid.NewGuid(),
            termId ?? Guid.NewGuid(),
            code,
            message,
            blocksRegistration,
            effectiveFromUtc ?? EffectiveFromUtc,
            effectiveToUtc,
            source,
            sourceReference,
            importedAtUtc ?? ImportedAtUtc);
}
