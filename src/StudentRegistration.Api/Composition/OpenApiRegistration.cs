using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;

namespace StudentRegistration.Api.Composition;

public static class OpenApiRegistration
{
    public const string DocumentName = "v1";

    public static IServiceCollection AddStudentRegistrationOpenApi(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOpenApi(DocumentName, options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = "Student Registration API",
                    Version = DocumentName
                };
                document.Servers = [];
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes =
                    new Dictionary<string, IOpenApiSecurityScheme>
                    {
                        [IdentitySecurityRegistration.AuthenticationScheme] =
                            new OpenApiSecurityScheme
                            {
                                Type = SecuritySchemeType.ApiKey,
                                In = ParameterLocation.Cookie,
                                Name = IdentitySecurityRegistration.AuthenticationCookieName
                            }
                    };
                return Task.CompletedTask;
            });
            options.AddOperationTransformer((operation, context, _) =>
            {
                var metadata =
                    context.Description.ActionDescriptor.EndpointMetadata;
                if (metadata.OfType<IAllowAnonymous>().Any() ||
                    !metadata.OfType<IAuthorizeData>().Any())
                {
                    return Task.CompletedTask;
                }

                operation.Security ??= [];
                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(
                        IdentitySecurityRegistration.AuthenticationScheme,
                        context.Document)] = []
                });
                return Task.CompletedTask;
            });
        });
        return services;
    }
}
