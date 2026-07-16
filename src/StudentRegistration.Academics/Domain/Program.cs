namespace StudentRegistration.Academics.Domain;

public sealed class Program
{
    private Program()
    {
    }

    public Program(
        Guid id,
        Guid catalogueVersionId,
        string code,
        string displayName,
        bool isActive,
        CatalogueFieldProvenance provenance)
    {
        DomainValue.Identifier(id, nameof(id));
        DomainValue.Identifier(catalogueVersionId, nameof(catalogueVersionId));

        Id = id;
        CatalogueVersionId = catalogueVersionId;
        Code = DomainValue.Code(code, nameof(code));
        DisplayName = DomainValue.Required(displayName, nameof(displayName));
        IsActive = isActive;
        Provenance = provenance ?? throw new ArgumentNullException(nameof(provenance));
    }

    public Guid Id { get; private set; }

    public Guid CatalogueVersionId { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public CatalogueFieldProvenance Provenance { get; private set; } = null!;

    public byte[] Version { get; private set; } = [];
}
