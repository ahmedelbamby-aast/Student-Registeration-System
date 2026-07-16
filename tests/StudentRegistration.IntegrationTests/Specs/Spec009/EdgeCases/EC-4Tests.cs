using StudentRegistration.Academics.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec009.EdgeCases;

public sealed class EC_4Tests
{
    [Fact]
    public void Unknown_or_mismatched_typed_rule_is_rejected_before_storage()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PolicyRule(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "UNKNOWN",
            "UNKNOWN_RULE_TYPE",
            (PolicyValueType)999,
            "value",
            "source",
            CatalogueSourceKind.SyntheticDemo));
        Assert.Throws<ArgumentException>(() => new PolicyRule(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "NORMAL_MAX_CREDITS",
            "NORMAL_LOAD_EXCEEDED",
            PolicyValueType.Number,
            "eighteen",
            "source",
            CatalogueSourceKind.SyntheticDemo));
    }
}
