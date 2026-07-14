namespace StudentRegistration.AcceptanceTests.Specs.Spec007;

public sealed class AC_6Tests
{
    [Fact]
    public void Parallel_activation_delegates_to_one_conditional_transition()
    {
        var source = Spec007AcceptanceAssertions.Source(
            "src/StudentRegistration.IdentityAccess/Application/StudentActivationService.cs",
            "TryActivateAsync",
            "VerifyHashedPassword",
            "HashPassword",
            "newSecurityStamp",
            "ActivationFailed");
        Assert.Equal(
            1,
            source.Split("TryActivateAsync", StringSplitOptions.None).Length - 1);
    }
}
