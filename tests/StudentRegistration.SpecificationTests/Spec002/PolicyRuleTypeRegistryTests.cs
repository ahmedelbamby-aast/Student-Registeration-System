using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Spec002;

public sealed class PolicyRuleTypeRegistryTests
{
    [Fact]
    public void Registry_contains_only_the_nine_reviewed_demo_rule_types()
    {
        var registryText = RepositoryFiles.Read(
            "specs/002-aastmt-policy-rulebook/schemas/policy-rule-types.json");
        using var registry = JsonDocument.Parse(registryText);
        var types = registry.RootElement.GetProperty("types").EnumerateArray().ToArray();

        Assert.Equal(9, types.Length);
        Assert.Equal(
            new[]
            {
                "RegistrationWindow", "AcademicStanding", "BlockingHold",
                "Prerequisite", "CreditLoad", "ProbationLoad",
                "RepeatEligibility", "Capacity", "MeetingConflict"
            },
            types.Select(type => type.GetProperty("typeKey").GetString()).ToArray());
    }

    [Fact]
    public void Registry_rejects_unknown_or_executable_content_and_keeps_runtime_in_spec_009()
    {
        var registry = RepositoryFiles.Read(
            "specs/002-aastmt-policy-rulebook/schemas/policy-rule-types.json");

        RepositoryFiles.ContainsAll(
            registry,
            "\"runtimeOwner\": \"SPEC-009\"",
            "\"unknownTypeOutcome\": \"Reject\"",
            "\"executableContentOutcome\": \"RejectWithoutStoreOrRun\"",
            "policy-rule-definition.schema.json");
        Assert.DoesNotContain("\"expression\"", registry, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"script\"", registry, StringComparison.OrdinalIgnoreCase);
    }
}
