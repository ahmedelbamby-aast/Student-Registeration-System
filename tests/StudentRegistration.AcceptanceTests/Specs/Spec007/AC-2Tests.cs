namespace StudentRegistration.AcceptanceTests.Specs.Spec007;

public sealed class AC_2Tests
{
    [Fact]
    public void Unknown_identity_activation_is_generic_and_creates_nothing()
    {
        var source = Spec007AcceptanceAssertions.Source(
            "src/StudentRegistration.IdentityAccess/Application/StudentActivationService.cs",
            "ActivationFailed",
            "NormalizeUniversityId",
            "TryActivateAsync",
            "user ?? _dummyUser",
            "user?.PasswordHash ?? _dummyHash",
            "user?.Id ?? Guid.Empty");
        Assert.DoesNotContain("_store.Add", source, StringComparison.Ordinal);
        Assert.DoesNotContain("_store.Create", source, StringComparison.Ordinal);
    }
}
