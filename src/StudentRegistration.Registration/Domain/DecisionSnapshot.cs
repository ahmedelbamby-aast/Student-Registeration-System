using System.Collections.ObjectModel;

namespace StudentRegistration.Registration.Domain;

public sealed record DecisionSnapshot
{
    public DecisionSnapshot(
        string policyVersion,
        string academicContextVersion,
        string planVersion,
        IReadOnlyDictionary<Guid, string> groupVersions,
        string resultCode)
    {
        PolicyVersion = RegistrationPlanDomainGuard.Required(
            policyVersion,
            nameof(policyVersion));
        AcademicContextVersion = RegistrationPlanDomainGuard.Required(
            academicContextVersion,
            nameof(academicContextVersion));
        PlanVersion = RegistrationPlanDomainGuard.Required(
            planVersion,
            nameof(planVersion));
        GroupVersions = CopyVersions(groupVersions, nameof(groupVersions));
        ResultCode = RegistrationPlanDomainGuard.Required(
            resultCode,
            nameof(resultCode));
    }

    public string PolicyVersion { get; }

    public string AcademicContextVersion { get; }

    public string PlanVersion { get; }

    public IReadOnlyDictionary<Guid, string> GroupVersions { get; }

    public string ResultCode { get; }

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
