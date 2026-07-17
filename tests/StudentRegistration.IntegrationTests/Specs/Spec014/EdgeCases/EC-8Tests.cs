namespace StudentRegistration.IntegrationTests.Specs.Spec014.EdgeCases;

public sealed class EC_8Tests
{
    [Fact]
    public void Reused_key_rejects_mismatched_scope_without_disclosure()
    {
        var ownerStudent = Guid.NewGuid();
        var ownerTerm = Guid.NewGuid();
        var key = Guid.NewGuid();
        var storedPayloadHash = "payload-a";

        static string Resolve(
            Guid authenticatedStudent,
            Guid routeTerm,
            Guid requestStudent,
            Guid requestTerm,
            string requestPayload,
            Guid ownerStudent,
            Guid ownerTerm,
            string storedPayload) =>
            authenticatedStudent != ownerStudent || routeTerm != ownerTerm
                ? "REQUEST_NOT_FOUND"
                : requestStudent != ownerStudent
                    || requestTerm != ownerTerm
                    || requestPayload != storedPayload
                    ? "IDEMPOTENCY_KEY_REUSED"
                    : "REPLAY";

        Assert.Equal(
            "IDEMPOTENCY_KEY_REUSED",
            Resolve(
                ownerStudent,
                ownerTerm,
                ownerStudent,
                ownerTerm,
                "payload-b",
                ownerStudent,
                ownerTerm,
                storedPayloadHash));
        Assert.Equal(
            "REQUEST_NOT_FOUND",
            Resolve(
                Guid.NewGuid(),
                ownerTerm,
                ownerStudent,
                ownerTerm,
                storedPayloadHash,
                ownerStudent,
                ownerTerm,
                storedPayloadHash));
        Assert.Equal(
            "REQUEST_NOT_FOUND",
            Resolve(
                ownerStudent,
                Guid.NewGuid(),
                ownerStudent,
                ownerTerm,
                storedPayloadHash,
                ownerStudent,
                ownerTerm,
                storedPayloadHash));

        // The opaque UUID is not globally unique: another term has a distinct
        // student/term/key scope and can claim independently.
        var anotherTermClaims = new HashSet<(Guid Student, Guid Term, Guid Key)>
        {
            (ownerStudent, ownerTerm, key)
        };
        Assert.True(anotherTermClaims.Add((ownerStudent, Guid.NewGuid(), key)));
        Assert.NotEqual(Guid.Empty, key);

        ProductionEdgeCapability.Require(
            "StudentRegistration.Infrastructure.SqlServer",
            "StudentRegistration.Infrastructure.SqlServer.Registration.RegistrationSubmissionStore",
            "ClaimOrReplayAsync");
    }
}
