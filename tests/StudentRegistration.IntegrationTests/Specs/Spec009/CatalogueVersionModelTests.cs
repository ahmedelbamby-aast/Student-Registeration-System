using StudentRegistration.Academics.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec009;

public sealed class CatalogueVersionModelTests
{
    private static readonly DateTime EffectiveFromUtc =
        new(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime PublishedAtUtc =
        new(2026, 7, 16, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Catalogue_version_is_immutable_scoped_and_supersedable()
    {
        var sourceDraftId = Guid.NewGuid();
        var priorId = Guid.NewGuid();
        var version = new CatalogueVersion(
            Guid.NewGuid(),
            sourceDraftId,
            priorId,
            "AI-DS",
            "2026.1",
            "SRC-DATA-SCIENCE",
            EffectiveFromUtc,
            PublishedAtUtc,
            "admin-1",
            CatalogueVersionState.Published);

        Assert.Equal(sourceDraftId, version.SourceDraftId);
        Assert.Equal(priorId, version.SupersedesId);
        Assert.Equal("AI-DS", version.ScopeCode);
        Assert.Equal("2026.1", version.VersionCode);
        Assert.Equal(CatalogueVersionState.Published, version.State);
        Assert.Empty(version.Version);

        version.MarkSuperseded();
        Assert.Equal(CatalogueVersionState.Superseded, version.State);
        Assert.Throws<InvalidOperationException>(version.MarkSuperseded);
    }

    [Fact]
    public void Catalogue_version_requires_identity_scope_source_actor_and_utc_dates()
    {
        Assert.Throws<ArgumentException>(() => Create(id: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(sourceDraftId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(scopeCode: " "));
        Assert.Throws<ArgumentException>(() => Create(versionCode: " "));
        Assert.Throws<ArgumentException>(() => Create(sourceReference: " "));
        Assert.Throws<ArgumentException>(() => Create(publishedBy: " "));
        Assert.Throws<ArgumentException>(() => Create(
            publishedAtUtc: DateTime.SpecifyKind(PublishedAtUtc, DateTimeKind.Unspecified)));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Create(state: (CatalogueVersionState)999));
    }

    private static CatalogueVersion Create(
        Guid? id = null,
        Guid? sourceDraftId = null,
        string scopeCode = "AI-DS",
        string versionCode = "2026.1",
        string sourceReference = "SRC-DATA-SCIENCE",
        string publishedBy = "admin-1",
        DateTime? publishedAtUtc = null,
        CatalogueVersionState state = CatalogueVersionState.Published) =>
        new(
            id ?? Guid.NewGuid(),
            sourceDraftId ?? Guid.NewGuid(),
            null,
            scopeCode,
            versionCode,
            sourceReference,
            EffectiveFromUtc,
            publishedAtUtc ?? PublishedAtUtc,
            publishedBy,
            state);
}
