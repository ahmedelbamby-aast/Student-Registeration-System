namespace StudentRegistration.Registration.Domain;

public sealed record RegistrationPlanSelection(
    Guid ItemId,
    Guid OfferingId,
    Guid GroupId,
    string OfferingVersion,
    string GroupVersion);

public sealed class RegistrationPlanItem
{
    private RegistrationPlanItem()
    {
    }

    internal RegistrationPlanItem(
        Guid id,
        Guid planId,
        Guid offeringId,
        Guid selectedGroupId,
        string capturedOfferingVersion,
        string capturedGroupVersion)
    {
        RegistrationPlanDomainGuard.Identifier(id, nameof(id));
        RegistrationPlanDomainGuard.Identifier(planId, nameof(planId));
        RegistrationPlanDomainGuard.Identifier(offeringId, nameof(offeringId));
        RegistrationPlanDomainGuard.Identifier(
            selectedGroupId,
            nameof(selectedGroupId));

        Id = id;
        PlanId = planId;
        OfferingId = offeringId;
        SelectedGroupId = selectedGroupId;
        CapturedOfferingVersion = RegistrationPlanDomainGuard.Required(
            capturedOfferingVersion,
            nameof(capturedOfferingVersion));
        CapturedGroupVersion = RegistrationPlanDomainGuard.Required(
            capturedGroupVersion,
            nameof(capturedGroupVersion));
    }

    public Guid Id { get; private set; }

    public Guid PlanId { get; private set; }

    public Guid OfferingId { get; private set; }

    public Guid SelectedGroupId { get; private set; }

    public string CapturedOfferingVersion { get; private set; } = string.Empty;

    public string CapturedGroupVersion { get; private set; } = string.Empty;
}
