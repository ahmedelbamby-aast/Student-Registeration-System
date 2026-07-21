namespace StudentRegistration.Contracts.Identity;

public sealed record StudentLoginRequest(string UniversityId, string Password);

public sealed record StaffLoginRequest(string UserName, string Password);

public sealed record ActivateStudentRequest(
    string UniversityId,
    string InitialPassword,
    string NewPassword);

public sealed record SessionDto(
    string DisplayName,
    IReadOnlyList<string> Roles,
    string? ActiveRole,
    string SessionState,
    DateTime ExpiresAtUtc);

public sealed record RecoveryRequest(string UniversityIdOrUserName);

public sealed record RecoveryCompleteRequest(string ChallengeToken, string NewPassword);

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
