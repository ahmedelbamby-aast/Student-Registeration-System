using StudentRegistration.Academics.Domain;
using StudentRegistration.Contracts;

namespace StudentRegistration.IntegrationTests.Specs.Spec008;

public sealed class AcademicTermModelTests
{
    [Fact]
    public void Constructor_preserves_the_approved_term_fields_and_server_owned_state()
    {
        var id = Guid.NewGuid();
        var creationRequestId = Guid.NewGuid();
        var startsOn = new DateOnly(2026, 9, 20);
        var endsOn = new DateOnly(2027, 1, 15);

        var term = new AcademicTerm(
            id,
            "2026-FALL",
            creationRequestId,
            "sha256:canonical-create-payload",
            "Fall 2026",
            startsOn,
            endsOn,
            "Africa/Cairo",
            TermState.Draft);

        Assert.Equal(id, term.Id);
        Assert.Equal("2026-FALL", term.Code);
        Assert.Equal(creationRequestId, term.CreationClientRequestId);
        Assert.Equal("sha256:canonical-create-payload", term.CreationPayloadHash);
        Assert.Equal("Fall 2026", term.DisplayName);
        Assert.Equal(startsOn, term.TeachingStartsOn);
        Assert.Equal(endsOn, term.TeachingEndsOn);
        Assert.Equal("Africa/Cairo", term.TimeZoneId);
        Assert.Equal(TermState.Draft, term.State);
        Assert.Empty(term.Version);
    }

    [Fact]
    public void Constructor_requires_identifiers_text_a_valid_date_range_timezone_and_state()
    {
        Assert.Throws<ArgumentException>(() => Create(id: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(creationRequestId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(code: " "));
        Assert.Throws<ArgumentException>(() => Create(creationPayloadHash: " "));
        Assert.Throws<ArgumentException>(() => Create(displayName: " "));
        Assert.Throws<ArgumentException>(() => Create(timeZoneId: " "));
        Assert.Throws<ArgumentException>(() => Create(timeZoneId: "Not/A-Time-Zone"));
        Assert.Throws<ArgumentException>(() => Create(
            startsOn: new DateOnly(2026, 9, 20),
            endsOn: new DateOnly(2026, 9, 20)));
        Assert.Throws<ArgumentException>(() => Create(
            startsOn: new DateOnly(2026, 9, 21),
            endsOn: new DateOnly(2026, 9, 20)));
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(state: (TermState)999));
    }

    [Fact]
    public void Creation_replay_metadata_and_identity_are_not_publicly_mutable()
    {
        AssertPrivateSetter(nameof(AcademicTerm.Id));
        AssertPrivateSetter(nameof(AcademicTerm.CreationClientRequestId));
        AssertPrivateSetter(nameof(AcademicTerm.CreationPayloadHash));
        AssertPrivateSetter(nameof(AcademicTerm.Version));
    }

    private static AcademicTerm Create(
        Guid? id = null,
        string code = "2026-FALL",
        Guid? creationRequestId = null,
        string creationPayloadHash = "sha256:canonical-create-payload",
        string displayName = "Fall 2026",
        DateOnly? startsOn = null,
        DateOnly? endsOn = null,
        string timeZoneId = "Africa/Cairo",
        TermState state = TermState.Draft) =>
        new(
            id ?? Guid.NewGuid(),
            code,
            creationRequestId ?? Guid.NewGuid(),
            creationPayloadHash,
            displayName,
            startsOn ?? new DateOnly(2026, 9, 20),
            endsOn ?? new DateOnly(2027, 1, 15),
            timeZoneId,
            state);

    private static void AssertPrivateSetter(string propertyName)
    {
        var property = typeof(AcademicTerm).GetProperty(propertyName);
        Assert.NotNull(property);
        Assert.False(property.SetMethod?.IsPublic ?? false);
    }
}
