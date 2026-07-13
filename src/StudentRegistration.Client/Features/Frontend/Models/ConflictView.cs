using System.Text.Json.Serialization;

namespace StudentRegistration.Client.Features.Frontend.Models;

public sealed class ConflictSubjectGroupView
{
    public ConflictSubjectGroupView(string subjectCode, string subjectName, string groupCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subjectCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(subjectName);
        ArgumentException.ThrowIfNullOrWhiteSpace(groupCode);

        SubjectCode = subjectCode;
        SubjectName = subjectName;
        GroupCode = groupCode;
    }

    public string SubjectCode { get; }

    public string SubjectName { get; }

    public string GroupCode { get; }

    [JsonIgnore]
    public string Reference => $"{SubjectCode}:{GroupCode}";
}

public sealed class ConflictOverlapSlotView
{
    public ConflictOverlapSlotView(
        DayOfWeek day,
        TimeOnly startsAt,
        TimeOnly endsAt,
        IReadOnlyList<string> subjectGroupReferences)
    {
        ArgumentNullException.ThrowIfNull(subjectGroupReferences);

        if (endsAt <= startsAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(endsAt),
                "An overlap must end after it starts.");
        }

        if (subjectGroupReferences.Count < 2
            || subjectGroupReferences.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException(
                "An overlap must identify at least two subject groups.",
                nameof(subjectGroupReferences));
        }

        Day = day;
        StartsAt = startsAt;
        EndsAt = endsAt;
        SubjectGroupReferences = subjectGroupReferences.ToArray();
    }

    [JsonConverter(typeof(JsonStringEnumConverter<DayOfWeek>))]
    public DayOfWeek Day { get; }

    public TimeOnly StartsAt { get; }

    public TimeOnly EndsAt { get; }

    public IReadOnlyList<string> SubjectGroupReferences { get; }
}

public sealed class ConflictAlternativeView
{
    public ConflictAlternativeView(
        string alternativeId,
        string summary,
        IReadOnlyList<string> replacementGroupReferences)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alternativeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(summary);
        ArgumentNullException.ThrowIfNull(replacementGroupReferences);

        if (replacementGroupReferences.Count == 0
            || replacementGroupReferences.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException(
                "An alternative must identify at least one replacement group.",
                nameof(replacementGroupReferences));
        }

        AlternativeId = alternativeId;
        Summary = summary;
        ReplacementGroupReferences = replacementGroupReferences.ToArray();
    }

    public string AlternativeId { get; }

    public string Summary { get; }

    public IReadOnlyList<string> ReplacementGroupReferences { get; }
}

public sealed class ConflictResolutionLinkView
{
    public ConflictResolutionLinkView(string label, string href)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        ArgumentException.ThrowIfNullOrWhiteSpace(href);

        if (!href.StartsWith("/", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Conflict resolution links must be application-relative.",
                nameof(href));
        }

        Label = label;
        Href = href;
    }

    public string Label { get; }

    public string Href { get; }
}

/// <summary>
/// Presentation-only rendering of server-authoritative scheduling conflicts.
/// </summary>
public sealed class ConflictView
{
    public ConflictView(
        IReadOnlyList<ConflictSubjectGroupView> subjectGroups,
        IReadOnlyList<ConflictOverlapSlotView> overlapSlots,
        IReadOnlyList<ConflictAlternativeView> alternatives,
        IReadOnlyList<ConflictResolutionLinkView> resolutionLinks)
    {
        ArgumentNullException.ThrowIfNull(subjectGroups);
        ArgumentNullException.ThrowIfNull(overlapSlots);
        ArgumentNullException.ThrowIfNull(alternatives);
        ArgumentNullException.ThrowIfNull(resolutionLinks);

        if (subjectGroups.Count < 2 || subjectGroups.Any(group => group is null))
        {
            throw new ArgumentException(
                "A conflict must contain at least two subject groups.",
                nameof(subjectGroups));
        }

        if (overlapSlots.Count == 0 || overlapSlots.Any(slot => slot is null))
        {
            throw new ArgumentException(
                "A conflict must contain every overlap slot.",
                nameof(overlapSlots));
        }

        if (alternatives.Any(alternative => alternative is null))
        {
            throw new ArgumentException(
                "Alternatives cannot contain null entries.",
                nameof(alternatives));
        }

        if (resolutionLinks.Count == 0 || resolutionLinks.Any(link => link is null))
        {
            throw new ArgumentException(
                "At least one manual resolution link is required.",
                nameof(resolutionLinks));
        }

        var knownReferences = subjectGroups
            .Select(group => group.Reference)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (overlapSlots
            .SelectMany(slot => slot.SubjectGroupReferences)
            .Any(reference => !knownReferences.Contains(reference)))
        {
            throw new ArgumentException(
                "Every overlap reference must identify a listed subject group.",
                nameof(overlapSlots));
        }

        SubjectGroups = subjectGroups.ToArray();
        OverlapSlots = overlapSlots.ToArray();
        Alternatives = alternatives.ToArray();
        ResolutionLinks = resolutionLinks.ToArray();
    }

    public IReadOnlyList<ConflictSubjectGroupView> SubjectGroups { get; }

    public IReadOnlyList<ConflictOverlapSlotView> OverlapSlots { get; }

    public IReadOnlyList<ConflictAlternativeView> Alternatives { get; }

    public IReadOnlyList<ConflictResolutionLinkView> ResolutionLinks { get; }
}
