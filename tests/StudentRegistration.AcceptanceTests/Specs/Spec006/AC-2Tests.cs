namespace StudentRegistration.AcceptanceTests.Specs.Spec006;

public sealed class AC_2Tests
{
    private const string DeferredReason =
        "Deferred to SPEC-010: activate only after an approved, version-pinned EF Core/SQL Server offering endpoint and its SPEC-010-owned response DTO are implemented in the real API host.";

    [Fact(Skip = DeferredReason)]
    public void Endpoint_serializes_only_declared_dto_fields_and_never_an_ef_entity_graph()
    {
        // Given a real EF entity with an internal rowversion and navigation graph.
        // When its approved downstream endpoint returns the feature-owned DTO over HTTP.
        // Then only fields declared by that DTO contract are serialized.
        // And the rowversion and navigation graph remain internal unless an explicitly authorized DTO field exposes a version value.
        throw new NotImplementedException(
            "Exercise an approved EF-backed endpoint and its owned DTO; an in-memory object or mock serializer cannot prove AC-2.");
    }
}
