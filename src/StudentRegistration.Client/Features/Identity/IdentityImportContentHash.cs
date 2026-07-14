using System.Security.Cryptography;
using System.Text.Json;
using StudentRegistration.Contracts.Identity;

namespace StudentRegistration.Client.Features.Identity;

public static class IdentityImportContentHash
{
    public static string Compute(IReadOnlyList<IdentityImportUserRequest> users)
    {
        ArgumentNullException.ThrowIfNull(users);

        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartArray();
            foreach (var row in users)
            {
                ArgumentNullException.ThrowIfNull(row);
                var kind = row.Kind.Trim().ToLowerInvariant();
                writer.WriteStartObject();
                writer.WriteString("externalReference", row.ExternalReference.Trim());
                writer.WriteString("kind", kind);
                WriteOptional(
                    writer,
                    "universityId",
                    kind == "student" ? row.UniversityId?.Trim().ToUpperInvariant() : null);
                WriteOptional(writer, "userName", row.UserName);
                WriteOptional(writer, "staffNumber", row.StaffNumber);
                writer.WriteString("displayName", row.DisplayName.Trim());
                writer.WriteStartArray("roles");
                foreach (var role in row.Roles
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Select(value => value.Trim())
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal))
                {
                    writer.WriteStringValue(role);
                }

                writer.WriteEndArray();
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }

        return $"SHA256:{Convert.ToHexString(SHA256.HashData(buffer.ToArray()))}";
    }

    private static void WriteOptional(Utf8JsonWriter writer, string name, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            writer.WriteNull(name);
            return;
        }

        writer.WriteString(name, value.Trim());
    }
}
