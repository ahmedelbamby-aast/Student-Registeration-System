namespace StudentRegistration.IdentityAccess.Domain;

public sealed class Staff
{
    private Staff()
    {
    }

    public Staff(
        Guid id,
        Guid applicationUserId,
        string staffNumber,
        string displayName,
        bool isActive = true)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A staff identifier is required.", nameof(id));
        }

        if (applicationUserId == Guid.Empty)
        {
            throw new ArgumentException("An application user is required.", nameof(applicationUserId));
        }

        Id = id;
        ApplicationUserId = applicationUserId;
        StaffNumber = IdentityDomainGuard.Required(staffNumber, nameof(staffNumber), 50);
        DisplayName = IdentityDomainGuard.Required(displayName, nameof(displayName), 200);
        IsActive = isActive;
    }

    public Guid Id { get; private set; }
    public Guid ApplicationUserId { get; private set; }
    public string StaffNumber { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    public void SetActive(bool active) => IsActive = active;
}
