using System.Globalization;
using System.Text;
using StudentRegistration.Contracts;
using StudentRegistration.Registration.Domain;

namespace StudentRegistration.Registration.Application;

public sealed record OfferingSearchRequest(
    string? Query,
    string? Eligibility,
    decimal? Credits,
    int? Day,
    string? Availability,
    string? Sort,
    int Page = 1,
    int PageSize = 20);

public enum OfferingSearchOutcome
{
    Found,
    PageSizeInvalid,
    ValidationError,
    ContextNotFound,
    DependencyUnavailable
}

public sealed record OfferingSearchResult(
    OfferingSearchOutcome Outcome,
    Page<OfferingEligibility>? Page = null,
    string? ErrorCode = null,
    DiscoveryMetadata? Metadata = null);

public sealed class OfferingSearchQuery(EligibilityService service)
{
    private const string DefaultSort = "courseCode,id";
    private static readonly HashSet<string> EligibilityFilters =
        new(StringComparer.Ordinal) { "eligible", "unavailable", "all" };
    private static readonly HashSet<string> AvailabilityFilters =
        new(StringComparer.Ordinal) { "available", "full", "unavailable", "all" };
    private static readonly HashSet<string> Sorts =
        new(StringComparer.Ordinal)
        {
            DefaultSort,
            "courseCode-desc,id",
            "title,id",
            "title-desc,id",
            "credits,id",
            "credits-desc,id"
        };

    public async Task<OfferingSearchResult> SearchAsync(
        Guid applicationUserId,
        Guid termId,
        OfferingSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Page < 1 || request.PageSize is < 1 or > 100)
        {
            return new(
                OfferingSearchOutcome.PageSizeInvalid,
                ErrorCode: "PAGE_SIZE_INVALID");
        }

        if (!TryNormalize(request, out var normalized))
        {
            return new(
                OfferingSearchOutcome.ValidationError,
                ErrorCode: "VALIDATION_ERROR");
        }

        var evaluation = await service.EvaluateTermAsync(
            applicationUserId,
            termId,
            cancellationToken);
        if (evaluation.Outcome is EligibilityEvaluationOutcome.ContextNotFound)
        {
            return new(
                OfferingSearchOutcome.ContextNotFound,
                ErrorCode: evaluation.ErrorCode,
                Metadata: evaluation.Metadata);
        }

        if (evaluation.Outcome is EligibilityEvaluationOutcome.DependencyUnavailable)
        {
            return new(
                OfferingSearchOutcome.DependencyUnavailable,
                ErrorCode: evaluation.ErrorCode,
                Metadata: evaluation.Metadata);
        }

        var filtered = Filter(evaluation.Items, normalized);
        var ordered = Sort(filtered, normalized.Sort);
        var total = ordered.Count;
        var skip = (long)(request.Page - 1) * request.PageSize;
        var items = skip >= total
            ? []
            : ordered
                .Skip((int)skip)
                .Take(request.PageSize)
                .ToArray();
        return new(
            OfferingSearchOutcome.Found,
            new Page<OfferingEligibility>(
                items,
                request.Page,
                request.PageSize,
                total,
                normalized.Sort),
            Metadata: evaluation.Metadata);
    }

    private static bool TryNormalize(
        OfferingSearchRequest request,
        out NormalizedSearch normalized)
    {
        string? query = null;
        if (request.Query is not null)
        {
            query = request.Query.Normalize(NormalizationForm.FormKC).Trim();
            if (query.Length is < 1 or > 100)
            {
                normalized = null!;
                return false;
            }
        }

        var eligibility = string.IsNullOrWhiteSpace(request.Eligibility)
            ? "eligible"
            : request.Eligibility.Trim().ToLowerInvariant();
        var availability = string.IsNullOrWhiteSpace(request.Availability)
            ? "all"
            : request.Availability.Trim().ToLowerInvariant();
        var sort = string.IsNullOrWhiteSpace(request.Sort)
            ? DefaultSort
            : request.Sort.Trim();
        if (!EligibilityFilters.Contains(eligibility) ||
            !AvailabilityFilters.Contains(availability) ||
            !Sorts.Contains(sort) ||
            request.Day is < 0 or > 6 ||
            request.Credits is { } credits &&
            (credits is < 0.5m or > 30m || DecimalScale(credits) > 2))
        {
            normalized = null!;
            return false;
        }

        normalized = new(
            query,
            eligibility,
            request.Credits,
            request.Day,
            availability,
            sort);
        return true;
    }

    private static IReadOnlyList<OfferingEligibility> Filter(
        IReadOnlyList<OfferingEligibility> source,
        NormalizedSearch filter) =>
        source.Where(item =>
            (filter.Query is null
                || item.CourseCode.Contains(
                    filter.Query,
                    StringComparison.OrdinalIgnoreCase)
                || item.Title.Contains(
                    filter.Query,
                    StringComparison.OrdinalIgnoreCase))
            && (filter.Eligibility == "all"
                || filter.Eligibility == "eligible" && item.Eligible
                || filter.Eligibility == "unavailable" && !item.Eligible)
            && (filter.Credits is null || item.Credits == filter.Credits)
            && (filter.Day is null || item.Groups.Any(group =>
                group.Meetings.Any(meeting =>
                    (int)meeting.DayOfWeek == filter.Day)))
            && AvailabilityMatches(item, filter.Availability))
            .ToArray();

    private static bool AvailabilityMatches(
        OfferingEligibility item,
        string availability) => availability switch
        {
            "all" => true,
            "available" => item.Groups.Any(group => group.Selectable),
            "full" => IsFull(item.Groups),
            "unavailable" => item.Groups.All(group => !group.Selectable),
            _ => false
        };

    private static bool IsFull(IReadOnlyList<GroupSummary> groups)
    {
        var published = groups.Where(group => group.State == "published").ToArray();
        return published.Length > 0
            && published.All(group => group.SeatsRemaining == 0);
    }

    private static IReadOnlyList<OfferingEligibility> Sort(
        IReadOnlyList<OfferingEligibility> source,
        string sort)
    {
        IOrderedEnumerable<OfferingEligibility> ordered = sort switch
        {
            "courseCode-desc,id" => source.OrderByDescending(
                item => item.CourseCode,
                StringComparer.OrdinalIgnoreCase),
            "title,id" => source.OrderBy(
                item => item.Title,
                StringComparer.OrdinalIgnoreCase),
            "title-desc,id" => source.OrderByDescending(
                item => item.Title,
                StringComparer.OrdinalIgnoreCase),
            "credits,id" => source.OrderBy(item => item.Credits),
            "credits-desc,id" => source.OrderByDescending(item => item.Credits),
            _ => source.OrderBy(
                item => item.CourseCode,
                StringComparer.OrdinalIgnoreCase)
        };
        return ordered.ThenBy(item => item.OfferingId).ToArray();
    }

    private static int DecimalScale(decimal value) =>
        (decimal.GetBits(value)[3] >> 16) & 0x7F;

    private sealed record NormalizedSearch(
        string? Query,
        string Eligibility,
        decimal? Credits,
        int? Day,
        string Availability,
        string Sort);
}
