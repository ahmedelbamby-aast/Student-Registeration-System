namespace StudentRegistration.SpecificationTests.Specs.Spec005.Entities;

public sealed class ApplicationUserErdContractTests
{
    [Fact]
    public void Reference_declares_owner_source_fields_and_invariants() =>
        ErdReferenceContractAssertions.AssertReference(
            "specs/005-erd-data-lifecycle/contracts/entities/ApplicationUser.md",
            new(
                "ApplicationUser",
                "APPLICATION_USER",
                "SPEC-007",
                "src/StudentRegistration.IdentityAccess/Domain/ApplicationUser.cs",
                [
                    "uniqueidentifier Id PK", "string UserName UK",
                    "string NormalizedUserName UK", "string UniversityId UK",
                    "string PasswordHash", "bool IsEnabled", "rowversion Version"
                ],
                [
                    "APPLICATION_USER ||--o| STUDENT : has_academic_profile",
                    "APPLICATION_USER ||--o| STAFF : represents",
                    "Unique filtered normalized ApplicationUser.UniversityId for student identities",
                    "rowversion on mutable aggregate roots and admin records"
                ]));
}
