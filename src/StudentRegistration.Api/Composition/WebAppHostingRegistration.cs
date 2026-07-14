namespace StudentRegistration.Api.Composition;

/// <summary>
/// Hosts the Blazor WebAssembly client from the same origin as the API.
/// </summary>
public static class WebAppHostingRegistration
{
    public static IApplicationBuilder UseStudentRegistrationWebApp(
        this IApplicationBuilder application)
    {
        ArgumentNullException.ThrowIfNull(application);

        application.UseBlazorFrameworkFiles();
        application.UseStaticFiles();
        return application;
    }

    public static IEndpointRouteBuilder MapStudentRegistrationWebAppFallback(
        this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapFallback(
            "/api/{**path}",
            static context =>
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return Task.CompletedTask;
            });
        endpoints.MapFallbackToFile("index.html");
        return endpoints;
    }
}
