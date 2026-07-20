using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace StudentRegistration.IdentityAccess.Application.Authorization;

/// <summary>
/// Names and registers the IdentityAccess role and resource policies.
/// </summary>
public static class RolePolicies
{
    public const string Student = "Student";
    public const string Admin = "Admin";
    public const string Lecturer = "Lecturer";
    public const string TeachingAssistant = "TeachingAssistant";

    public const string StaffContext = "StaffContext";
    public const string IdentityManagement = "IdentityManagement";
    public const string OwnStudentResource = "OwnStudentResource";
    public const string AssignedTeachingResource = "AssignedTeachingResource";

    public const string IdentityAccessManage = "IdentityAccess.Manage";
    public const string ContextRead = "Context.Read";
    public const string AcademicProfileReadOwn = "AcademicProfile.ReadOwn";
    public const string AcademicTermsManage = "AcademicTerms.Manage";
    public const string AcademicProfilesManage = "AcademicProfiles.Manage";
    public const string CataloguePolicyManage = "CataloguePolicy.Manage";
    public const string CatalogueReadAvailable = "Catalogue.ReadAvailable";
    public const string OfferingsManage = "Offerings.Manage";
    public const string OfferingDetailsRead = "OfferingDetailsRead";
    public const string RegistrationSubmitOwn = "Registration.SubmitOwn";
    public const string RegistrationRecordsReadOwn = "RegistrationRecords.ReadOwn";
    public const string RegistrationRecordsRead = "RegistrationRecords.Read";
    public const string RegistrationApprovalDecideAll = "RegistrationApproval.DecideAll";
    public const string RegistrationApprovalDecideAssigned = "RegistrationApproval.DecideAssigned";
    public const string AdminOperationsMetricsRead = "AdminOperations.Metrics.Read";
    public const string AdminAuditRead = "AdminAudit.Read";
    public const string AdminAuditExport = "AdminAudit.Export";
    public const string AdminAuditExportReadAll = "AdminAudit.Export.ReadAll";

    public const string PermissionClaimType = "permission";
    public const string AvailableRoleClaimType = "available_role";

