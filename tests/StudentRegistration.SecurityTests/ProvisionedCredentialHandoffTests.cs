using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using StudentRegistration.Api.Composition;
using StudentRegistration.Api.Development;
using StudentRegistration.Contracts.Auditing;
using StudentRegistration.IdentityAccess.Application;
using StudentRegistration.IdentityAccess.Application.Ports;
using StudentRegistration.Infrastructure.SqlServer.Audit;
using StudentRegistration.Infrastructure.SqlServer.Persistence;

namespace StudentRegistration.SecurityTests;

public sealed class ProvisionedCredentialHandoffTests
{
    [Fact]
    public async Task Development_handoff_is_pending_until_commit_complete_and_abort_is_idempotent()
    {
        var contentRoot = Path.Combine(
            Path.GetTempPath(),
            "StudentRegistration-SPEC007",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(contentRoot);
        try
        {
            var environment = new TestHostEnvironment(Environments.Development, contentRoot);
            var handoff = new DevelopmentProvisionedCredentialHandoff(
                environment,
                TimeProvider.System);
            var importId = Guid.NewGuid();
            var credential = CreateCredential("student-1");
            var pendingPath = Path.Combine(
                contentRoot,
                ".local",
                "credentials",
                ".pending",
                $"import-{importId:N}.pending.json");
            var completedPath = Path.Combine(
                contentRoot,
                ".local",
                "credentials",
                "imports",
                $"import-{importId:N}.json");

            await handoff.PrepareAsync(importId, [credential], CancellationToken.None);

            Assert.True(File.Exists(pendingPath));
            Assert.False(File.Exists(completedPath));
            var mismatch = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                handoff.PrepareAsync(
                    importId,
                    [credential with { Secret = CreateCredential("student-1").Secret }],
                    CancellationToken.None));
            Assert.Contains("IDENTITY_HANDOFF_PREPARE_MISMATCH", mismatch.Message);
            Assert.Contains(credential.Secret, await File.ReadAllTextAsync(pendingPath));

            await handoff.CompleteAsync(importId, CancellationToken.None);
            await handoff.CompleteAsync(importId, CancellationToken.None);

            Assert.False(File.Exists(pendingPath));
            Assert.True(File.Exists(completedPath));
            Assert.Contains(credential.Secret, await File.ReadAllTextAsync(completedPath));

            var abortedImportId = Guid.NewGuid();
            await handoff.PrepareAsync(
                abortedImportId,
                [CreateCredential("student-2")],
                CancellationToken.None);
            await handoff.AbortAsync(abortedImportId, CancellationToken.None);
            await handoff.AbortAsync(abortedImportId, CancellationToken.None);

            Assert.False(File.Exists(Path.Combine(
                contentRoot,
                ".local",
                "credentials",
                ".pending",
                $"import-{abortedImportId:N}.pending.json")));
        }
        finally
        {
            var resolved = Path.GetFullPath(contentRoot);
            var expectedRoot = Path.GetFullPath(Path.Combine(
                Path.GetTempPath(),
                "StudentRegistration-SPEC007"));
            Assert.StartsWith(expectedRoot, resolved, StringComparison.OrdinalIgnoreCase);
            if (Directory.Exists(resolved))
            {
                Directory.Delete(resolved, recursive: true);
            }
        }
    }

    [Fact]
    public async Task Testing_handoff_publishes_only_after_complete_and_keeps_published_retry_stable()
    {
        var environment = new TestHostEnvironment(
            "Testing",
            Path.GetTempPath());
        var handoff = new TestingProvisionedCredentialHandoff(environment);
        var importId = Guid.NewGuid();
        var original = CreateCredential("staff-1");

        await handoff.PrepareAsync(importId, [original], CancellationToken.None);
        Assert.False(handoff.TryGetCompleted(importId, out _));
        var mismatch = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handoff.PrepareAsync(
                importId,
                [original with { Secret = CreateCredential("staff-1").Secret }],
                CancellationToken.None));
        Assert.Contains("IDENTITY_HANDOFF_PREPARE_MISMATCH", mismatch.Message);

        await handoff.CompleteAsync(importId, CancellationToken.None);
        await handoff.PrepareAsync(
            importId,
            [CreateCredential("replacement")],
            CancellationToken.None);
        await handoff.CompleteAsync(importId, CancellationToken.None);

        Assert.True(handoff.TryGetCompleted(importId, out var completed));
        Assert.Single(completed);
        Assert.Equal(original, completed[0]);
    }

    [Fact]
    public void Local_handoffs_reject_the_production_environment()
    {
        var environment = new TestHostEnvironment(
            Environments.Production,
            Path.GetTempPath());

        Assert.Throws<InvalidOperationException>(() =>
            new DevelopmentProvisionedCredentialHandoff(environment, TimeProvider.System));
        Assert.Throws<InvalidOperationException>(() =>
            new TestingProvisionedCredentialHandoff(environment));
    }

    [Fact]
    public void Identity_composition_registers_every_scoped_sql_port_and_testing_handoff()
    {
        var services = new ServiceCollection();
        services.AddStudentRegistrationIdentitySecurity(
            new ConfigurationBuilder().Build(),
            new TestHostEnvironment("Testing", Path.GetTempPath()));

        AssertScoped<IIdentityAccountStore, IdentityAccountStore>(services);
        AssertScoped<IIdentitySeedStore, IdentitySeedStore>(services);
        AssertScoped<IAdminUserLifecycleStore, AdminUserLifecycleStore>(services);
        AssertScoped<IIdentityAbuseStateStore, IdentityAbuseStateStore>(services);
        AssertScoped<IAuditEventWriter, AuditTransactionWriter>(services);
        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(IProvisionedCredentialHandoff)
            && descriptor.Lifetime == ServiceLifetime.Singleton);
        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(AdminUserLifecycleService)
            && descriptor.Lifetime == ServiceLifetime.Scoped);
    }

    private static DemoCredential CreateCredential(string loginIdentifier) =>
        new(
            loginIdentifier,
            string.Concat("T3st!", Guid.NewGuid().ToString("N")),
            DateTime.UtcNow);

    private static void AssertScoped<TService, TImplementation>(
        IServiceCollection services)
    {
        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(TService)
            && descriptor.ImplementationType == typeof(TImplementation)
            && descriptor.Lifetime == ServiceLifetime.Scoped);
    }

    private sealed class TestHostEnvironment(
        string environmentName,
        string contentRootPath) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "StudentRegistration.SecurityTests";
        public string ContentRootPath { get; set; } = contentRootPath;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
