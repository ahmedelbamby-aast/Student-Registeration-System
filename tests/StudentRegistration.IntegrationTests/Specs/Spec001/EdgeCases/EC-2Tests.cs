namespace StudentRegistration.IntegrationTests.Specs.Spec001.EdgeCases;

public sealed class EC_2Tests
{
    [Fact(Skip = "Pending SPEC-007 no-supported-role session implementation.")]
    public void Staff_identity_without_supported_role_is_denied_safely()
    {
        // Future integration fixture asserts the stable no-role response.
    }
}
