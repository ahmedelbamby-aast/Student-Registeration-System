namespace StudentRegistration.AcceptanceTests.Specs.Spec007;

public sealed class AC_8Tests
{
    [Fact]
    public void Every_identity_page_design_record_is_mapped()
    {
        Spec007AcceptanceAssertions.Source(
            "src/StudentRegistration.Client/Features/Identity/IdentityRouteStateMapper.cs",
            "AUTH-02",
            "AUTH-03",
            "AUTH-04",
            "AUTH-05",
            "STU-08");
    }
}
