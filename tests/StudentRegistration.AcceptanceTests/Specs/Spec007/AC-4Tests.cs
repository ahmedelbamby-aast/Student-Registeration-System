namespace StudentRegistration.AcceptanceTests.Specs.Spec007;

public sealed class AC_4Tests
{
    [Fact]
    public void Every_identity_mutation_requires_antiforgery()
    {
        var source = Spec007AcceptanceAssertions.Source(
            "src/StudentRegistration.IdentityAccess/Endpoints/Spec007Endpoints.cs",
            "/api/auth/student/login",
            "/api/auth/student/activate",
            "/api/auth/staff/login",
            "/api/auth/logout",
            "/api/auth/recovery/request",
            "/api/auth/recovery/complete",
            "/api/auth/password/change",
            "/api/auth/sessions/revoke-all",
            "/api/auth/session/context");

        Assert.True(
            source.Split("RequireAntiforgery", StringSplitOptions.None).Length - 1 >= 9,
            "Every state-changing identity route must carry antiforgery metadata.");
    }
}
