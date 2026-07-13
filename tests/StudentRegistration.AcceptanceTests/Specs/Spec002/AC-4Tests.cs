using StudentRegistration.TestSupport.Spec002;

namespace StudentRegistration.AcceptanceTests.Specs.Spec002;

public sealed class AC_4Tests
{
    [Fact]
    public void Unknown_rule_types_and_executable_expressions_are_rejected_without_storage()
    {
        var harness = Spec002PolicyTestHarness.Load();

        var unknown = harness.ValidatePublication(new PolicyPublicationDraft
        {
            RuleType = "UserDefinedExpression"
        });
        var executable = harness.ValidatePublication(new PolicyPublicationDraft
        {
            ExecutableExpression = "student.Gpa >= 2.0"
        });

        Assert.False(unknown.Accepted);
        Assert.Contains("UNKNOWN_RULE_TYPE", unknown.RejectionCodes);
        Assert.False(unknown.StoredExecutableContent);
        Assert.False(executable.Accepted);
        Assert.Contains("EXECUTABLE_POLICY_CONTENT_FORBIDDEN", executable.RejectionCodes);
        Assert.False(executable.StoredExecutableContent);
    }
}
