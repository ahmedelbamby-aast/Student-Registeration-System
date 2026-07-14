using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.Api.Composition;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Identity;
using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Shared;

public sealed class ApiErrorAndDtoTests
{
    [Fact]
    public void Public_contracts_are_framework_free_and_limit_secrets_to_transient_inputs()
    {
        var isolation = RepositoryFiles.Read(
            "specs/006-domain-class-api-contracts/contracts/dto-isolation.md");
        RepositoryFiles.ContainsAll(
            isolation,
            "DTO projection only",
            "EF entities never cross",
            "password",
            "security stamp",
            "rowversion",
            "authorization precedes existence",
            "403",
            "currentVersion");

        var references = typeof(ApiError).Assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();
        Assert.DoesNotContain(references, name =>
            name.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal) ||
            name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal) ||
            name.StartsWith("Microsoft.Data.SqlClient", StringComparison.Ordinal));

        string[] forbiddenPublicMembers =
        [
            "PasswordHash",
            "SecurityStamp",
            "ConcurrencyStamp",
            "Navigation",
            "DbContext",
            "ConnectionString",
            "ConnectionValue",
            "InternalIdentityKey"
        ];
        var publicProperties = typeof(ApiError).Assembly.ExportedTypes
            .SelectMany(type => type.GetProperties().Select(property => (Type: type, Property: property)))
            .ToArray();
        Assert.All(
            forbiddenPublicMembers,
            forbidden => Assert.DoesNotContain(
                publicProperties,
                item => string.Equals(item.Property.Name, forbidden, StringComparison.Ordinal)));

        var approvedTransientSecretInputs = new HashSet<(Type Type, string Property)>
        {
            (typeof(StudentLoginRequest), nameof(StudentLoginRequest.Password)),
            (typeof(StaffLoginRequest), nameof(StaffLoginRequest.Password)),
            (typeof(ActivateStudentRequest), nameof(ActivateStudentRequest.InitialPassword)),
            (typeof(ActivateStudentRequest), nameof(ActivateStudentRequest.NewPassword)),
            (typeof(RecoveryCompleteRequest), nameof(RecoveryCompleteRequest.ChallengeToken)),
            (typeof(RecoveryCompleteRequest), nameof(RecoveryCompleteRequest.NewPassword)),
            (typeof(ChangePasswordRequest), nameof(ChangePasswordRequest.CurrentPassword)),
            (typeof(ChangePasswordRequest), nameof(ChangePasswordRequest.NewPassword))
        };
        var actualSecretInputs = publicProperties
            .Where(item =>
                item.Property.Name.Contains("Password", StringComparison.OrdinalIgnoreCase) ||
                item.Property.Name.Contains("Pin", StringComparison.OrdinalIgnoreCase) ||
                item.Property.Name.Contains("Secret", StringComparison.OrdinalIgnoreCase) ||
                item.Property.Name.Contains("Token", StringComparison.OrdinalIgnoreCase) ||
                item.Property.Name.Contains("Proof", StringComparison.OrdinalIgnoreCase) ||
                item.Property.Name.Contains("Credential", StringComparison.OrdinalIgnoreCase))
            .Select(item => (Type: item.Type, Property: item.Property.Name))
            .ToHashSet();

        Assert.True(
            approvedTransientSecretInputs.SetEquals(actualSecretInputs),
            $"Unexpected secret-bearing public contract members: {string.Join(", ", actualSecretInputs.Except(approvedTransientSecretInputs).Select(item => $"{item.Type.Name}.{item.Property}"))}");
        Assert.All(
            publicProperties.Where(item => approvedTransientSecretInputs.Contains((item.Type, item.Property.Name))),
            item => Assert.Equal(typeof(string), item.Property.PropertyType));
    }

    [Fact]
    public async Task Unexpected_exception_maps_to_only_a_safe_correlated_ApiError()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddStudentRegistrationJsonContracts();
        services.AddSafeApiErrors();
        await using var provider = services.BuildServiceProvider();
        var handler = provider.GetServices<IExceptionHandler>().Single();
        var context = new DefaultHttpContext
        {
            RequestServices = provider,
            TraceIdentifier = "correlation-safe-123"
        };
        context.Response.Body = new MemoryStream();

        var handled = await handler.TryHandleAsync(
            context,
            new InvalidOperationException(
                "SQL SELECT password_hash FROM Users; Server=secret-host"),
            CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        context.Response.Body.Position = 0;
        using var response = await JsonDocument.ParseAsync(context.Response.Body);
        var root = response.RootElement;
        Assert.Equal(3, root.EnumerateObject().Count());
        Assert.Equal("UNEXPECTED_ERROR", root.GetProperty("code").GetString());
        Assert.Equal(
            "An unexpected error occurred. Use the reference when contacting support.",
            root.GetProperty("message").GetString());
        Assert.Equal("correlation-safe-123", root.GetProperty("correlationId").GetString());
        var payload = root.GetRawText();
        Assert.DoesNotContain("SQL", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret-host", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack", payload, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Safe_error_pipeline_has_one_registration_and_one_middleware_seam()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.Api/Composition/ApiErrorHandlingExtensions.cs");

        RepositoryFiles.ContainsAll(
            source,
            "AddSafeApiErrors",
            "UseSafeApiErrors",
            "IExceptionHandler",
            "UNEXPECTED_ERROR",
            "Status500InternalServerError",
            "TraceIdentifier");
        Assert.DoesNotContain("exception.Message", source, StringComparison.Ordinal);
        Assert.DoesNotContain("exception.StackTrace", source, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Fallback_declines_framework_bad_requests_and_aborted_requests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSafeApiErrors();
        await using var provider = services.BuildServiceProvider();
        var handler = provider.GetServices<IExceptionHandler>().Single();

        var badRequestContext = new DefaultHttpContext
        {
            RequestServices = provider
        };
        badRequestContext.Response.Body = new MemoryStream();
        Assert.False(await handler.TryHandleAsync(
            badRequestContext,
            new BadHttpRequestException("Invalid request.", StatusCodes.Status400BadRequest),
            CancellationToken.None));
        Assert.Equal(0, badRequestContext.Response.Body.Length);

        using var requestAbort = new CancellationTokenSource();
        requestAbort.Cancel();
        var abortedContext = new DefaultHttpContext
        {
            RequestServices = provider,
            RequestAborted = requestAbort.Token
        };
        abortedContext.Response.Body = new MemoryStream();
        Assert.False(await handler.TryHandleAsync(
            abortedContext,
            new OperationCanceledException(requestAbort.Token),
            CancellationToken.None));
        Assert.Equal(0, abortedContext.Response.Body.Length);
    }
}
