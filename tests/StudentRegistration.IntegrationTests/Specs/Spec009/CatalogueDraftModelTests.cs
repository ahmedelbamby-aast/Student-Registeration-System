using StudentRegistration.Academics.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec009;

public sealed class CatalogueDraftModelTests
{
    [Fact]
    public void Catalogue_draft_has_explicit_editable_lifecycle_content_hash_and_rowversion()
    {
        var draft = Create();

        Assert.Equal("AI-DS", draft.ScopeCode);
        Assert.Equal(CatalogueDraftState.Editing, draft.State);
        Assert.Equal("HASH-1", draft.CanonicalContentHash);
        Assert.Empty(draft.Version);

        draft.MarkValidated("VALID");
        Assert.Equal(CatalogueDraftState.Validated, draft.State);
        draft.ReplaceContent("HASH-2", "{\"courses\":19}");
        Assert.Equal(CatalogueDraftState.Editing, draft.State);
        Assert.Equal("HASH-2", draft.CanonicalContentHash);
        Assert.Equal(string.Empty, draft.ValidationSummaryJson);
        draft.MarkValidated("VALID");
        draft.MarkPublished();
        Assert.Equal(CatalogueDraftState.Published, draft.State);
        Assert.Throws<InvalidOperationException>(
            () => draft.ReplaceContent("HASH-3", "{}"));
    }

    [Fact]
    public void Catalogue_draft_requires_identity_scope_and_content()
    {
        Assert.Throws<ArgumentException>(() => Create(id: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(scopeCode: " "));
        Assert.Throws<ArgumentException>(() => Create(contentHash: " "));
        Assert.Throws<ArgumentException>(() => Create(contentJson: " "));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Create(state: (CatalogueDraftState)999));
    }

    private static CatalogueDraft Create(
        Guid? id = null,
        string scopeCode = "AI-DS",
        string contentHash = "HASH-1",
        string contentJson = "{\"courses\":19}",
        CatalogueDraftState state = CatalogueDraftState.Editing) =>
        new(
            id ?? Guid.NewGuid(),
            scopeCode,
            null,
            contentHash,
            contentJson,
            string.Empty,
            state);
}
