using System.Reflection;

namespace StudentRegistration.AcceptanceTests.Specs.Spec008;

public sealed class AC_8Tests
{
    [Fact]
    public void Shared_app_context_carries_complete_identity_session_and_academic_values()
    {
        var contracts = Assembly.Load("StudentRegistration.Contracts");
        var contextType = RequiredType(
            contracts,
            "StudentRegistration.Contracts.AppContextDto");
        Assert.Equal(
            [
                "ActiveRole", "AuthorizedRoles", "DisplayName", "ExpiresAtUtc",
                "RegistrationTerm", "RegistrationWindow", "RegistrationWindowState",
                "ServerTimeUtc", "ServiceState", "SessionState", "SupportReferencePath",
                "TeachingTerm", "TimeZoneId"
            ],
            contextType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal));

        var now = new DateTime(2026, 7, 14, 10, 0, 0, DateTimeKind.Utc);
        var teachingTerm = Create(
            contracts,
            "StudentRegistration.Contracts.TermSummaryDto",
            "teaching-term",
            "2026-T",
            "Summer 2026",
            EnumValue(contracts, "StudentRegistration.Contracts.TermState", "Teaching"),
            "teaching-v1");
        var registrationTerm = Create(
            contracts,
            "StudentRegistration.Contracts.TermSummaryDto",
            "registration-term",
            "2026-R",
            "Registration 2026",
            EnumValue(
                contracts,
                "StudentRegistration.Contracts.TermState",
                "RegistrationOpen"),
            "registration-v1");
        var openState = EnumValue(
            contracts,
            "StudentRegistration.Contracts.RegistrationWindowState",
            "Open");
        var window = Create(
            contracts,
            "StudentRegistration.Contracts.RegistrationWindowSummaryDto",
            "window-1",
            openState,
            now.AddHours(-1),
            now.AddHours(2),
            "window-v1");
        var context = Create(
            contracts,
            "StudentRegistration.Contracts.AppContextDto",
            now,
            "Africa/Cairo",
            teachingTerm,
            registrationTerm,
            openState,
            window,
            EnumValue(contracts, "StudentRegistration.Contracts.ServiceState", "Available"),
            "Synthetic Student",
            new[] { "Student" },
            "Student",
            EnumValue(contracts, "StudentRegistration.Contracts.SessionState", "Active"),
            now.AddHours(1),
            "/support");

        Assert.Equal(now, Property(context, "ServerTimeUtc"));
        Assert.Equal("Africa/Cairo", Property(context, "TimeZoneId"));
        Assert.Same(teachingTerm, Property(context, "TeachingTerm"));
        Assert.Same(registrationTerm, Property(context, "RegistrationTerm"));
        Assert.Same(window, Property(context, "RegistrationWindow"));
        Assert.Equal(openState, Property(context, "RegistrationWindowState"));
        Assert.Equal("Synthetic Student", Property(context, "DisplayName"));
        Assert.Equal("Student", Property(context, "ActiveRole"));
    }

    [Fact]
    public void No_registration_context_is_explicit_none_with_a_nullable_window()
    {
        var contracts = Assembly.Load("StudentRegistration.Contracts");
        var now = new DateTime(2026, 7, 14, 10, 0, 0, DateTimeKind.Utc);
        var none = EnumValue(
            contracts,
            "StudentRegistration.Contracts.RegistrationWindowState",
            "None");
        var context = Create(
            contracts,
            "StudentRegistration.Contracts.AppContextDto",
            now,
            "Africa/Cairo",
            null,
            null,
            none,
            null,
            EnumValue(contracts, "StudentRegistration.Contracts.ServiceState", "Available"),
            "Synthetic Student",
            new[] { "Student" },
            "Student",
            EnumValue(contracts, "StudentRegistration.Contracts.SessionState", "Active"),
            now.AddHours(1),
            "/support");

        Assert.Equal(none, Property(context, "RegistrationWindowState"));
        Assert.Null(Property(context, "RegistrationWindow"));
        Assert.Null(Property(context, "RegistrationTerm"));
    }

    [Fact]
    public void Context_handler_composes_server_academics_and_spec007_session_under_context_read()
    {
        var academics = Assembly.Load("StudentRegistration.Academics");
        var api = Assembly.Load("StudentRegistration.Api");
        var resolver = RequiredType(
            academics,
            "StudentRegistration.Academics.Application.AcademicContextResolver");
        var endpoints = RequiredType(
            academics,
            "StudentRegistration.Academics.Endpoints.Spec008Endpoints");
        _ = RequiredType(
            api,
            "StudentRegistration.Api.Composition.AcademicSessionContextAdapter");

        Assert.NotNull(resolver.GetMethod("ResolveAsync", BindingFlags.Instance | BindingFlags.Public));
        Assert.NotNull(
            endpoints.GetMethod("MapSpec008Endpoints", BindingFlags.Static | BindingFlags.Public));

        var resolverSource = RepositoryFiles.Read(
            "src/StudentRegistration.Academics/Application/AcademicContextResolver.cs");
        RepositoryFiles.ContainsAll(
            resolverSource,
            "ServerTimeUtc",
            "TimeZoneId",
            "TeachingTerm",
            "RegistrationTerm",
            "RegistrationWindow",
            "RegistrationWindowState.None",
            "ServiceState");

        var adapterSource = RepositoryFiles.Read(
            "src/StudentRegistration.Api/Composition/AcademicSessionContextAdapter.cs");
        RepositoryFiles.ContainsAll(
            adapterSource,
            "SessionLifecycleService",
            "DisplayName",
            "AuthorizedRoles",
            "ActiveRole",
            "SessionState",
            "ExpiresAtUtc",
            "SupportReferencePath");

        var endpointSource = RepositoryFiles.Read(
            "src/StudentRegistration.Academics/Endpoints/Spec008Endpoints.cs");
        RepositoryFiles.ContainsAll(
            endpointSource,
            "/api/context",
            "Context.Read",
            "AppContextDto");
        Assert.DoesNotContain("browserTime", endpointSource, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("clientTime", endpointSource, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("selectedTermId", endpointSource, StringComparison.OrdinalIgnoreCase);

        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read("specs/008-academic-term-student-profile/contracts/api.md"),
            "No client time/term input",
            "Authenticated + `Context.Read`",
            "no partial success body");
    }

    private static Type RequiredType(Assembly assembly, string fullName)
    {
        var type = assembly.GetType(fullName);
        Assert.True(type is not null, $"Required AC-8 runtime type has not been delivered: {fullName}");
        return type!;
    }

    private static object EnumValue(Assembly assembly, string fullName, string name) =>
        Enum.Parse(RequiredType(assembly, fullName), name);

    private static object Create(Assembly assembly, string fullName, params object?[] arguments)
    {
        var type = RequiredType(assembly, fullName);
        var constructor = Assert.Single(type.GetConstructors(BindingFlags.Instance | BindingFlags.Public));
        return constructor.Invoke(arguments);
    }

    private static object? Property(object instance, string name) =>
        instance.GetType().GetProperty(name)!.GetValue(instance);
}
