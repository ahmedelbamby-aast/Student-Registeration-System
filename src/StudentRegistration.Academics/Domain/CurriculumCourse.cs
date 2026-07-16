namespace StudentRegistration.Academics.Domain;

public sealed class CurriculumCourse
{
    private CurriculumCourse()
    {
    }

    public CurriculumCourse(
        Guid catalogueVersionId,
        Guid programId,
        Guid courseId,
        int level,
        int? recommendedTerm,
        bool isRequired,
        string? cohortScope,
        CatalogueFieldProvenance provenance)
    {
        DomainValue.Identifier(catalogueVersionId, nameof(catalogueVersionId));
        DomainValue.Identifier(programId, nameof(programId));
        DomainValue.Identifier(courseId, nameof(courseId));
        if (level < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(level));
        }

        if (recommendedTerm is < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(recommendedTerm));
        }

        CatalogueVersionId = catalogueVersionId;
        ProgramId = programId;
        CourseId = courseId;
        Level = level;
        RecommendedTerm = recommendedTerm;
        IsRequired = isRequired;
        CohortScope = cohortScope is null
            ? null
            : DomainValue.Required(cohortScope, nameof(cohortScope));
        Provenance = provenance ?? throw new ArgumentNullException(nameof(provenance));
    }

    public Guid CatalogueVersionId { get; private set; }

    public Guid ProgramId { get; private set; }

    public Guid CourseId { get; private set; }

    public int Level { get; private set; }

    public int? RecommendedTerm { get; private set; }

    public bool IsRequired { get; private set; }

    public string? CohortScope { get; private set; }

    public CatalogueFieldProvenance Provenance { get; private set; } = null!;
}
