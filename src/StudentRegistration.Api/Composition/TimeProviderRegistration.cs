using Microsoft.Extensions.DependencyInjection.Extensions;

namespace StudentRegistration.Api.Composition;

public static class TimeProviderRegistration
{
    public static IServiceCollection AddAuthoritativeTime(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton(TimeProvider.System);
        return services;
    }
}
