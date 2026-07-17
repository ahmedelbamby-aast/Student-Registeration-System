namespace StudentRegistration.IntegrationTests.Specs.Spec017.EdgeCases;

public sealed class EC_5Tests
{
    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Identity_owner_rejects_the_final_admin_removal_and_preserves_one_admin()
    {
        var upstream = new Spec007.AdminUserLifecycleStorePersistenceTests();

        await upstream.Real_sql_admin_lifecycle_is_idempotent_guarded_and_audit_atomic();
    }
}
