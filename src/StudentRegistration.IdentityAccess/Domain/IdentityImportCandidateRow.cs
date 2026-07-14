namespace StudentRegistration.IdentityAccess.Domain;

/// <summary>
/// Immutable, normalized staging child owned by one identity import batch.
/// It contains only fields needed to pre-provision an account and role set.
/// </summary>
public sealed class IdentityImportCandidateRow
{
    private static readonly HashSet<string> StaffRoles =
        new(StringComparer.Ordinal)
        {
            "Admin",
            "Lecturer",
            "TeachingAssistant"
        };

    private IdentityImportCandidateRow()
    {
    }

    public IdentityImportCandidateRow(
        Guid id,
        Guid identityImportBatchId,
        int ordinal,
        string externalReference,
        string kind,
        string? universityId,
        string? userName,
        string? staffNumber,
        string displayName,
        IReadOnlyList<string> roles)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A candidate-row identifier is required.", nameof(id));
        }

        if (identityImportBatchId == Guid.Empty)
        {
            throw new ArgumentException("An import batch is required.", nameof(identityImportBatchId));
        }

        if (ordinal is < 1 or > 500)
        {
            throw new ArgumentOutOfRangeException(
                nameof(ordinal),
                "The candidate ordinal must be between 1 and 500.");
        }

        ArgumentNullException.ThrowIfNull(roles);
        var normalizedKind = IdentityDomainGuard.Required(kind, nameof(kind), 20)
            .ToLowerInvariant();
        var normalizedRoles = roles
            .Select(role => IdentityDomainGuard.Required(role, nameof(roles), 50))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        if (normalizedKind == "student")
        {
            if (normalizedRoles.Length != 1 || normalizedRoles[0] != "Student")
            {
                throw new ArgumentException(
                    "A student candidate requires only the server-derived Student role.",
                    nameof(roles));
            }

            UniversityId = IdentityDomainGuard.Required(
                universityId!,
                nameof(universityId),
                50).ToUpperInvariant();
            if (!string.IsNullOrWhiteSpace(userName) || !string.IsNullOrWhiteSpace(staffNumber))
            {
                throw new ArgumentException(
                    "A student candidate cannot contain staff identifiers.",
                    nameof(userName));
            }
        }
        else if (normalizedKind == "staff")
        {
            if (normalizedRoles.Length == 0 || normalizedRoles.Any(role => !StaffRoles.Contains(role)))
            {
                throw new ArgumentException(
                    "A staff candidate requires an allow-listed staff role.",
                    nameof(roles));
            }

            if (!string.IsNullOrWhiteSpace(universityId))
            {
                throw new ArgumentException(
                    "A staff candidate cannot contain a University ID.",
                    nameof(universityId));
            }

            UserName = IdentityDomainGuard.Required(userName!, nameof(userName), 200);
            StaffNumber = IdentityDomainGuard.Required(
                staffNumber!,
                nameof(staffNumber),
                50);
        }
        else
        {
            throw new ArgumentException("Candidate kind must be student or staff.", nameof(kind));
        }

        Id = id;
        IdentityImportBatchId = identityImportBatchId;
        Ordinal = ordinal;
        ExternalReference = IdentityDomainGuard.Required(
            externalReference,
            nameof(externalReference),
            100);
        Kind = normalizedKind;
        DisplayName = IdentityDomainGuard.Required(displayName, nameof(displayName), 200);
        Roles = string.Join(',', normalizedRoles);
    }

    public Guid Id { get; private set; }
    public Guid IdentityImportBatchId { get; private set; }
    public int Ordinal { get; private set; }
    public string ExternalReference { get; private set; } = string.Empty;
    public string Kind { get; private set; } = string.Empty;
    public string? UniversityId { get; private set; }
    public string? UserName { get; private set; }
    public string? StaffNumber { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public string Roles { get; private set; } = string.Empty;
}
