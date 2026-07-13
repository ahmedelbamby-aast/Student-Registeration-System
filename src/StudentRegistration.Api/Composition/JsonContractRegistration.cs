using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudentRegistration.Api.Composition;

public static class JsonContractRegistration
{
    public static IServiceCollection AddStudentRegistrationJsonContracts(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.ConfigureHttpJsonOptions(options =>
        {
            var serializer = options.SerializerOptions;
            serializer.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            serializer.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            serializer.PropertyNameCaseInsensitive = true;
            serializer.NumberHandling = JsonNumberHandling.Strict;
            serializer.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
            serializer.Converters.Add(
                new JsonStringEnumConverter(
                    JsonNamingPolicy.CamelCase,
                    allowIntegerValues: false));
        });

        return services;
    }
}
