using System.Security.Claims;
using StudentRegistration.IdentityAccess.Application.Authorization;
using StudentRegistration.Registration.Application;
using StudentRegistration.Registration.Domain;

namespace StudentRegistration.ApplicationTests.Specs.Spec014;

public sealed class RegistrationApprovalServiceTests
{
    [Fact]
    public async Task Admin_uses_global_scope_and_staff_uses_assignment_scope()
    {
        var store = new CapturingStore();
        var service = new RegistrationApprovalService(store);

        await service.ListAsync(Principal(RolePolicies.Admin,
            RolePolicies.RegistrationApprovalDecideAll), 1, 20);
        Assert.True(store.LastActor!.DecideAll);
        Assert.Equal(RegistrationApprovalActorRole.Admin, store.LastActor.Role);

        await service.ListAsync(Principal(RolePolicies.Lecturer,
            RolePolicies.RegistrationApprovalDecideAssigned), 1, 20);
        Assert.False(store.LastActor!.DecideAll);
        Assert.Equal(RegistrationApprovalActorRole.Lecturer, store.LastActor.Role);
    }

    [Fact]
    public async Task Missing_permission_or_unsupported_role_is_denied_before_store_lookup()
    {
        var store = new CapturingStore();
        var service = new RegistrationApprovalService(store);

        var missing = await service.ListAsync(Principal(RolePolicies.Admin, null), 1, 20);
        var student = await service.ListAsync(Principal(RolePolicies.Student,
            RolePolicies.RegistrationSubmitOwn), 1, 20);

        Assert.Equal(RegistrationApprovalOutcome.Unauthorized, missing.Outcome);
        Assert.Equal(RegistrationApprovalOutcome.Unauthorized, student.Outcome);
        Assert.Equal(0, store.Calls);
    }

    [Fact]
    public async Task Decision_validation_runs_before_store_lookup()
    {
        var store = new CapturingStore();
        var service = new RegistrationApprovalService(store);
        var result = await service.DecideAsync(
            Principal(RolePolicies.Admin, RolePolicies.RegistrationApprovalDecideAll),
            Guid.Empty,
            Guid.NewGuid(),
            new("approve", "ok", "submission-v1", "line-v1", Guid.NewGuid()));

        Assert.Equal(RegistrationApprovalOutcome.Invalid, result.Outcome);
        Assert.Equal(0, store.Calls);
    }

    private static ClaimsPrincipal Principal(string role, string? permission)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, role)
        };
        if (permission is not null)
        {
            claims.Add(new(RolePolicies.PermissionClaimType, permission));
        }

        return new(new ClaimsIdentity(claims, "test"));
    }

    private sealed class CapturingStore : IRegistrationApprovalStore
    {
        public int Calls { get; private set; }
        public RegistrationApprovalActor? LastActor { get; private set; }

        public Task<RegistrationApprovalResult> ListAsync(
            RegistrationApprovalActor actor, int page, int pageSize,
            CancellationToken cancellationToken = default)
        {
            Calls++;
            LastActor = actor;
            return Task.FromResult(new RegistrationApprovalResult(
                RegistrationApprovalOutcome.Found,
                Page: new([], page, pageSize, 0)));
        }

        public Task<RegistrationApprovalResult> FindAsync(
            RegistrationApprovalActor actor, Guid submissionId, Guid lineId,
            CancellationToken cancellationToken = default)
        {
            Calls++;
            LastActor = actor;
            return Task.FromResult(new RegistrationApprovalResult(
                RegistrationApprovalOutcome.NotFound));
        }

        public Task<RegistrationApprovalResult> DecideAsync(
            RegistrationApprovalActor actor, Guid submissionId, Guid lineId,
            DecideRegistrationLineRequest request,
            CancellationToken cancellationToken = default)
        {
            Calls++;
            LastActor = actor;
            return Task.FromResult(new RegistrationApprovalResult(
                RegistrationApprovalOutcome.NotFound));
        }
    }
}
