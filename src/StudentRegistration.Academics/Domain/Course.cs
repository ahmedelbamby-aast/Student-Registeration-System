namespace StudentRegistration.Academics.Domain;

public sealed class Course
{
    private Course()
    {
    }

    public Course(
        Guid id,
        Guid catalogueVersionId,
        string code,
        string title,
        decimal credits,
        bool isActive,
        CatalogueFieldProvenance provenance)
    {
        DomainValue.Identifier(id, nameof(id));
        DomainValue.Identifier(catalogueVersionId, nameof(catalogueVersionId));
        if (credits != 3m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(credits),
                "Every subject in the approved demo curriculum must be exactly three credits.");
        }

        Id = id;
        CatalogueVersionId = catalogueVersionId;
        Code = DomainValue.Code(code, nameof(code));
        Title = DomainValue.Required(title, nameof(title));
        Credits = credits;
        IsActive = isActive;
        Provenance = provenance ?? throw new ArgumentNullException(nameof(provenance));
    }

    public Guid Id { get; private set; }

    public Guid CatalogueVersionId { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public decimal Credits { get; private set; }

    public bool IsActive { get; private set; }

    public CatalogueFieldProvenance Provenance { get; private set; } = null!;

    public byte[] Version { get; private set; } = [];
}
