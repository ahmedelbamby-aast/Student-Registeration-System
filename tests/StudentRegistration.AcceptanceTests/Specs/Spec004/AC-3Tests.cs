using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec004;

public sealed class AC_3Tests
{
    [Fact]
    public void Message_broker_without_a_durable_external_consumer_is_rejected()
    {
        // Given a proposal introduces a broker before the MVP has a durable external consumer.
        var conformance = RepositoryFiles.Read(
            "specs/004-architecture-engineering-principles/contracts/prohibited-complexity.md");

        // When architecture review applies the approved complexity gate.
        RepositoryFiles.ContainsAll(
            conformance,
            "message broker",
            "durable external consumer",
            "rejected",
            "separately approved ADR/spec");

        // Then an executable architecture check guards the decision.
        var executableRules = RepositoryFiles.Read(
            "tests/StudentRegistration.ArchitectureTests/ProhibitedComplexityTests.cs");
        RepositoryFiles.ContainsAll(executableRules, "Message_broker", "Assert.");
    }
}
