using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.Api.Composition;
using StudentRegistration.Contracts;
using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Shared;

public sealed class ApiErrorAndDtoTests
{
    [Fact]
    public void Public_contracts_are_framework_free_and_exclude_persistence_and_credentials()
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
            "Password",
            "PasswordHash",
            "SecurityStamp",
            "ConcurrencyStamp",
            "Navigation",
            "DbContext"
        ];
        var publicMembers = typeof(ApiError).Assembly.ExportedTypes
            .SelectMany(type => type.GetProperties())
            .Select(property => property.Name)
            .ToArray();
        Assert.All(
            forbiddenPublicMembers,
            forbidden => Assert.DoesNotContain(forbidden, publicMembers));
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
