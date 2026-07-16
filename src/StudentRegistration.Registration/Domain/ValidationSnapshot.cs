using System.Collections.ObjectModel;

namespace StudentRegistration.Registration.Domain;

public sealed record ValidationSnapshot
{
    public ValidationSnapshot(
        DateTime evaluatedAtUtc,
        string academicContextVersion,
        string policyVersion,
        string catalogueVersion,
        IReadOnlyDictionary<Guid, string> offeringVersions,
        IReadOnlyDictionary<Guid, string> groupVersions)
    {
        if (evaluatedAtUtc.Kind is not DateTimeKind.Utc)
        {
            throw new ArgumentException(
                "The validation instant must be UTC.",
                nameof(evaluatedAtUtc));
        }

        EvaluatedAtUtc = evaluatedAtUtc;
        AcademicContextVersion = RegistrationPlanDomainGuard.Required(
            academicContextVersion,
            nameof(academicContextVersion));
        PolicyVersion = RegistrationPlanDomainGuard.Required(
            policyVersion,
            nameof(policyVersion));
        CatalogueVersion = RegistrationPlanDomainGuard.Required(
            catalogueVersion,
            nameof(catalogueVersion));
        OfferingVersions = CopyVersions(
            offeringVersions,
            nameof(offeringVersions));
        GroupVersions = CopyVersions(groupVersions, nameof(groupVersions));
    }

    public DateTime EvaluatedAtUtc { get; }

    public string AcademicContextVersion { get; }

    public string PolicyVersion { get; }

    public string CatalogueVersion { get; }

    public IReadOnlyDictionary<Guid, string> OfferingVersions { get; }

    public IReadOnlyDictionary<Guid, string> GroupVersions { get; }

    private static IReadOnlyDictionary<Guid, string> CopyVersions(
        IReadOnlyDictionary<Guid, string> versions,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(versions, parameterName);
        var copy = new Dictionary<Guid, string>();
        foreach (var pair in versions)
        {
            RegistrationPlanDomainGuard.Identifier(pair.Key, parameterName);
            copy.Add(
                pair.Key,
                RegistrationPlanDomainGuard.Required(pair.Value, parameterName));
        }

        return new ReadOnlyDictionary<Guid, string>(copy);
    }
}
