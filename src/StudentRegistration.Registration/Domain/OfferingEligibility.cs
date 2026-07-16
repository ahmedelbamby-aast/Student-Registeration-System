using System.Collections.ObjectModel;

namespace StudentRegistration.Registration.Domain;

public sealed record OfferingEligibility
{
    public OfferingEligibility(
        Guid offeringId,
        string courseCode,
        string title,
        decimal credits,
        decimal currentPlanCredits,
        decimal projectedPlanCredits,
        decimal defaultTargetCredits,
        decimal maximumAllowedCredits,
        bool eligible,
        IReadOnlyList<EligibilityReason> reasons,
        IReadOnlyList<GroupSummary> groups,
        IReadOnlyDictionary<string, string> inputSummary,
        DateTime evaluatedAtUtc,
        string academicContextVersion,
        string catalogueVersion,
        Guid policySetId,
        string policyVersion,
        string offeringRowVersion,
        string currentPlanVersion)
    {
        if (offeringId == Guid.Empty)
        {
            throw new ArgumentException(
                "An offering identifier is required.",
                nameof(offeringId));
        }

        if (credits < 0m ||
            currentPlanCredits < 0m ||
            projectedPlanCredits < 0m ||
            defaultTargetCredits <= 0m ||
            maximumAllowedCredits <= 0m)
        {
            throw new ArgumentException("Credit projections must be non-negative.");
        }

        if (evaluatedAtUtc.Kind is not DateTimeKind.Utc)
        {
            throw new ArgumentException(
                "The evaluation instant must be UTC.",
                nameof(evaluatedAtUtc));
        }

        ArgumentNullException.ThrowIfNull(inputSummary);
        OfferingId = offeringId;
        CourseCode = EligibilityDomainGuard.Required(
            courseCode,
            nameof(courseCode));
        Title = EligibilityDomainGuard.Required(title, nameof(title));
        Credits = credits;
        CurrentPlanCredits = currentPlanCredits;
        ProjectedPlanCredits = projectedPlanCredits;
        DefaultTargetCredits = defaultTargetCredits;
        MaximumAllowedCredits = maximumAllowedCredits;
        Eligible = eligible;
        Reasons = EligibilityDomainGuard.Copy(reasons, nameof(reasons));
        Groups = EligibilityDomainGuard.Copy(groups, nameof(groups));
        InputSummary = new ReadOnlyDictionary<string, string>(
            inputSummary.ToDictionary(
                pair => EligibilityDomainGuard.Required(pair.Key, nameof(inputSummary)),
                pair => EligibilityDomainGuard.Required(pair.Value, nameof(inputSummary)),
                StringComparer.Ordinal));
        EvaluatedAtUtc = evaluatedAtUtc;
        AcademicContextVersion = EligibilityDomainGuard.Required(
            academicContextVersion,
            nameof(academicContextVersion));
        CatalogueVersion = EligibilityDomainGuard.Required(
            catalogueVersion,
            nameof(catalogueVersion));
        PolicySetId = policySetId;
        PolicyVersion = EligibilityDomainGuard.Required(
            policyVersion,
            nameof(policyVersion));
        OfferingRowVersion = EligibilityDomainGuard.Required(
            offeringRowVersion,
            nameof(offeringRowVersion));
        CurrentPlanVersion = EligibilityDomainGuard.Required(
            currentPlanVersion,
            nameof(currentPlanVersion));
    }

    public Guid OfferingId { get; }

    public string CourseCode { get; }

    public string Title { get; }

    public decimal Credits { get; }

    public decimal CurrentPlanCredits { get; }

    public decimal ProjectedPlanCredits { get; }

    public decimal DefaultTargetCredits { get; }

    public decimal MaximumAllowedCredits { get; }

    public bool Eligible { get; }

    public IReadOnlyList<EligibilityReason> Reasons { get; }

    public IReadOnlyList<GroupSummary> Groups { get; }

    public IReadOnlyDictionary<string, string> InputSummary { get; }

    public DateTime EvaluatedAtUtc { get; }

    public string AcademicContextVersion { get; }

    public string CatalogueVersion { get; }

    public Guid PolicySetId { get; }

    public string PolicyVersion { get; }

    public string OfferingRowVersion { get; }

    public string CurrentPlanVersion { get; }

    public bool Equals(OfferingEligibility? other) =>
        other is not null &&
        OfferingId == other.OfferingId &&
        string.Equals(CourseCode, other.CourseCode, StringComparison.Ordinal) &&
        string.Equals(Title, other.Title, StringComparison.Ordinal) &&
        Credits == other.Credits &&
        CurrentPlanCredits == other.CurrentPlanCredits &&
        ProjectedPlanCredits == other.ProjectedPlanCredits &&
        DefaultTargetCredits == other.DefaultTargetCredits &&
        MaximumAllowedCredits == other.MaximumAllowedCredits &&
        Eligible == other.Eligible &&
        Reasons.SequenceEqual(other.Reasons) &&
        Groups.SequenceEqual(other.Groups) &&
        InputSummary.Count == other.InputSummary.Count &&
        InputSummary.All(pair =>
            other.InputSummary.TryGetValue(pair.Key, out var value) &&
            string.Equals(pair.Value, value, StringComparison.Ordinal)) &&
        EvaluatedAtUtc == other.EvaluatedAtUtc &&
        string.Equals(
            AcademicContextVersion,
            other.AcademicContextVersion,
            StringComparison.Ordinal) &&
        string.Equals(
            CatalogueVersion,
            other.CatalogueVersion,
            StringComparison.Ordinal) &&
        PolicySetId == other.PolicySetId &&
        string.Equals(PolicyVersion, other.PolicyVersion, StringComparison.Ordinal) &&
        string.Equals(
            OfferingRowVersion,
            other.OfferingRowVersion,
            StringComparison.Ordinal) &&
        string.Equals(
            CurrentPlanVersion,
            other.CurrentPlanVersion,
            StringComparison.Ordinal);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(OfferingId);
        hash.Add(CourseCode, StringComparer.Ordinal);
        hash.Add(Title, StringComparer.Ordinal);
        hash.Add(Credits);
        hash.Add(CurrentPlanCredits);
        hash.Add(ProjectedPlanCredits);
        hash.Add(DefaultTargetCredits);
        hash.Add(MaximumAllowedCredits);
        hash.Add(Eligible);
        foreach (var reason in Reasons)
        {
            hash.Add(reason);
        }

        foreach (var group in Groups)
        {
            hash.Add(group);
        }

        foreach (var pair in InputSummary.OrderBy(
                     pair => pair.Key,
                     StringComparer.Ordinal))
        {
            hash.Add(pair.Key, StringComparer.Ordinal);
            hash.Add(pair.Value, StringComparer.Ordinal);
        }

        hash.Add(EvaluatedAtUtc);
        hash.Add(AcademicContextVersion, StringComparer.Ordinal);
        hash.Add(CatalogueVersion, StringComparer.Ordinal);
        hash.Add(PolicySetId);
        hash.Add(PolicyVersion, StringComparer.Ordinal);
        hash.Add(OfferingRowVersion, StringComparer.Ordinal);
        hash.Add(CurrentPlanVersion, StringComparer.Ordinal);
        return hash.ToHashCode();
    }
}
