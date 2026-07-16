using StudentRegistration.Academics.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec009;

public sealed class CoursePrerequisiteModelTests
{
    [Fact]
    public void Prerequisite_preserves_version_relationship_and_provenance()
    {
        var edge = Create();

        Assert.NotEqual(Guid.Empty, edge.CatalogueVersionId);
        Assert.NotEqual(edge.CourseId, edge.RequiredCourseId);
        Assert.Equal("C", edge.MinimumGrade);
        Assert.Equal(CatalogueSourceKind.OfficialSource, edge.Provenance.SourceKind);
    }

    [Fact]
    public void Graph_validation_rejects_self_edges_missing_references_and_cycles()
    {
        var versionId = Guid.NewGuid();
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var missing = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() => Create(
            catalogueVersionId: versionId,
            courseId: a,
            requiredCourseId: a));
        Assert.Throws<InvalidOperationException>(() =>
            CoursePrerequisite.ValidateGraph(
                [a, b],
                [Create(versionId, a, missing)]));

        var exception = Assert.Throws<InvalidOperationException>(() =>
            CoursePrerequisite.ValidateGraph(
                [a, b],
                [Create(versionId, a, b), Create(versionId, b, a)]));
        Assert.Contains(a.ToString("D"), exception.Message, StringComparison.Ordinal);
        Assert.Contains(b.ToString("D"), exception.Message, StringComparison.Ordinal);
    }

    private static CoursePrerequisite Create(
        Guid? catalogueVersionId = null,
        Guid? courseId = null,
        Guid? requiredCourseId = null) =>
        new(
            catalogueVersionId ?? Guid.NewGuid(),
            courseId ?? Guid.NewGuid(),
            requiredCourseId ?? Guid.NewGuid(),
            "C",
            new CatalogueFieldProvenance(
                "SRC-DATA-SCIENCE",
                new DateOnly(2026, 7, 13),
                CatalogueSourceKind.OfficialSource,
                []));
}
