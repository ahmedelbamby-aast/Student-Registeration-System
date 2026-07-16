using StudentRegistration.Academics.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec009;

public sealed class PolicyRuleModelTests
{
    [Theory]
    [InlineData(PolicyValueType.Number, "18")]
    [InlineData(PolicyValueType.Boolean, "true")]
    [InlineData(PolicyValueType.String, "Active")]
    [InlineData(PolicyValueType.StringList, "[\"Active\",\"Probation\"]")]
    public void Policy_rule_accepts_only_values_matching_the_declared_type(
        PolicyValueType valueType,
        string value)
    {
        var rule = Create(valueType, value);

        Assert.Equal(valueType, rule.ValueType);
        Assert.Equal(value, rule.Value);
        Assert.Equal("NORMAL_MAX_CREDITS", rule.Code);
        Assert.Equal("DEMO-APPROVAL-2026.1", rule.SourceReference);
        Assert.Equal(CatalogueSourceKind.SyntheticDemo, rule.SourceKind);
    }

    [Fact]
    public void Policy_rule_rejects_unknown_or_mismatched_typed_values()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Create((PolicyValueType)999, "18"));
        Assert.Throws<ArgumentException>(
            () => Create(PolicyValueType.Number, "eighteen"));
        Assert.Throws<ArgumentException>(
            () => Create(PolicyValueType.Boolean, "yes"));
        Assert.Throws<ArgumentException>(
            () => Create(PolicyValueType.StringList, "{}"));
        Assert.Throws<ArgumentException>(
            () => Create(PolicyValueType.String, " "));
    }

    private static PolicyRule Create(
        PolicyValueType valueType,
        string value) =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "NORMAL_MAX_CREDITS",
            "NORMAL_LOAD_EXCEEDED",
            valueType,
            value,
            "DEMO-APPROVAL-2026.1",
            CatalogueSourceKind.SyntheticDemo);
}