    public static IServiceCollection AddIdentityAuthorization(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddAuthorization(Configure);
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IAuthorizationHandler, OwnStudentResourceHandler>());
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IAuthorizationHandler, AssignedTeachingResourceHandler>());
        return services;
    }

    public static void Configure(AuthorizationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        AddRolePolicy(options, Student);
        AddRolePolicy(options, Admin);
        AddRolePolicy(options, Lecturer);
        AddRolePolicy(options, TeachingAssistant);

        options.AddPolicy(
            StaffContext,
            policy => policy
                .RequireAuthenticatedUser()
                .RequireClaim(
                    AvailableRoleClaimType,
                    Admin,
                    Lecturer,
                    TeachingAssistant));

        options.AddPolicy(
            IdentityManagement,
            policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(Admin)
                .RequireClaim(PermissionClaimType, IdentityAccessManage));

        options.AddPolicy(
            ContextRead,
            policy => policy
                .RequireAuthenticatedUser()
                .RequireClaim(PermissionClaimType, ContextRead));

        options.AddPolicy(
            AcademicProfileReadOwn,
            policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(Student)
                .RequireClaim(PermissionClaimType, AcademicProfileReadOwn)
                .AddRequirements(OwnStudentResourceRequirement.Instance));

        options.AddPolicy(
            AcademicTermsManage,
            policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(Admin)
                .RequireClaim(PermissionClaimType, AcademicTermsManage));

        options.AddPolicy(
            AcademicProfilesManage,
            policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(Admin)
                .RequireClaim(PermissionClaimType, AcademicProfilesManage));

        options.AddPolicy(
            CataloguePolicyManage,
            policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(Admin)
                .RequireClaim(PermissionClaimType, CataloguePolicyManage));

        options.AddPolicy(
            CatalogueReadAvailable,
            policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(Student)
                .RequireClaim(PermissionClaimType, CatalogueReadAvailable));

        options.AddPolicy(
            OfferingsManage,
            policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(Admin)
                .RequireClaim(PermissionClaimType, OfferingsManage));

        options.AddPolicy(
            RegistrationSubmitOwn,
            policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(Student)
                .RequireClaim(PermissionClaimType, RegistrationSubmitOwn));

        options.AddPolicy(
            RegistrationRecordsReadOwn,
            policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(Student)
                .RequireClaim(PermissionClaimType, RegistrationRecordsReadOwn));

        options.AddPolicy(
            RegistrationRecordsRead,
            policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(Admin)
                .RequireClaim(PermissionClaimType, RegistrationRecordsRead));

        options.AddPolicy(
            RegistrationApprovalDecideAll,
            policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(Admin)
                .RequireClaim(PermissionClaimType, RegistrationApprovalDecideAll));

        options.AddPolicy(
            RegistrationApprovalDecideAssigned,
            policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(Lecturer, TeachingAssistant)
                .RequireClaim(PermissionClaimType, RegistrationApprovalDecideAssigned));

        AddAdminPermissionPolicy(options, AdminOperationsMetricsRead);
        AddAdminPermissionPolicy(options, AdminAuditRead);
        AddAdminPermissionPolicy(options, AdminAuditExport);
        AddAdminPermissionPolicy(options, AdminAuditExportReadAll);

        options.AddPolicy(
            OfferingDetailsRead,
            policy => policy
                .RequireAuthenticatedUser()
                .RequireAssertion(context =>
                    context.User.IsInRole(Student)
                    && context.User.HasClaim(
                        PermissionClaimType,
                        CatalogueReadAvailable)
                    || context.User.IsInRole(Admin)
                    && context.User.HasClaim(
                        PermissionClaimType,
                        OfferingsManage)));

        options.AddPolicy(
            OwnStudentResource,
            policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(Student)
                .AddRequirements(OwnStudentResourceRequirement.Instance));

        options.AddPolicy(
            AssignedTeachingResource,
            policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(Lecturer, TeachingAssistant)
                .AddRequirements(AssignedTeachingResourceRequirement.Instance));
    }

    public static IReadOnlyList<string> PermissionsForRole(string role) => role switch
    {
        Student =>
        [
            ContextRead,
            AcademicProfileReadOwn,
            CatalogueReadAvailable,
            RegistrationSubmitOwn,
            RegistrationRecordsReadOwn
        ],
        Admin =>
        [
            IdentityAccessManage,
            ContextRead,
            AcademicTermsManage,
            AcademicProfilesManage,
            CataloguePolicyManage,
            OfferingsManage,
            RegistrationRecordsRead,
            RegistrationApprovalDecideAll,
            AdminOperationsMetricsRead,
            AdminAuditRead,
            AdminAuditExport,
            AdminAuditExportReadAll
        ],
        Lecturer or TeachingAssistant => [ContextRead, RegistrationApprovalDecideAssigned],
        _ => []
    };

    private static void AddRolePolicy(AuthorizationOptions options, string role) =>
        options.AddPolicy(
            role,
            policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(role));

    private static void AddAdminPermissionPolicy(
        AuthorizationOptions options,
        string permission) =>
        options.AddPolicy(
            permission,
            policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(Admin)
                .RequireClaim(PermissionClaimType, permission));
}

/// <summary>
/// A resource whose student owner is expressed with the authenticated user ID.
/// </summary>
public interface IStudentOwnedResource
{
    Guid StudentUserId { get; }
}

/// <summary>
/// A teaching resource that can answer whether a staff user is assigned.
/// </summary>
public interface ITeachingAssignedResource
{
    bool IsAssignedTo(Guid staffUserId);
}

public sealed class OwnStudentResourceRequirement : IAuthorizationRequirement
{
    public static OwnStudentResourceRequirement Instance { get; } = new();

    private OwnStudentResourceRequirement()
    {
    }
}

public sealed class AssignedTeachingResourceRequirement : IAuthorizationRequirement
{
    public static AssignedTeachingResourceRequirement Instance { get; } = new();

    private AssignedTeachingResourceRequirement()
    {
    }
}

internal sealed class OwnStudentResourceHandler
    : AuthorizationHandler<OwnStudentResourceRequirement, IStudentOwnedResource>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OwnStudentResourceRequirement requirement,
        IStudentOwnedResource resource)
    {
        if (TryGetUserId(context.User, out var userId)
            && resource.StudentUserId == userId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }

    private static bool TryGetUserId(ClaimsPrincipal principal, out Guid userId) =>
        Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}

internal sealed class AssignedTeachingResourceHandler
    : AuthorizationHandler<AssignedTeachingResourceRequirement, ITeachingAssignedResource>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AssignedTeachingResourceRequirement requirement,
        ITeachingAssignedResource resource)
    {
        if (Guid.TryParse(
                context.User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var userId)
            && resource.IsAssignedTo(userId))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
