using StudentRegistration.Client.Features.Identity;
using StudentRegistration.Contracts.Identity;

namespace StudentRegistration.Client.UnitTests.Identity;

public sealed class IdentityImportContentHashTests
{
    [Fact]
    public void Canonical_hash_is_stable_across_safe_transport_normalization()
    {
        IdentityImportUserRequest[] entered =
        [
            new(
                " STAFF-1 ",
                " STAFF ",
                null,
                " lecturer.one ",
                " S-001 ",
                " Lecturer One ",
                ["TeachingAssistant", "Lecturer", "Lecturer"]),
            new(
                " STUDENT-1 ",
                " Student ",
                "ai-2026-001",
                null,
                null,
                " Demo Student ",
                [])
        ];
        IdentityImportUserRequest[] normalized =
        [
            new(
                "STAFF-1",
                "staff",
                null,
                "lecturer.one",
                "S-001",
                "Lecturer One",
                ["Lecturer", "TeachingAssistant"]),
            new(
                "STUDENT-1",
                "student",
                "AI-2026-001",
                null,
                null,
                "Demo Student",
                [])
        ];

        var enteredHash = IdentityImportContentHash.Compute(entered);
        var normalizedHash = IdentityImportContentHash.Compute(normalized);

        Assert.Equal(normalizedHash, enteredHash);
        Assert.StartsWith("SHA256:", enteredHash, StringComparison.Ordinal);
        Assert.Equal(71, enteredHash.Length);
    }
}
