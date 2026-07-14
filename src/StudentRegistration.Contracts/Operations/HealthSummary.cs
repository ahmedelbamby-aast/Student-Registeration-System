using System.Text.Json.Serialization;

namespace StudentRegistration.Contracts.Operations;

[JsonConverter(typeof(JsonStringEnumConverter<HealthSummaryStatus>))]
public enum HealthSummaryStatus
{
    [JsonStringEnumMemberName("healthy")]
    Healthy,

    [JsonStringEnumMemberName("degraded")]
    Degraded,

    [JsonStringEnumMemberName("unhealthy")]
    Unhealthy
}

public sealed record HealthSummary
{
    public const int MaxVersionLength = 100;

    public HealthSummary(
        HealthSummaryStatus status,
        string version,
        DateTime timestampUtc)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "A declared health status is required.");
        }

        if (string.IsNullOrWhiteSpace(version))
        {
            throw new ArgumentException(
                "A non-empty version is required.",
                nameof(version));
        }

        if (version.Length > MaxVersionLength || version.Any(char.IsControl))
        {
            throw new ArgumentException(
                $"Version must contain at most {MaxVersionLength} printable characters.",
                nameof(version));
        }

        if (timestampUtc.Kind is not DateTimeKind.Utc)
        {
            throw new ArgumentException(
                "The health timestamp must use UTC.",
                nameof(timestampUtc));
        }

        Status = status;
        Version = version;
        TimestampUtc = timestampUtc;
    }

    public HealthSummaryStatus Status { get; }

    public string Version { get; }

    public DateTime TimestampUtc { get; }
}
